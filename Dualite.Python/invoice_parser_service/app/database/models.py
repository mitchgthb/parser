from sqlalchemy import Column, Integer, String, Float, DateTime, Text, Boolean, ForeignKey, JSON, Date, Numeric
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
    metadata = Column(JSONB, default={})
    
    # Relationship to Invoice Extraction
    invoice_extraction = relationship("InvoiceExtraction", back_populates="job", uselist=False, cascade="all, delete-orphan")

class InvoiceExtraction(Base):
    __tablename__ = "invoice_extractions"
    __table_args__ = {"schema": "dualite"}
    
    id = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    job_id = Column(UUID(as_uuid=True), ForeignKey("dualite.processing_jobs.id", ondelete="CASCADE"), nullable=False)
    invoice_number = Column(String(100), nullable=True)
    invoice_date = Column(Date, nullable=True)
    seller_name = Column(String(255), nullable=True)
    seller_kvk = Column(String(20), nullable=True)
    seller_iban = Column(String(34), nullable=True)
    buyer_name = Column(String(255), nullable=True)
    buyer_kvk = Column(String(20), nullable=True)
    total_amount = Column(Numeric(15, 2), nullable=True)
    vat_amount = Column(Numeric(15, 2), nullable=True)
    vat_rate = Column(Numeric(5, 2), nullable=True)
    currency = Column(String(3), default="EUR")
    line_items = Column(JSONB, default={})
    extracted_at = Column(DateTime, default=datetime.utcnow)
    validation_status = Column(String(50), default="pending")
    validation_messages = Column(JSONB, default=[])
    extracted_fields = Column(JSONB, default={})
    confidence_scores = Column(JSONB, default={})
    
    # Relationship to Processing Job
    job = relationship("ProcessingJob", back_populates="invoice_extraction")
