from sqlalchemy import Column, Integer, String, Float, DateTime, Text, Boolean, ForeignKey, JSON, ARRAY
from sqlalchemy.dialects.postgresql import UUID, JSONB
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import relationship
import uuid
from datetime import datetime
from .db import Base

class ProcessingJob(Base):
    __tablename__ = "processing_jobs"
    __table_args__ = {"schema": "dualite"}
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    client_id = Column(UUID(as_uuid=True), nullable=False)
    job_type = Column(String(50), nullable=False)
    status = Column(String(50), default="pending")
    created_at = Column(DateTime, default=datetime.utcnow)
    updated_at = Column(DateTime, default=datetime.utcnow, onupdate=datetime.utcnow)
    completed_at = Column(DateTime, nullable=True)
    input_data = Column(JSONB, default={})
    output_data = Column(JSONB, default={})
    error_message = Column(Text, nullable=True)
    priority = Column(Integer, default=0)
    processing_time_ms = Column(Integer, nullable=True)
    meta = Column(JSONB, default={})
    
    # Relationship to Email Extraction
    email_extraction = relationship("EmailExtraction", back_populates="job", uselist=False, cascade="all, delete-orphan")

class EmailExtraction(Base):
    __tablename__ = "email_extractions"
    __table_args__ = {"schema": "dualite"}
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    job_id = Column(UUID(as_uuid=True), ForeignKey("dualite.processing_jobs.id", ondelete="CASCADE"), nullable=False)
    subject = Column(String(500), nullable=True)
    sender_email = Column(String(255), nullable=True)
    sender_name = Column(String(255), nullable=True)
    recipient_emails = Column(JSONB, default=[])
    cc_emails = Column(JSONB, default=[])
    body_text = Column(Text, nullable=True)
    entities = Column(JSONB, default={})
    intent = Column(String(100), nullable=True)
    intent_confidence = Column(Float, nullable=True)
    urgency_score = Column(Float, nullable=True)
    effort_estimate = Column(String(50), nullable=True)
    extracted_at = Column(DateTime, default=datetime.utcnow)
    extracted_fields = Column(JSONB, default={})
    
    # Relationship to Processing Job
    job = relationship("ProcessingJob", back_populates="email_extraction")
