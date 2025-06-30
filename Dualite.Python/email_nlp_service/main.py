from fastapi import FastAPI, HTTPException, Depends, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import json
import time
import os
import logging
from typing import Dict, Any, Optional, List

# Import service components
from app.services.email_intelligence import EmailIntelligenceEngine
from app.services.message_queue import MessageQueueService
from app.database.db import get_db, SessionLocal
from app.models.job import JobResponse, EmailData, JobStatus

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
    handlers=[
        logging.StreamHandler(),
        logging.FileHandler("email_nlp_service.log")
    ]
)
logger = logging.getLogger("email-nlp-service")

# Initialize FastAPI app
app = FastAPI(
    title="Email NLP Service",
    description="Document Processing Platform - Email NLP Processing Service",
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
email_intelligence = EmailIntelligenceEngine()
mq_service = MessageQueueService()

@app.on_event("startup")
async def startup_event():
    logger.info("Starting Email NLP Service")
    await mq_service.connect()
    await mq_service.start_consuming(process_job)

@app.on_event("shutdown")
async def shutdown_event():
    logger.info("Shutting down Email NLP Service")
    await mq_service.close()

@app.get("/")
def read_root():
    return {"service": "Email NLP Service", "status": "running"}

@app.get("/health")
def health_check():
    # Add more sophisticated health checks here
    return {"status": "healthy", "timestamp": time.time()}

@app.post("/process", response_model=JobResponse)
async def process_email(email_data: EmailData, background_tasks: BackgroundTasks, db=Depends(get_db)):
    """Process an email directly through the API (for testing and direct usage)"""
    try:
        # Create a job record
        job_id = f"email_{int(time.time())}_{hash(json.dumps(email_data.dict()))}"[:36]
        
        # Add job to background tasks
        background_tasks.add_task(process_email_task, job_id, email_data)
        
        return JobResponse(
            job_id=job_id,
            status=JobStatus.PROCESSING,
            message="Email processing job started"
        )
    except Exception as e:
        logger.error(f"Error starting email processing: {str(e)}")
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

async def process_email_task(job_id: str, email_data: EmailData):
    """Background task to process an email"""
    try:
        logger.info(f"Processing email job {job_id}")
        
        # Process the email using the NLP engine
        result = await email_intelligence.extract_entities(email_data.dict())
        
        # Update job in database as completed
        # Here would be database logic to update the job
        
        logger.info(f"Email job {job_id} completed successfully")
    except Exception as e:
        logger.error(f"Error processing email job {job_id}: {str(e)}")
        # Update job in database as failed
        # Here would be database logic to update the job

async def process_job(message: Dict[str, Any]):
    """Process a job received from the message queue"""
    try:
        job_id = message.get("job_id")
        data = message.get("data")
        logger.info(f"Received job from queue: {job_id}")
        
        # Convert to EmailData and process
        email_data = EmailData(**data)
        await process_email_task(job_id, email_data)
    except Exception as e:
        logger.error(f"Error processing job from queue: {str(e)}")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("main:app", host="0.0.0.0", port=8000, reload=True)
