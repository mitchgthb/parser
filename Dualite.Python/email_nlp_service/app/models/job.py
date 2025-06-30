from pydantic import BaseModel, Field
from typing import Dict, Any, List, Optional
from enum import Enum
import datetime

class JobStatus(str, Enum):
    PENDING = "pending"
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"

class EmailData(BaseModel):
    subject: str
    body: str
    sender_email: Optional[str] = None
    sender_name: Optional[str] = None
    sender_company: Optional[str] = None
    recipients: Optional[List[str]] = None
    date: Optional[datetime.datetime] = None
    metadata: Optional[Dict[str, Any]] = None

class JobResponse(BaseModel):
    job_id: str
    status: JobStatus
    message: str
    result: Optional[Dict[str, Any]] = None
    error: Optional[str] = None
    processing_time_ms: Optional[int] = None
    created_at: datetime.datetime = Field(default_factory=datetime.datetime.now)
    completed_at: Optional[datetime.datetime] = None

class EntityExtraction(BaseModel):
    sender_name: Optional[str] = None
    sender_email: Optional[str] = None
    sender_company: Optional[str] = None
    subject_line: Optional[str] = None
    detected_intent: Optional[str] = None
    estimated_effort_minutes: Optional[int] = None
    urgency_score: Optional[float] = None
    extracted_entities: Dict[str, Any] = {}
    confidence_scores: Dict[str, float] = {}
