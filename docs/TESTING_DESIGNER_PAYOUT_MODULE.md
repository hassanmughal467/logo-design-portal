# Designer Payout Module — Test Engineering Guide

## Directory structure

```
Backend/src/
  LogoDesignPortal.Application.Tests/
    Services/DesignerPayoutServiceTests.cs     (existing)
  LogoDesignPortal.API.IntegrationTests/
    Controllers/
      DesignerPayoutControllerAuthorizationTests.cs
      DesignerPayoutControllerPrivacyTests.cs

Frontend/e2e/tests/
  security/designer-payout.security.spec.ts
```

## Run commands

```bash
dotnet test Backend/src/LogoDesignPortal.API.IntegrationTests \
  --filter "FullyQualifiedName~DesignerPayout"

dotnet test Backend/src/LogoDesignPortal.Application.Tests \
  --filter "FullyQualifiedName~DesignerPayout"
```

## Related

- Client billing QA: `docs/QA_INVOICE_PAYMENT_REVISION_MODULE.md`
- Payment ownership fix: `PaymentInvoiceAccessHelper` + `PaymentService` mutation guards
