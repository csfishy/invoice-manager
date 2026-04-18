# Architecture Specification

## Overview

The system is a self-hosted web application consisting of:

- Next.js frontend
- ASP.NET Core Web API backend
- PostgreSQL database
- file storage for bill attachments
- isolated licensing module for offline machine-bound activation

## High-Level Principles

- Keep frontend and backend clearly separated.
- Keep licensing logic isolated from bill CRUD logic.
- Keep OCR and import processing separate from the main billing workflow.
- Keep secrets and operational configuration in environment variables.
- Prefer explicit, easy-to-maintain structure over abstraction-heavy design.

## Modules

- Auth module
- Bill management module
- Attachment module
- Search and filtering module
- Dashboard and reporting module
- Audit log module
- Licensing module
- Deployment and operations module

## Backend Layering

### Api

- request entry points
- auth wiring
- validation registration
- dependency injection composition
- startup checks

### Application

- use cases
- DTOs
- validation rules
- service interfaces

### Domain

- entities
- value objects
- enums
- business rules

### Infrastructure

- EF Core persistence
- storage adapters
- audit log persistence
- auth token services

### Licensing

- fingerprint generation
- fingerprint hashing
- signature verification
- license state evaluation

## Deployment Model

Customer installs the application on a Windows machine or server using Docker Compose.

## Storage

- PostgreSQL for transactional data
- mounted file storage path for uploaded attachments
- license file stored in a controlled application data path
- local bind-mounted folders for customer-friendly backup and restore flows

## Database Schema

### users

- id
- username
- display_name
- password_hash
- role
- is_active
- created_at_utc
- last_login_at_utc

### bills

- id
- reference_number
- type
- bill_category_id
- payment_status
- customer_name
- property_name
- provider_name
- account_number
- amount
- currency
- period_start
- period_end
- issue_date
- due_date
- paid_date
- notes
- keywords
- created_at_utc
- updated_at_utc
- created_by_user_id
- updated_by_user_id

### bill_categories

- id
- name
- type
- description
- sort_order
- is_active
- is_system_default
- created_at_utc

### bill_files

- id
- bill_id
- original_file_name
- stored_file_name
- content_type
- file_size
- uploaded_at_utc
- uploaded_by_user_id

### payment_records

- id
- bill_id
- amount_paid
- paid_on
- payment_method
- reference_number
- note
- recorded_by_user_id
- created_at_utc

### reminder_rules

- id
- name
- bill_category_id
- bill_type
- days_before_due
- recipient
- channel
- is_enabled
- created_at_utc

### audit_logs

- id
- occurred_at_utc
- user_id
- username
- action
- entity_type
- entity_id
- summary
- metadata_json

### license_bindings

- id
- license_id
- customer_name
- machine_fingerprint_hash
- binding_status
- bound_at_utc
- expires_at_utc
- last_validated_at_utc
- features_json

## Security

- JWT authentication
- role-based authorization
- environment variable based configuration
- public-key license verification
- audit logging for important actions
- hashed machine fingerprint storage only

## Licensing Architecture

- Machine fingerprint service
- fingerprint hash service
- license import service
- license signature verification service
- license status service
- startup license guard
