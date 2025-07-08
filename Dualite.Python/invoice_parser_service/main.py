from fastapi import FastAPI, HTTPException, Depends, BackgroundTasks, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import json
import time
import os
import logging
from typing import Dict, Any, Optional, List

# Import service components
from app.services.invoice_processor import InvoiceProcessor
from app.services.message_queue import MessageQueueService
from app.database.db import get_db, SessionLocal
from app.services.cache import RedisCache
from datetime import datetime
from uuid import uuid4
from app.models.job import JobResponse, InvoiceData, JobStatus

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler("invoice_parser_service.log")
    ]
)
logger = logging.getLogger("invoice-parser-service")

# Initialize FastAPI app
app = FastAPI(
    title="Invoice Parser Service",
    description="Document Processing Platform - Invoice Parser Service",
    version="1.0.0"
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, replace with specific origins
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Initialize services
invoice_processor = InvoiceProcessor()
mq_service = MessageQueueService()
cache = RedisCache()

# -------------------------------------------------------------
# Simple in-memory store to keep track of jobs and their results
# NOTE: This is suitable for single-instance deployments only.
# For production, replace with persistent storage (PostgreSQL/Redis).
# -------------------------------------------------------------

@app.on_event("startup")
async def startup_event():
    logger.info("Starting Invoice Parser Service")
    await mq_service.connect()
    await mq_service.start_consuming(process_job)

@app.on_event("shutdown")
async def shutdown_event():
    logger.info("Shutting down Invoice Parser Service")
    await mq_service.close()

@app.get("/")
def read_root():
    return {"service": "Invoice Parser Service", "status": "running"}

@app.get("/health")
def health_check():
    # Add more sophisticated health checks here
    return {"status": "healthy", "timestamp": time.time()}

@app.post("/parse", response_model=JobResponse)
async def parse_invoice(
    background_tasks: BackgroundTasks,
    file: UploadFile = File(...),
    db=Depends(get_db)
):
    """Parse an invoice PDF directly through the API (for testing and direct usage)"""
    try:
        # Read the uploaded file
        contents = await file.read()
        if not contents:
            raise HTTPException(status_code=400, detail="Empty file")
            
        # Check file type
        if not file.filename.lower().endswith('.pdf'):
            raise HTTPException(status_code=400, detail="Only PDF files are supported")
        
        # Generate a job ID
        job_id = f"invoice_{int(time.time())}_{hash(file.filename)}"[:36]
        
        # Create temp directory if it doesn't exist
        temp_dir = "./temp"
        os.makedirs(temp_dir, exist_ok=True)
        
        # Save the file temporarily
        file_path = f"{temp_dir}/{job_id}.pdf"
        with open(file_path, "wb") as f:
            f.write(contents)
        
        # Add job to background tasks
        background_tasks.add_task(process_invoice_task, job_id, file_path)

        # Persist job in DB
        db_job_data = {
            "id": job_id,
            "client_id": uuid4(),  # TODO: replace with real client id if available
            "job_type": "invoice",
            "status": JobStatus.PROCESSING.value,
            "input_data": {"filename": file.filename},
        }
        job_repo = JobRepository(db)
        await job_repo.create(db_job_data)

        # Build initial response
        job_resp = JobResponse(
            job_id=job_id,
            status=JobStatus.PROCESSING,
            message="Invoice processing job started"
        )

        # cache in Redis (non-blocking failure)
        await cache.cache_invoice_result(job_id, job_resp.dict())
        return job_resp
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.error(f"Error starting invoice processing: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to start processing: {str(e)}")

@app.get("/jobs/{job_id}", response_model=JobResponse)
async def get_job_status(job_id: str, db=Depends(get_db)):
    """Get the status of a processing job"""
    # 1. try redis cache
    cached = await cache.get_invoice_result(job_id)
    if cached:
        return JobResponse(**cached)

    # 2. fallback to DB
    job_repo = JobRepository(db)
    inv_repo = InvoiceExtractionRepository(db)
    db_job = await job_repo.get_by_id(job_id)
    if db_job is None:
        raise HTTPException(status_code=404, detail="Job not found")
    result = None
    if db_job.invoice_extraction:
        result = {
            "invoice_data": db_job.invoice_extraction.extracted_fields or {},
            "processing_metadata": {
                "processing_time_ms": db_job.processing_time_ms
            }
        }
    job_resp = JobResponse(
        job_id=str(db_job.id),
        status=JobStatus(db_job.status),
        message=db_job.error_message or "Job completed successfully" if db_job.status=="completed" else "Job in progress",
        result=result,
        error=db_job.error_message,
        processing_time_ms=db_job.processing_time_ms,
        created_at=db_job.created_at,
        completed_at=db_job.completed_at
    )
    # populate cache
    await cache.cache_invoice_result(job_id, job_resp.dict())
    return job_resp

async def process_invoice_task(job_id: str, file_path: str):
    """Background task to process an invoice"""
    start_ts = time.time()
    try:
        logger.info(f"Processing invoice job {job_id} from {file_path}")
        
        # Process the invoice using the processor
        result = await invoice_processor.process_invoice(file_path)
        
        processing_time_ms = int((time.time() - start_ts) * 1000)
        
        # Update job status in DB
        job_repo = JobRepository(SessionLocal())
        db_job = await job_repo.get_by_id(job_id)
        db_job.status = JobStatus.COMPLETED.value
        db_job.processing_time_ms = processing_time_ms
        db_job.completed_at = datetime.utcnow()
        await job_repo.update(db_job)

        # Update invoice extraction result in DB
        inv_repo = InvoiceExtractionRepository(SessionLocal())
        inv_result = await inv_repo.get_by_job_id(job_id)
        inv_result.extracted_fields = result
        await inv_repo.update(inv_result)

        # Update cache
        job_resp = JobResponse(
            job_id=job_id,
            status=JobStatus.COMPLETED,
            message="Job completed successfully",
            result=result,
            processing_time_ms=processing_time_ms,
            completed_at=datetime.utcnow()
        )
        await cache.cache_invoice_result(job_id, job_resp.dict())

        logger.info(f"Invoice job {job_id} completed successfully in {processing_time_ms} ms")
        
        # Cleanup - remove the temporary file
        try:
            os.remove(file_path)
        except Exception as e:
            logger.warning(f"Failed to remove temporary file {file_path}: {str(e)}")
            
    except Exception as e:
        logger.error(f"Error processing invoice job {job_id}: {str(e)}")
        # Update job status in DB
        job_repo = JobRepository(SessionLocal())
        db_job = await job_repo.get_by_id(job_id)
        db_job.status = JobStatus.FAILED.value
        db_job.error_message = str(e)
        db_job.completed_at = datetime.utcnow()
        await job_repo.update(db_job)

        # Update cache
        job_resp = JobResponse(
            job_id=job_id,
            status=JobStatus.FAILED,
            message="Job failed",
            error=str(e),
            completed_at=datetime.utcnow()
        )
        await cache.cache_invoice_result(job_id, job_resp.dict())

async def process_job(message: Dict[str, Any]):
    """Process a job received from the message queue"""
    try:
        job_id = message.get("job_id")
        file_path = message.get("file_path")
        
        if not file_path or not os.path.exists(file_path):
            logger.error(f"File path {file_path} does not exist for job {job_id}")
            return
            
        logger.info(f"Received invoice job from queue: {job_id}")
        
        # Process the invoice
        await process_invoice_task(job_id, file_path)
    except Exception as e:
        logger.error(f"Error processing job from queue: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
