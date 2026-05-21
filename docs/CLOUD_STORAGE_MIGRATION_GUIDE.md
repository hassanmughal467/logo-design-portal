# Cloud Storage Migration Guide

## Provider options

| Provider | Notes |
|----------|-------|
| **Cloudflare R2** | S3-compatible, no egress to Cloudflare CDN |
| **AWS S3** | Standard; use IAM instance role on ECS/EC2 |
| **Azure Blob** | Good if hosting on Azure App Service |

## Configuration (future)

```json
"FileStorage": {
  "Provider": "S3",
  "Bucket": "ldp-production",
  "Region": "us-east-1",
  "Prefix": "files/",
  "PublicBaseUrl": "https://cdn.example.com/files/"
}
```

## Implementation steps

1. Add `S3FileStorageProvider : IFileStorageProvider` in `Infrastructure/Storage/`.
2. Feature flag: `FileStorage:Provider` = `Local` | `S3`.
3. New uploads write to cloud; DB stores `files/Permanent/{id}.ext` keys only.
4. Background job: copy `Files/Permanent/*` → bucket; verify checksum.
5. Update `FileStorageHealthCheck` to HEAD bucket object.
6. Enable CDN origin = bucket or R2 public URL.

## Security

- Never expose bucket publicly; use **signed URLs** for downloads (short TTL).
- Keep virus scan hook (`IFileUploadScanHook`) before `WriteAsync`.
- Block `..` in paths (already in `LocalFileStorageProvider`).

## Rollback

Keep local provider config; switch `Provider` back to `Local` and shared volume.
