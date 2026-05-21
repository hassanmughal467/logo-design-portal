# File Storage Scaling Plan (Week 3)

## Current state

- **Local disk** under `FileStorage:Path` (default `Files/`)
- Subdirs: `Temporary/`, `Permanent/`
- Services: `FileService`, `RevisionService`, `QuoteService`, `OrphanFileCleanupService`
- Security: `UploadSecurityHelper`, magic-byte validation, size limits
- **No cloud provider** — direct `File.*` I/O

## Bottlenecks

| Risk | Severity |
|------|----------|
| Load-balanced upload to instance A, download from B | **Critical** |
| Disk fill on single server | High |
| No CDN for static delivery | Medium |
| Backup = filesystem copy | Medium |

## Week 3 prep

- **`IFileStorageProvider`** + **`LocalFileStorageProvider`** registered in DI
- Path traversal protection in `ResolvePath`
- Async buffered I/O (81 KB buffer)

## Target architecture

```
Upload API → FileService → IFileStorageProvider → [Local | S3 | R2 | Azure Blob]
                              ↓
                         DB: relative path only
```

## Phased rollout

| Phase | Action |
|-------|--------|
| 1 | Shared NAS/SMB for `FileStorage:Path` (quick multi-instance fix) |
| 2 | Implement `S3CompatibleFileStorageProvider` (R2/S3) |
| 3 | CDN in front of public assets; signed URLs for downloads |
| 4 | Retire local paths on new uploads; migrate legacy files |

## Capacity planning

- Assume **5–25 MB** per logo asset; **10 files/order** peak.
- 10k orders ≈ 500 GB–2 TB (plan object storage lifecycle).

See [CLOUD_STORAGE_MIGRATION_GUIDE.md](./CLOUD_STORAGE_MIGRATION_GUIDE.md).
