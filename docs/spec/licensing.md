# Licensing Specification

## Objectives

- Support offline-first validation for customer-hosted environments.
- Bind each license to one machine fingerprint hash.
- Never persist raw hardware identifiers in the application database or license file.
- Verify license signatures with an embedded public key only.
- Keep private signing keys fully outside the application.

## Fingerprint Design

The application derives a machine fingerprint from multiple identifiers and hashes the combined value with a configured salt.

Windows-oriented sources:

- `Win32_ComputerSystemProduct.UUID`
- `Win32_BaseBoard.SerialNumber`
- `Win32_BIOS.SerialNumber`
- `Win32_Processor.ProcessorId`
- `Win32_DiskDrive.SerialNumber`
- physical network adapter MAC addresses

Fallback sources for non-Windows or reduced environments:

- `/etc/machine-id`
- machine name
- architecture values

Rules:

- identifiers are normalized only in memory
- raw identifiers are never written to the database
- only the salted SHA-256 fingerprint hash is stored and compared

## Request Code Flow

On first run, an admin opens the Licensing page and copies the machine request code.

Request code format:

- Base64-encoded JSON
- fields:
  - `version`
  - `productName`
  - `machineName`
  - `machineFingerprintHash`
  - `generatedAtUtc`

The customer sends this request code to the vendor through an offline support channel.

## License File Format

The vendor issues the license externally and signs it with the vendor private key.

License JSON fields:

- `version`
- `licenseId`
- `customerName`
- `productName`
- `issuedAtUtc`
- `expiresAtUtc`
- `machineFingerprintHash`
- `boundAtUtc`
- `features`
- `signature`

The signature is computed over the canonical payload fields only, excluding the `signature` field itself.

## Validation Flow

1. Read the license file from the configured data path.
2. Parse the signed JSON document.
3. Verify the signature with the embedded public key.
4. Generate the current machine fingerprint hash locally.
5. Compare the local hash with the licensed hash.
6. Check product name and expiration.
7. Persist only the hashed fingerprint and current binding status.
8. Return the status to the admin UI and enforcement layer.

## Enforcement Behavior

- Missing, invalid, expired, or mismatched licenses are reported with clear status messages.
- Protected business endpoints reject access when enforcement is enabled and the license is not valid.
- License inspection, request-code generation, and license import remain available to admins so activation can complete.
- Startup performs a validation pass and writes the result to application logs.

Statuses used by the application:

- `Valid`
- `MissingLicense`
- `InvalidLicense`
- `InvalidSignature`
- `FingerprintMismatch`
- `Expired`
- `WrongProduct`

## External Issuance Process

The vendor-side signing workflow is intentionally outside this repository.

Recommended vendor process:

1. Receive the request code from the customer.
2. Decode the Base64 JSON payload.
3. Verify the requested product and customer order details.
4. Build the license JSON payload with the same `machineFingerprintHash`.
5. Sign the canonical payload with the vendor private RSA key.
6. Send the signed license file back to the customer.

Security notes:

- the private signing key must never be copied into this application
- only the public verification key is embedded in the shipping app
- request codes and license files can be exchanged fully offline

## Development Note

For local development and automated tests only, `Licensing__AllowUnlicensedDevelopmentMode=true` can bypass endpoint enforcement while still surfacing real license status in the UI and logs. Customer deployments should keep this disabled.
