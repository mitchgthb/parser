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
        
        return JobResponse(
            job_id=job_id,
            status=JobStatus.PROCESSING,
            message="Invoice processing job started"
        )
    except HTTPException as e:
        raise e
    except Exception as e:
        logger.error(f"Error starting invoice processing: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Failed to start processing: {str(e)}")

@app.get("/jobs/{job_id}", response_model=JobResponse)
async def get_job_status(job_id: str, db=Depends(get_db)):
    """Get the status of a processing job"""
    try:
        # Retrieve job from database
        # Here would be database logic to fetch the job status
        # For now, we return a mock response
        return JobResponse(
            job_id=job_id,
            status=JobStatus.COMPLETED,
            message="Job completed successfully"
        )
    except Exception as e:
        logger.error(f"Error retrieving job status: {str(e)}")
        raise HTTPException(status_code=404, detail="Job not found")

async def process_invoice_task(job_id: str, file_path: str):
    """Background task to process an invoice"""
    try:
        logger.info(f"Processing invoice job {job_id} from {file_path}")
        
        # Process the invoice using the processor
        result = await invoice_processor.process_invoice(file_path)
        
        # Update job in database as completed
        # Here would be database logic to update the job
        
        logger.info(f"Invoice job {job_id} completed successfully")
        
        # Cleanup - remove the temporary file
        try:
            os.remove(file_path)
        except Exception as e:
            logger.warning(f"Failed to remove temporary file {file_path}: {str(e)}")
            
    except Exception as e:
        logger.error(f"Error processing invoice job {job_id}: {str(e)}")
        # Update job in database as failed
        # Here would be database logic to update the job

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
