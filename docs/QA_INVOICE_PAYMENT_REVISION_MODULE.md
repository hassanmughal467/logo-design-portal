# Invoice, Payment & Revision Module — QA Reference

**Module scope:** Client billing (`InvoiceService`, `InvoicesController`), payments (`PaymentService`, `PaymentsController`), revision workflow (`RevisionService`, `RevisionsController`), revision limits (`RevisionLimitHelper`), order status coupling on approve/revision.

**Canonical backend:** `Backend/src/`

---

## 1. Manual QA checklist

### Invoices (Admin)

| # | Step | Expected |
|---|------|----------|
| I1 | Admin creates invoice from completed billable order(s) | 201; invoice number assigned; order(s) marked invoiced |
| I2 | Admin sends invoice to client | 200; client notification (if email configured) |
| I3 | Admin marks invoice paid (bank transfer) | Status `Paid`; `PaidDate` set |
| I4 | Admin edits invoice items (draft/unpaid) | Totals recalculated |
| I5 | Admin flexible invoice (date range / selected orders) | Only eligible uninvoiced orders included |
| I6 | Admin downloads invoice PDF | 200 `application/pdf` |
| I7 | Admin downloads invoice report | 200 PDF; max ~500 rows |
| I8 | Create invoice with empty orders + no manual lines | 400 |

### Invoices (Client)

| # | Step | Expected |
|---|------|----------|
| I9 | Client lists own invoices | 200; only own `clientId` rows |
| I10 | Client opens own invoice by id | 200 |
| I11 | Client opens another client's invoice GUID | **404** (not 403 — no enumeration) |
| I12 | Client creates / mark-paid / send invoice | **403** (role guard) |
| I13 | Client views invoice logs for own invoice | 200 |
| I14 | Client views logs for other's invoice | 404 |

### Payments

| # | Step | Expected |
|---|------|----------|
| P1 | Client generates bank transfer link for own unpaid invoice | 200; payment row `Pending` |
| P2 | Client views payment by id (own invoice) | 200 |
| P3 | Client views payment for another client's invoice | **404** |
| P4 | Client lists payments by invoice id (own) | 200 |
| P5 | Client lists payments by other's invoice id | **404** |
| P6 | Admin updates payment status | 200 |
| P7 | Designer updates payment status | **403** |
| P8 | Client gets bank details | 200 |
| P9 | PayPal webhook with empty body | 400 |
| P10 | PayPal webhook without verification headers | **401** |

### Revisions

| # | Step | Expected |
|---|------|----------|
| R1 | Client requests revision when status `PreviewDelivered` | 200; status → `RevisionRequested` |
| R2 | Client requests revision when `InProgress` | 400 |
| R3 | Client exceeds package revision limit | 400 with limit message |
| R4 | Admin enabled `AllowExtraRevisions` | Client can request beyond limit |
| R5 | Designer calls request-revision endpoint | **403** (role) |
| R6 | Client requests revision on another client's order | **403** |
| R7 | Client approves logo from `PreviewDelivered` | Order → `ClientApproved` / completion path |
| R8 | `can-request` / `can-approve` endpoints | Boolean matches service rules |
| R9 | Client downloads own revision reference file | 200 |
| R10 | Designer downloads revision file on assigned order | 200 |
| R11 | Upload disallowed extension on revision | 400 |

---

## 2. Edge cases

- Double invoice same order (second create should fail or skip — verify message).
- Mark paid twice on same invoice.
- Partial payment vs full `TotalAmount` (PayPal/Wise vs manual mark-paid).
- Invoice on order not `Completed` or not `BillingEligible`.
- Revision at exactly limit boundary (`revisionCount == limit`).
- Premium package (`price >= 500`) unlimited revisions (`revisionLimit` null).
- Concurrent mark-paid + payment webhook (race on invoice status).
- `ProductionSafetyOptions.DisableInvoiceEditing` blocks edits.
- Expired payment link (`ExpiresAt`).
- Revision with empty instructions (< 10 chars) — validation 400.
- Revision multipart over 500MB combined.

---

## 3. Failure scenarios

| Scenario | Symptom | Mitigation |
|----------|---------|------------|
| PayPal credentials missing | Payment link empty / failed status | Settings seed; monitor logs |
| PDF generation failure | 500 on download | Check branding assets / QuestPDF |
| Invoice access returns 404 for valid client | Wrong `ClientProfile.UserId` link | Data integrity |
| Revision upload disk permission | 500 on request revision | IIS app pool write on `FileStorage:Path` |
| Order locked (`Completed`) | Revision/approve blocked | Expected — verify message |

---

## 4. Security vulnerabilities (test focus)

| ID | Risk | Test coverage |
|----|------|----------------|
| SEC-INV-01 | Cross-client invoice IDOR (GET by GUID) | `InvoicesControllerPrivacyTests` |
| SEC-PAY-01 | Cross-client payment IDOR | `PaymentsControllerPrivacyTests` |
| SEC-PAY-02 | Payment creation without invoice ownership check | **Fixed** — `PaymentInvoiceAccessHelper`; `PaymentsControllerAuthorizationTests` expects 403 |
| SEC-PAY-03 | PayPal webhook signature bypass | Webhook security integration tests |
| SEC-PAY-04 | Client marks invoice paid via API | Role guard on `mark-paid` |
| SEC-REV-01 | Revision request on another client's order | `RevisionsControllerAuthorizationTests` |
| SEC-REV-02 | Revision file download IDOR | Service-level access in `DownloadRevisionFileAsync` |
| SEC-INV-02 | Designer reads all invoices (if unintended) | Manual review — access only restricts `Client` role |

---

## 5. Permission abuse cases

| Actor | Abuse attempt | Expected |
|-------|---------------|----------|
| Client | `POST /api/invoices` | 403 |
| Client | `PUT .../mark-paid` | 403 |
| Client | `GET` other client's invoice | 404 |
| Designer | `POST /api/payments` for client invoice | **403** |
| Designer | `POST revisions/.../request` | 403 |
| Anonymous | `POST /api/payments/webhook/paypal` (no sig) | 401 |
| Client | Approve logo on other's order | 403 |

---

## 6. Invalid workflow transitions (revision ↔ order status)

| From | Action | Expected status |
|------|--------|-----------------|
| PreviewDelivered | Client request revision | RevisionRequested |
| PreviewDelivered | Client approve logo | ClientApproved (then completion flow) |
| InProgress | Request revision | 400 |
| Completed | Request revision | 400 (locked) |
| RevisionRequested | Request revision again | 400 until preview re-delivered |

Enforced in: `RevisionService`, `OrderStatusTransitionHelper`, `OrderStatusStateMachine`.

---

## 7–10. Automated tests

See `docs/TESTING_INVOICE_PAYMENT_REVISION_MODULE.md` for file paths, naming, CI wiring, and setup.
