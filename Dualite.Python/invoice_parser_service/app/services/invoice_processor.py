import logging
import time
import os
import json
import re
import tempfile
from typing import Dict, Any, List, Optional, Tuple

import fitz  # PyMuPDF
import pdfplumber
import pytesseract
from PIL import Image, ImageOps

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class InvoiceProcessor:
    """Advanced invoice processing engine using OCR and layout analysis"""
    
    def __init__(self):
        """Initialize the invoice processor with necessary tools and models"""
        try:
            # In a production environment, we would initialize these components:
            # self.ocr_engine = self._setup_ocr()
            # self.layout_analyzer = self._setup_layout_analyzer()
            # self.dutch_validator = DutchTaxValidator()
            logger.info("Invoice processor initialized successfully")
            
        except Exception as e:
            logger.error(f"Error initializing invoice processor: {str(e)}")
    
    def _setup_ocr(self):
        """Set up the OCR engine (Tesseract)"""
        # Placeholder for OCR setup
        # In production, would configure pytesseract
        return None
    
    def _setup_layout_analyzer(self):
        """Set up the layout analysis model"""
        # Placeholder for layout analyzer setup
        # In production, would load a trained LayoutLM model
        return None
    
    async def process_invoice(self, pdf_path: str) -> Dict[str, Any]:
        """Process an invoice PDF file"""
        start_time = time.time()
        
        try:
            # Basic validation
            if not pdf_path or not os.path.exists(pdf_path):
                raise ValueError(f"PDF file not found: {pdf_path}")
            
            # Multi-strategy processing - in a real system these would be implemented
            # 1. First try native PDF text extraction
            text_result = await self._extract_text_native(pdf_path)
            
            # 2. If confidence is low, fall back to OCR
            if text_result.get("confidence", 0) < 0.8:
                ocr_result = await self._extract_text_ocr(pdf_path)
                text_result = self._merge_results(text_result, ocr_result)
            
            # 3. Extract structured data using layout analysis
            structured_data = await self._extract_structured_data(text_result)
            
            # 4. Validate data according to Dutch tax regulations
            validated_data = await self._validate_data(structured_data)
            
            # Prepare response
            processing_time_ms = int((time.time() - start_time) * 1000)
            
            result = {
                "invoice_data": validated_data,
                "processing_metadata": {
                    "method": text_result.get("method", "simulation"),
                    "confidence": text_result.get("confidence", 0.95),
                    "processing_time_ms": processing_time_ms
                }
            }
            
            logger.info(f"Invoice processed successfully in {processing_time_ms}ms")
            return result
            
        except Exception as e:
            logger.error(f"Error processing invoice: {str(e)}")
            processing_time = int((time.time() - start_time) * 1000)
            return {
                "error": str(e),
                "processing_time_ms": processing_time
            }
    
    async def _extract_text_native(self, pdf_path: str) -> Dict[str, Any]:
        """Extract embedded text from PDF using PyMuPDF (fast path)."""
        start_ts = time.time()
        try:
            text_chunks: List[str] = []
            with fitz.open(pdf_path) as doc:
                for page in doc:
                    text_chunks.append(page.get_text("text"))
            full_text = "\n".join(text_chunks)
            # Crude confidence: if we extracted >100 chars we assume it's a text PDF.
            confidence = 1.0 if len(full_text.strip()) > 100 else 0.4
            return {
                "text": full_text,
                "method": "native_pdf",
                "confidence": confidence,
                "pages": len(text_chunks),
                "processing_time_ms": int((time.time() - start_ts) * 1000)
            }
        except Exception as e:
            logger.error(f"Error in native PDF extraction: {str(e)}")
            return {
                "text": "",
                "method": "native_pdf",
                "confidence": 0.0,
                "error": str(e)
            }
    
    async def _extract_text_ocr(self, pdf_path: str) -> Dict[str, Any]:
        """Extract text from PDF by rasterising pages and running Tesseract OCR."""
        start_ts = time.time()
        try:
            text_pages: List[str] = []
            ocr_confidences: List[float] = []
            with pdfplumber.open(pdf_path) as pdf:
                for page in pdf.pages:
                    page_image = page.to_image(resolution=300)  # PIL Image
                    img: Image.Image = page_image.original
                    # enhance contrast, convert to grayscale for better OCR
                    img = ImageOps.grayscale(img)
                    ocr_result = pytesseract.image_to_string(img, lang="eng")
                    text_pages.append(ocr_result)
                    # Tesseract doesn't expose page confidence easily; use heuristic length
                    ocr_confidences.append(0.7 if len(ocr_result.strip()) > 50 else 0.4)
            full_text = "\n".join(text_pages)
            avg_conf = sum(ocr_confidences) / len(ocr_confidences) if ocr_confidences else 0.5
            return {
                "text": full_text,
                "method": "ocr",
                "confidence": avg_conf,
                "pages": len(text_pages),
                "processing_time_ms": int((time.time() - start_ts) * 1000)
            }
        except Exception as e:
            logger.error(f"Error in OCR extraction: {str(e)}")
            return {
                "text": "",
                "method": "ocr",
                "confidence": 0.0,
                "error": str(e)
            }
    
    def _merge_results(self, result1: Dict[str, Any], result2: Dict[str, Any]) -> Dict[str, Any]:
        """Merge results from multiple extraction methods"""
        # Choose the result with higher confidence
        if result1.get("confidence", 0) >= result2.get("confidence", 0):
            return result1
        return result2
    
    async def _extract_structured_data(self, text_result: Dict[str, Any]) -> Dict[str, Any]:
        """Extract structured data from text using regex and layout analysis"""
        try:
            text = text_result.get("text", "")
            
            # Very basic extraction using regex patterns
            # In production, use more sophisticated NLP and layout analysis
            
            # Extract invoice number
            invoice_number_match = re.search(r'Invoice Number:\s*([\w-]+)', text)
            invoice_number = invoice_number_match.group(1) if invoice_number_match else None
            
            # Extract date
            date_match = re.search(r'Date:\s*(\d{1,2}-\d{1,2}-\d{4})', text)
            invoice_date = date_match.group(1) if date_match else None
            
            # Extract seller information
            seller_name_match = re.search(r'From:\s*([^\n]+)', text)
            seller_name = seller_name_match.group(1) if seller_name_match else None
            
            seller_kvk_match = re.search(r'KvK:\s*(\d{8})', text)
            seller_kvk = seller_kvk_match.group(1) if seller_kvk_match else None
            
            seller_iban_match = re.search(r'IBAN:\s*([A-Z0-9]+)', text)
            seller_iban = seller_iban_match.group(1) if seller_iban_match else None
            
            # Extract buyer information
            buyer_name_match = re.search(r'To:\s*([^\n]+)', text)
            buyer_name = buyer_name_match.group(1) if buyer_name_match else None
            
            buyer_kvk_match = re.search(r'KvK:\s*(\d{8})', text)
            buyer_kvk = buyer_kvk_match.group(1) if buyer_kvk_match else None
            
            # Extract amounts
            total_match = re.search(r'Total:\s*€(\d+\.\d{2})', text)
            total_amount = float(total_match.group(1)) if total_match else None
            
            vat_match = re.search(r'VAT\s*\((\d+)%\):\s*€(\d+\.\d{2})', text)
            vat_rate = float(vat_match.group(1)) if vat_match else None
            vat_amount = float(vat_match.group(2)) if vat_match else None
            
            # Extract line items
            line_items = []
            for line in text.split('\n'):
                if '€' in line and 'x' in line and '=' in line:
                    parts = line.split('-')
                    if len(parts) >= 2:
                        product = parts[0].strip()
                        price_match = re.search(r'€(\d+\.\d{2})\s*x\s*(\d+)\s*=\s*€(\d+\.\d{2})', parts[1])
                        if price_match:
                            unit_price = float(price_match.group(1))
                            quantity = int(price_match.group(2))
                            total = float(price_match.group(3))
                            line_items.append({
                                "description": product,
                                "unit_price": unit_price,
                                "quantity": quantity,
                                "total": total
                            })
            
            # Structured data result
            return {
                "invoice_number": invoice_number,
                "invoice_date": invoice_date,
                "seller_name": seller_name,
                "seller_kvk": seller_kvk,
                "seller_iban": seller_iban,
                "buyer_name": buyer_name,
                "buyer_kvk": buyer_kvk,
                "total_amount": total_amount,
                "vat_amount": vat_amount,
                "vat_rate": vat_rate,
                "currency": "EUR",
                "line_items": line_items
            }
            
        except Exception as e:
            logger.error(f"Error extracting structured data: {str(e)}")
            return {}
    
    async def _validate_data(self, data: Dict[str, Any]) -> Dict[str, Any]:
        """Validate the extracted data against business rules"""
        # In production, implement validation according to Dutch tax regulations
        # For demo purposes, we'll just add a validation status
        
        validation_status = "valid"
        validation_messages = []
        
        # Check required fields for Dutch invoices
        if not data.get("invoice_number"):
            validation_status = "invalid"
            validation_messages.append("Missing invoice number")
            
        if not data.get("invoice_date"):
            validation_status = "invalid"
            validation_messages.append("Missing invoice date")
            
        if not data.get("seller_name"):
            validation_status = "invalid"
            validation_messages.append("Missing seller name")
            
        if not data.get("seller_kvk"):
            validation_status = "warning"
            validation_messages.append("Missing seller KVK number")
            
        if not data.get("total_amount"):
            validation_status = "invalid"
            validation_messages.append("Missing total amount")
            
        if not data.get("vat_amount") and data.get("total_amount", 0) > 0:
            validation_status = "warning"
            validation_messages.append("Missing VAT amount")
            
        # Add validation results to the data
        data["validation_status"] = validation_status
        data["validation_messages"] = validation_messages
        
        return data
