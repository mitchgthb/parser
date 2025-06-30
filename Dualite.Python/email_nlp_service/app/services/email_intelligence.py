import spacy
import logging
import json
import time
from typing import Dict, Any, List, Optional
import asyncio

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class EmailIntelligenceEngine:
    """Advanced email processing engine using NLP techniques"""
    
    def __init__(self):
        """Initialize the email intelligence engine with NLP models"""
        try:
            # Load spaCy model - we'll use a simpler one for demo purposes
            # In production, use a larger model like 'en_core_web_lg' or custom trained model
            self.nlp = spacy.load("en_core_web_sm")
            logger.info("NLP model loaded successfully")
            
            # In a real implementation, we would load trained models for:
            # - Intent classification
            # - Effort estimation
            # - Urgency detection
            # self.intent_classifier = self._load_model("intent_model.pkl")
            # self.effort_estimator = self._load_model("effort_model.pkl")
            # self.urgency_detector = self._load_model("urgency_model.pkl")
            
        except Exception as e:
            logger.error(f"Error initializing NLP models: {str(e)}")
            # Fall back to minimal functionality
            self.nlp = None
    
    def _load_model(self, model_path: str) -> Any:
        """Load a trained ML model from file"""
        # Placeholder for model loading logic
        logger.info(f"Loading model from {model_path}")
        return None  # Replace with actual model loading
    
    async def extract_entities(self, email_data: Dict[str, Any]) -> Dict[str, Any]:
        """Extract entities and insights from an email"""
        start_time = time.time()
        
        try:
            # Basic validation
            if 'body' not in email_data or not email_data['body']:
                raise ValueError("Email body is required")
                
            if 'subject' not in email_data or not email_data['subject']:
                logger.warning("Email subject is missing")
                email_data['subject'] = ""
                
            # Extract basic entities
            entities = await self.extract_basic_entities(email_data)
            
            # Advanced processing
            intent = await self.classify_intent(email_data)
            effort = await self.estimate_effort(email_data, intent)
            urgency = await self.calculate_urgency(email_data)
            
            # Combine results
            result = {
                "entities": entities,
                "intent": intent,
                "estimated_effort_minutes": effort,
                "urgency_score": urgency,
                "confidence_scores": self.get_confidence_metrics(),
                "processing_time_ms": int((time.time() - start_time) * 1000)
            }
            
            logger.info(f"Email processed successfully in {result['processing_time_ms']}ms")
            return result
            
        except Exception as e:
            logger.error(f"Error extracting email entities: {str(e)}")
            processing_time = int((time.time() - start_time) * 1000)
            return {
                "error": str(e),
                "processing_time_ms": processing_time
            }
    
    async def extract_basic_entities(self, email_data: Dict[str, Any]) -> Dict[str, Any]:
        """Extract basic named entities from email content"""
        if not self.nlp:
            return {"error": "NLP model not available"}
            
        try:
            # Combine subject and body for processing
            text = f"{email_data['subject']}. {email_data['body']}"
            
            # Process with spaCy
            doc = self.nlp(text)
            
            # Extract named entities
            entities = {
                "persons": [],
                "organizations": [],
                "dates": [],
                "times": [],
                "money": [],
                "locations": [],
                "emails": [],
                "urls": [],
            }
            
            # Group entities by type
            for ent in doc.ents:
                if ent.label_ == "PERSON":
                    entities["persons"].append(ent.text)
                elif ent.label_ == "ORG":
                    entities["organizations"].append(ent.text)
                elif ent.label_ == "DATE":
                    entities["dates"].append(ent.text)
                elif ent.label_ == "TIME":
                    entities["times"].append(ent.text)
                elif ent.label_ == "MONEY":
                    entities["money"].append(ent.text)
                elif ent.label_ in ["GPE", "LOC"]:
                    entities["locations"].append(ent.text)
            
            # Extract emails and URLs with regex (simplified for demo)
            # In production, use more robust extraction methods
            
            # Remove duplicates
            for key in entities:
                entities[key] = list(set(entities[key]))
                
            return entities
            
        except Exception as e:
            logger.error(f"Error in basic entity extraction: {str(e)}")
            return {"error": str(e)}
    
    async def classify_intent(self, email_data: Dict[str, Any]) -> str:
        """Classify the intent of the email"""
        # Simplified intent detection for demo
        # In production, use a trained classifier
        
        text = email_data['subject'] + " " + email_data['body']
        text = text.lower()
        
        # Very basic keyword matching
        if any(word in text for word in ["urgent", "immediately", "asap", "emergency"]):
            return "urgent_request"
        elif any(word in text for word in ["help", "support", "assist", "guidance"]):
            return "support_request"
        elif any(word in text for word in ["invoice", "payment", "bill", "quote", "price"]):
            return "billing_inquiry"
        elif any(word in text for word in ["meeting", "call", "appointment", "schedule"]):
            return "meeting_request"
        elif any(word in text for word in ["thank", "appreciate", "grateful", "thanks"]):
            return "appreciation"
        elif any(word in text for word in ["feedback", "review", "opinion", "suggestion"]):
            return "feedback"
        else:
            return "general_inquiry"
    
    async def estimate_effort(self, email_data: Dict[str, Any], intent: str) -> int:
        """Estimate the effort required to handle this email in minutes"""
        # Simplified effort estimation for demo
        # In production, use a trained model
        
        text_length = len(email_data['body'])
        
        # Very basic heuristic
        if intent == "urgent_request":
            return 30
        elif intent == "support_request":
            return 20
        elif intent == "billing_inquiry":
            return 15
        elif intent == "meeting_request":
            return 10
        elif text_length > 1000:
            return 25
        elif text_length > 500:
            return 15
        else:
            return 10
    
    async def calculate_urgency(self, email_data: Dict[str, Any]) -> float:
        """Calculate an urgency score for the email (0.0-1.0)"""
        # Simplified urgency detection for demo
        # In production, use a trained model
        
        text = email_data['subject'] + " " + email_data['body']
        text = text.lower()
        
        # Basic keyword scoring
        urgency_score = 0.5  # Default medium urgency
        
        if "urgent" in text or "emergency" in text or "asap" in text:
            urgency_score += 0.4
        if "important" in text or "priority" in text:
            urgency_score += 0.2
        if "tomorrow" in text or "today" in text:
            urgency_score += 0.2
        if "when possible" in text or "no rush" in text:
            urgency_score -= 0.3
        
        # Cap between 0.0 and 1.0
        return max(0.0, min(1.0, urgency_score))
    
    def get_confidence_metrics(self) -> Dict[str, float]:
        """Return confidence scores for the predictions"""
        # In a production system, these would come from the models
        return {
            "entity_extraction": 0.85,
            "intent_classification": 0.78,
            "effort_estimation": 0.65,
            "urgency_detection": 0.72
        }
