FROM python:3.10-slim

WORKDIR /app

# Install system dependencies for PDF processing
RUN apt-get update && apt-get install -y \
    poppler-utils \
    tesseract-ocr \
    libpq-dev \
    build-essential \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements file and install Python dependencies
COPY requirements.txt /app/
RUN pip install --no-cache-dir -r requirements.txt

# Copy the application code
COPY . /app/

# Set only essential environment variables
ENV PYTHONPATH=/app
ENV PYTHONUNBUFFERED=1
# Note: All connection-related environment variables (DB, RabbitMQ)
# should be configured in Railway or other deployment platform

# Run the FastAPI application
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8001"]
