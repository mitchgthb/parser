from pydantic import BaseModel, Field
from typing import Dict, Any, List, Optional
from enum import Enum
import datetime

class JobStatus(str, Enum):
    PENDING = "pending"
    PROCESSING = "processing"
    COMPLETED = "completed"
    FAILED = "failed"

class InvoiceData(BaseModel):
    invoice_number: Optional[str] = None
    invoice_date: Optional[str] = None
    seller_name: Optional[str] = None
    seller_kvk: Optional[str] = None
    seller_iban: Optional[str] = None
    buyer_name: Optional[str] = None
    buyer_kvk: Optional[str] = None
    total_amount: Optional[float] = None
    vat_amount: Optional[float] = None
    vat_rate: Optional[float] = None
    currency: str = "EUR"
    line_items: Optional[List[Dict[str, Any]]] = None
    meta: Optional[Dict[str, Any]] = None

class JobResponse(BaseModel):
    job_id: str
    status: JobStatus
    message: str
    result: Optional[Dict[str, Any]] = None
    error: Optional[str] = None
    processing_time_ms: Optional[int] = None
    created_at: datetime.datetime = Field(default_factory=datetime.datetime.now)
    completed_at: Optional[datetime.datetime] = None

class InvoiceExtraction(BaseModel):
    invoice_number: Optional[str] = None
    invoice_date: Optional[str] = None
    total_amount: Optional[float] = None
    vat_amount: Optional[float] = None
    vat_rate: Optional[float] = None
    currency: str = "EUR"
    seller_name: Optional[str] = None
    seller_kvk: Optional[str] = None
    seller_iban: Optional[str] = None
    buyer_name: Optional[str] = None
    buyer_kvk: Optional[str] = None
    line_items: Optional[List[Dict[str, Any]]] = None
    extracted_fields: Optional[Dict[str, Any]] = None
    validation_status: str = "pending"
    validation_messages: List[str] = []
    confidence_scores: Dict[str, float] = {}
