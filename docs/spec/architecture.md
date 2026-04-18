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
