import redis
import json
import logging
import os
from typing import Dict, Any, Optional, Union

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class RedisCache:
    """Service for interacting with Redis cache"""
    
    def __init__(self):
        """Initialize the Redis cache service"""
        # Get configuration from environment variables or use defaults
        redis_url = os.getenv("REDIS_URL", "redis://localhost:6379/0")
        self.ttl = int(os.getenv("REDIS_DEFAULT_TTL", "3600"))  # Default TTL: 1 hour
        
        try:
            self.redis = redis.from_url(redis_url)
            self.connected = True
            logger.info(f"Connected to Redis at {redis_url}")
        except Exception as e:
            logger.error(f"Failed to connect to Redis: {str(e)}")
            self.connected = False
            
    async def get(self, key: str) -> Optional[Any]:
        """Get a value from cache"""
        try:
            if not self.connected:
                return None
                
            value = self.redis.get(key)
            if value is None:
                return None
                
            return json.loads(value)
        except Exception as e:
            logger.error(f"Error retrieving from cache: {str(e)}")
            return None
            
    async def set(self, key: str, value: Any, ttl: Optional[int] = None) -> bool:
        """Set a value in cache"""
        try:
            if not self.connected:
                return False
                
            # Use provided TTL or default
            expiry = ttl if ttl is not None else self.ttl
            
            # Convert value to JSON string
            json_value = json.dumps(value)
            
            # Set in Redis with expiry
            self.redis.setex(key, expiry, json_value)
            
            return True
        except Exception as e:
            logger.error(f"Error setting cache value: {str(e)}")
            return False
            
    async def delete(self, key: str) -> bool:
        """Delete a value from cache"""
        try:
            if not self.connected:
                return False
                
            self.redis.delete(key)
            return True
        except Exception as e:
            logger.error(f"Error deleting from cache: {str(e)}")
            return False
            
    async def exists(self, key: str) -> bool:
        """Check if a key exists in cache"""
        try:
            if not self.connected:
                return False
                
            return self.redis.exists(key) > 0
        except Exception as e:
            logger.error(f"Error checking cache key: {str(e)}")
            return False
            
    async def increment(self, key: str, amount: int = 1) -> Optional[int]:
        """Increment a counter in cache"""
        try:
            if not self.connected:
                return None
                
            return self.redis.incrby(key, amount)
        except Exception as e:
            logger.error(f"Error incrementing cache value: {str(e)}")
            return None
    
    async def set_rate_limit(self, key: str, limit: int, window_seconds: int) -> bool:
        """Set up a rate limiter using Redis"""
        try:
            if not self.connected:
                return False
                
            current = self.redis.get(key)
            if current is None:
                # Key doesn't exist, initialize rate limit
                self.redis.setex(key, window_seconds, 1)
                return True
            else:
                # Key exists, check if limit is reached
                count = int(current)
                if count < limit:
                    # Increment and return
                    self.redis.incr(key)
                    return True
                else:
                    # Rate limit exceeded
                    return False
        except Exception as e:
            logger.error(f"Error in rate limit logic: {str(e)}")
            return False
            
    async def clear_expired_keys(self, pattern: str) -> int:
        """Clear expired keys matching pattern (useful for maintenance)"""
        try:
            if not self.connected:
                return 0
                
            # Find keys matching pattern
            keys = self.redis.keys(pattern)
            if not keys:
                return 0
                
            # Check TTL for each key and delete those with negative TTL (expired)
            expired_count = 0
            for key in keys:
                ttl = self.redis.ttl(key)
                if ttl < 0:
                    self.redis.delete(key)
                    expired_count += 1
                    
            return expired_count
        except Exception as e:
            logger.error(f"Error clearing expired keys: {str(e)}")
            return 0
