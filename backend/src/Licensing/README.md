# Licensing Layer

This module should remain isolated from general bill CRUD logic.

Expected responsibilities:

- machine fingerprint generation from multiple hardware identifiers
- hashed fingerprint persistence and comparison
- externally signed license file parsing
- embedded public-key signature verification
- license status inspection services for the admin UI
