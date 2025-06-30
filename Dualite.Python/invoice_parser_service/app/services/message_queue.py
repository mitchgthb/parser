import pika
import json
import logging
import asyncio
from typing import Dict, Any, Callable, Optional
import os

# Configure logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

class MessageQueueService:
    """Service for interacting with RabbitMQ message queue"""
    
    def __init__(self):
        """Initialize the message queue service"""
        # Get configuration from environment variables or use defaults
        self.host = os.getenv("RABBITMQ_HOST", "localhost")
        self.port = int(os.getenv("RABBITMQ_PORT", "5672"))
        self.username = os.getenv("RABBITMQ_USERNAME", "guest")
        self.password = os.getenv("RABBITMQ_PASSWORD", "guest")
        self.queue_name = os.getenv("RABBITMQ_QUEUE", "invoice_processing")
        
        # Initialize connection variables
        self.connection = None
        self.channel = None
        self.connected = False
        
    async def connect(self) -> bool:
        """Establish connection to RabbitMQ server"""
        try:
            # Use a connection parameters object
            credentials = pika.PlainCredentials(self.username, self.password)
            parameters = pika.ConnectionParameters(
                host=self.host,
                port=self.port,
                credentials=credentials,
                heartbeat=600,
                blocked_connection_timeout=300
            )
            
            # Connect to RabbitMQ
            self.connection = pika.BlockingConnection(parameters)
            self.channel = self.connection.channel()
            
            # Declare the queue
            self.channel.queue_declare(queue=self.queue_name, durable=True)
            
            self.connected = True
            logger.info(f"Connected to RabbitMQ at {self.host}:{self.port}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to connect to RabbitMQ: {str(e)}")
            self.connected = False
            return False
    
    async def publish(self, message: Dict[str, Any]) -> bool:
        """Publish a message to the queue"""
        try:
            if not self.connected:
                await self.connect()
                
            if not self.connected:
                raise ConnectionError("Not connected to RabbitMQ")
                
            # Convert message to JSON
            message_body = json.dumps(message).encode()
            
            # Publish message
            self.channel.basic_publish(
                exchange='',
                routing_key=self.queue_name,
                body=message_body,
                properties=pika.BasicProperties(
                    delivery_mode=2,  # make message persistent
                    content_type='application/json'
                )
            )
            
            logger.info(f"Published message to queue {self.queue_name}")
            return True
            
        except Exception as e:
            logger.error(f"Failed to publish message: {str(e)}")
            self.connected = False
            return False
    
    async def start_consuming(self, callback: Callable[[Dict[str, Any]], None]) -> None:
        """Start consuming messages from the queue"""
        try:
            if not self.connected:
                await self.connect()
                
            if not self.connected:
                raise ConnectionError("Not connected to RabbitMQ")
                
            # Define the callback wrapper
            def message_callback(ch, method, properties, body):
                try:
                    # Parse JSON message
                    message = json.loads(body)
                    
                    # Process message using the provided callback
                    asyncio.run(callback(message))
                    
                    # Acknowledge the message
                    ch.basic_ack(delivery_tag=method.delivery_tag)
                    
                except Exception as e:
                    logger.error(f"Error processing message: {str(e)}")
                    # Negative acknowledgment, requeue the message
                    ch.basic_nack(delivery_tag=method.delivery_tag, requeue=True)
            
            # Set prefetch count
            self.channel.basic_qos(prefetch_count=1)
            
            # Start consuming
            self.channel.basic_consume(
                queue=self.queue_name,
                on_message_callback=message_callback
            )
            
            logger.info(f"Started consuming from queue {self.queue_name}")
            
            # Start consuming in a separate thread
            import threading
            consume_thread = threading.Thread(
                target=self.channel.start_consuming,
                daemon=True
            )
            consume_thread.start()
            
        except Exception as e:
            logger.error(f"Failed to start consuming: {str(e)}")
            self.connected = False
    
    async def close(self) -> None:
        """Close the connection to RabbitMQ"""
        try:
            if self.channel and self.channel.is_open:
                self.channel.stop_consuming()
                self.channel.close()
                
            if self.connection and self.connection.is_open:
                self.connection.close()
                
            self.connected = False
            logger.info("Closed connection to RabbitMQ")
            
        except Exception as e:
            logger.error(f"Error closing RabbitMQ connection: {str(e)}")
