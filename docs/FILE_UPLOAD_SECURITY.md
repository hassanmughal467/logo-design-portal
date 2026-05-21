# File Upload Security

## Central enforcement

`LogoDesignPortal.Application/Helpers/UploadSecurityHelper.cs`

| Control | Implementation |
|---------|----------------|
| Extension whitelist | Per-service allow lists + global **blocked** extensions (`.exe`, `.html`, `.php`, …) |
| MIME validation | `ValidateDeclaredContentType` for image/PDF types |
| Magic bytes | `ValidateMagicBytes` for `.png`, `.jpg`, `.gif`, `.webp`, `.pdf`, `.svg` |
| Filename sanitization | `SanitizeOriginalFileName` — strips paths, illegal chars |
| Path traversal | Rejects `..` in names |
| Double-extension decoy | `logo.png.exe` pattern detection |
| Size limits | `UploadLimits` — 100 MB/file, 500 MB/request |
| Duplicate window | `FileService` — 1-hour duplicate upload guard |
| Storage | GUID-based stored names; original name sanitized for downloads |

## Endpoints audited

| Endpoint | Service | Validation |
|----------|---------|------------|
| `POST /api/files/{orderId}` | `FileService` | Full |
| `POST /api/files/{orderId}/multiple` | `FileService` | Full |
| Order create with files | `FileService.PrepareReferenceFiles*` | Full |
| Revisions with files | `RevisionService` | Full (added magic bytes) |
| Quote attachments | `QuoteService` | Full (added MIME + magic bytes) |
| Settings logo | `SettingsController` | Image types only |

## Attack scenarios covered

| Scenario | Mitigation |
|----------|------------|
| `.exe` upload | Blocked extension list |
| `malware.png.exe` | Decoy extension detection + effective `.exe` |
| PDF content with `.png` name | Magic byte mismatch → 400 |
| `../../etc/passwd` | Path traversal rejection |
| Oversized multipart | `UploadLimits` + `FormOptions` |
| IDOR upload to another order | Authorization in `FileService` (integration tests) |
| Designer upload to unassigned order | 403 integration test |

## Tests

- `UploadSecurityHelperTests` (unit)
- `FilesControllerUploadSecurityIntegrationTests` (integration)
- `FileServiceUploadAbuseTests` (unit)

## Remaining gaps (Week 2+)

- ClamAV / cloud malware scanning hook (`IFileUploadScanHook` stub)
- SVG script content sanitization
- Settings logo: persist to storage with same pipeline as `FileService` (currently placeholder URL)
