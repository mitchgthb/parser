from sqlalchemy.orm import Session
from sqlalchemy.future import select
from typing import Generic, TypeVar, Type, List, Optional, Dict, Any, Union
from uuid import UUID
import logging
from .models import Base, ProcessingJob, EmailExtraction

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Define generic type variable
T = TypeVar('T', bound=Base)

class Repository(Generic[T]):
    """Generic repository for database operations"""
    
    def __init__(self, db: Session, model_class: Type[T]):
        self.db = db
        self.model_class = model_class
    
    async def get_by_id(self, id: Union[str, UUID]) -> Optional[T]:
        """Get an entity by ID"""
        try:
            return self.db.query(self.model_class).filter(self.model_class.id == id).first()
        except Exception as e:
            logger.error(f"Error getting {self.model_class.__name__} by ID {id}: {str(e)}")
            return None
    
    async def get_all(self) -> List[T]:
        """Get all entities"""
        try:
            return self.db.query(self.model_class).all()
        except Exception as e:
            logger.error(f"Error getting all {self.model_class.__name__}: {str(e)}")
            return []
    
    async def create(self, entity: Dict[str, Any]) -> Optional[T]:
        """Create a new entity"""
        try:
            db_entity = self.model_class(**entity)
            self.db.add(db_entity)
            self.db.commit()
            self.db.refresh(db_entity)
            return db_entity
        except Exception as e:
            self.db.rollback()
            logger.error(f"Error creating {self.model_class.__name__}: {str(e)}")
            return None
    
    async def update(self, id: Union[str, UUID], entity: Dict[str, Any]) -> Optional[T]:
        """Update an entity"""
        try:
            db_entity = await self.get_by_id(id)
            if db_entity is None:
                return None
                
            for key, value in entity.items():
                setattr(db_entity, key, value)
                
            self.db.commit()
            self.db.refresh(db_entity)
            return db_entity
        except Exception as e:
            self.db.rollback()
            logger.error(f"Error updating {self.model_class.__name__} with ID {id}: {str(e)}")
            return None
    
    async def delete(self, id: Union[str, UUID]) -> bool:
        """Delete an entity"""
        try:
            db_entity = await self.get_by_id(id)
            if db_entity is None:
                return False
                
            self.db.delete(db_entity)
            self.db.commit()
            return True
        except Exception as e:
            self.db.rollback()
            logger.error(f"Error deleting {self.model_class.__name__} with ID {id}: {str(e)}")
            return False

class JobRepository(Repository[ProcessingJob]):
    """Repository for Processing Jobs"""
    
    def __init__(self, db: Session):
        super().__init__(db, ProcessingJob)
    
    async def get_by_status(self, status: str) -> List[ProcessingJob]:
        """Get jobs by status"""
        try:
            return self.db.query(self.model_class).filter(self.model_class.status == status).all()
        except Exception as e:
            logger.error(f"Error getting jobs with status {status}: {str(e)}")
            return []
    
    async def get_by_client(self, client_id: Union[str, UUID]) -> List[ProcessingJob]:
        """Get jobs by client ID"""
        try:
            return self.db.query(self.model_class).filter(self.model_class.client_id == client_id).all()
        except Exception as e:
            logger.error(f"Error getting jobs for client {client_id}: {str(e)}")
            return []
    
    async def update_status(self, job_id: Union[str, UUID], status: str, error_message: Optional[str] = None) -> Optional[ProcessingJob]:
        """Update job status"""
        try:
            update_data = {"status": status}
            if error_message:
                update_data["error_message"] = error_message
                
            return await self.update(job_id, update_data)
        except Exception as e:
            logger.error(f"Error updating status for job {job_id}: {str(e)}")
            return None

class EmailExtractionRepository(Repository[EmailExtraction]):
    """Repository for Email Extractions"""
    
    def __init__(self, db: Session):
        super().__init__(db, EmailExtraction)
    
    async def get_by_job_id(self, job_id: Union[str, UUID]) -> Optional[EmailExtraction]:
        """Get email extraction by job ID"""
        try:
            return self.db.query(self.model_class).filter(self.model_class.job_id == job_id).first()
        except Exception as e:
            logger.error(f"Error getting email extraction for job {job_id}: {str(e)}")
            return None
