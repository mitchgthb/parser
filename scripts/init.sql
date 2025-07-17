-- Initialize PostgreSQL database for Dualite Document Processing Platform

-- Create extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Create schema
CREATE SCHEMA IF NOT EXISTS dualite;

-- Create tables

-- Clients table
CREATE TABLE IF NOT EXISTS dualite.clients (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    organization VARCHAR(255),
    email VARCHAR(255) NOT NULL UNIQUE,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    status VARCHAR(50) DEFAULT 'active',
    custom_fields JSONB DEFAULT '{}'::jsonb
);

-- API Keys table
CREATE TABLE IF NOT EXISTS dualite.api_keys (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES dualite.clients(id) ON DELETE CASCADE,
    key_hash VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP WITH TIME ZONE,
    last_used_at TIMESTAMP WITH TIME ZONE,
    is_active BOOLEAN DEFAULT TRUE,
    permissions JSONB DEFAULT '{}'::jsonb,
    meta JSONB DEFAULT '{}'::jsonb
);

-- Create index for API key lookups
CREATE INDEX IF NOT EXISTS idx_api_keys_hash ON dualite.api_keys(key_hash);

-- Processing Jobs table
CREATE TABLE IF NOT EXISTS dualite.processing_jobs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES dualite.clients(id),
    job_type VARCHAR(50) NOT NULL, -- 'email', 'invoice', etc.
    status VARCHAR(50) DEFAULT 'pending',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP WITH TIME ZONE,
    input_data JSONB DEFAULT '{}'::jsonb,
    output_data JSONB DEFAULT '{}'::jsonb,
    error_message TEXT,
    priority INTEGER DEFAULT 0,
    processing_time_ms INTEGER,
    meta JSONB DEFAULT '{}'::jsonb
);

-- Create index for job status lookups
CREATE INDEX IF NOT EXISTS idx_jobs_status ON dualite.processing_jobs(status);
CREATE INDEX IF NOT EXISTS idx_jobs_client_id ON dualite.processing_jobs(client_id);

-- Email Extractions table
CREATE TABLE IF NOT EXISTS dualite.email_extractions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    job_id UUID NOT NULL REFERENCES dualite.processing_jobs(id) ON DELETE CASCADE,
    subject VARCHAR(500),
    sender_email VARCHAR(255),
    sender_name VARCHAR(255),
    recipient_emails TEXT[],
    cc_emails TEXT[],
    body_text TEXT,
    entities JSONB DEFAULT '{}'::jsonb,
    intent VARCHAR(100),
    intent_confidence FLOAT,
    urgency_score FLOAT,
    effort_estimate VARCHAR(50),
    extracted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    extracted_fields JSONB DEFAULT '{}'::jsonb
);

-- Invoice Extractions table
CREATE TABLE IF NOT EXISTS dualite.invoice_extractions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    job_id UUID NOT NULL REFERENCES dualite.processing_jobs(id) ON DELETE CASCADE,
    invoice_number VARCHAR(100),
    invoice_date DATE,
    seller_name VARCHAR(255),
    seller_kvk VARCHAR(20),
    seller_iban VARCHAR(34),
    buyer_name VARCHAR(255),
    buyer_kvk VARCHAR(20),
    total_amount DECIMAL(15, 2),
    vat_amount DECIMAL(15, 2),
    vat_rate DECIMAL(5, 2),
    currency VARCHAR(3) DEFAULT 'EUR',
    line_items JSONB DEFAULT ''::jsonb,
    extracted_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    validation_status VARCHAR(50) DEFAULT 'pending',
    validation_messages TEXT[],
    extracted_fields JSONB DEFAULT '{}'::jsonb,
    confidence_scores JSONB DEFAULT '{}'::jsonb
);

-- Create rate limits table for API throttling
CREATE TABLE IF NOT EXISTS dualite.rate_limits (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES dualite.clients(id) ON DELETE CASCADE,
    endpoint VARCHAR(255) NOT NULL,
    requests_per_minute INTEGER NOT NULL DEFAULT 60,
    requests_per_hour INTEGER NOT NULL DEFAULT 1000,
    requests_per_day INTEGER NOT NULL DEFAULT 10000,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    is_active BOOLEAN DEFAULT TRUE
);

-- Create usage tracking table
CREATE TABLE IF NOT EXISTS dualite.usage_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES dualite.clients(id),
    api_key_id UUID REFERENCES dualite.api_keys(id),
    endpoint VARCHAR(255) NOT NULL,
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    response_time_ms INTEGER,
    status_code INTEGER,
    request_size INTEGER,
    response_size INTEGER,
    ip_address VARCHAR(45),
    user_agent TEXT,
    request_id UUID DEFAULT uuid_generate_v4()
);

-- Create index for usage tracking by time
CREATE INDEX IF NOT EXISTS idx_usage_logs_timestamp ON dualite.usage_logs(timestamp);
CREATE INDEX IF NOT EXISTS idx_usage_logs_client ON dualite.usage_logs(client_id);

-- Create billing table
CREATE TABLE IF NOT EXISTS dualite.billing_records (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    client_id UUID NOT NULL REFERENCES dualite.clients(id),
    billing_period_start DATE NOT NULL,
    billing_period_end DATE NOT NULL,
    total_requests INTEGER DEFAULT 0,
    email_processes INTEGER DEFAULT 0,
    invoice_processes INTEGER DEFAULT 0,
    amount_due DECIMAL(15, 2) DEFAULT 0.0,
    currency VARCHAR(3) DEFAULT 'EUR',
    status VARCHAR(50) DEFAULT 'pending',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    payment_date TIMESTAMP WITH TIME ZONE,
    meta JSONB DEFAULT '{}'::jsonb
);

-- Create function to update updated_at timestamp
CREATE OR REPLACE FUNCTION update_timestamp()
RETURNS TRIGGER AS $$
BEGIN
   NEW.updated_at = CURRENT_TIMESTAMP;
   RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create triggers for updated_at fields
CREATE TRIGGER update_clients_timestamp BEFORE UPDATE ON dualite.clients
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_api_keys_timestamp BEFORE UPDATE ON dualite.api_keys
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_processing_jobs_timestamp BEFORE UPDATE ON dualite.processing_jobs
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

CREATE TRIGGER update_rate_limits_timestamp BEFORE UPDATE ON dualite.rate_limits
    FOR EACH ROW EXECUTE FUNCTION update_timestamp();

-- Create sample data for testing
INSERT INTO dualite.clients (name, organization, email, status)
VALUES ('Test User', 'Test Organization', 'test@example.com', 'active')
ON CONFLICT DO NOTHING;

-- Create a test API key (hashed value represents 'test-api-key')
INSERT INTO dualite.api_keys (client_id, key_hash, name, is_active, permissions)
SELECT id, 'a6e9d25e7e294af9b3d9111a8aace9b24ab2617d9678b5f3cc10be46dd78534f', 'Test API Key', true, '{"email": true, "invoice": true}'::jsonb
FROM dualite.clients WHERE email = 'test@example.com'
ON CONFLICT DO NOTHING;

-- Grant permissions
GRANT ALL PRIVILEGES ON SCHEMA dualite TO postgres;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA dualite TO postgres;
