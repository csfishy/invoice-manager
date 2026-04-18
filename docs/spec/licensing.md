# Licensing Specification

## Objectives

- Support offline-first validation.
- Bind the license to a machine fingerprint.
- Store only a hashed fingerprint.
- Accept only externally signed license files.
- Verify signatures using an embedded public key.

## Fingerprint Design

The machine fingerprint should be derived from multiple stable identifiers, for example:

- motherboard or system UUID
- CPU identifier
- primary disk serial
- MAC address of an active physical network adapter

The implementation should:

- combine multiple identifiers instead of trusting only one
- normalize values before hashing
- tolerate limited hardware drift when appropriate
- hash the final fingerprint before storage or comparison

## License File Expectations

Recommended license payload fields:

- customer name
- license id
- product name
- issued at
- expires at, if applicable
- allowed machine fingerprint hash
- allowed edition or feature flags
- signature

## Validation Flow

1. Read the license file from the configured application data path.
2. Parse the signed payload.
3. Verify the signature with the embedded public key only.
4. Generate the local machine fingerprint.
5. Hash the local fingerprint.
6. Compare the local hash with the licensed fingerprint hash.
7. Evaluate expiration and product constraints.
8. Return the license status to the application and admin UI.

## Operational Requirements

- No private key is stored in the application.
- License validation must work without internet access.
- Admins must be able to inspect license status from the UI.
- Invalid license state should be visible during startup and health checks.

## Suggested Admin UI Fields

- license id
- customer name
- status
- issue date
- expiration date
- fingerprint match result
- enabled features
- last validation timestamp
