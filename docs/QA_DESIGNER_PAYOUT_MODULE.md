# Designer Payout Module — QA Reference

**Module scope:** Designer pricing proposals, admin price approval, payout-eligible orders, designer invoice generation (`DesignerPayoutService`), `DesignerPayoutController`, `DesignerInvoiceController`.

**Canonical backend:** `Backend/src/`

---

## 1. Manual QA checklist

### Designer pricing

| # | Step | Expected |
|---|------|----------|
| D1 | Designer submits pricing on assigned `InProgress` order | Status → `PriceApprovalPending`; proposed price saved |
| D2 | Designer proposes price on unassigned order | 403 |
| D3 | Client attempts propose pricing | 403 |
| D4 | Admin approves proposed price | `PriceApprovalStatus` approved; order returns to `InProgress` when charges align |
| D5 | Admin rejects proposed price | Appropriate status/message |

### Payout eligibility

| # | Step | Expected |
|---|------|----------|
| D6 | Designer views `me/eligible-orders` | Only own completed, approved, uninvoiced work |
| D7 | Admin views `designers/{id}/eligible-orders` | That designer's eligible orders |
| D8 | Designer calls admin-only eligible-orders route | 403 |

### Designer invoices (admin)

| # | Step | Expected |
|---|------|----------|
| D9 | Admin opens payout summary | Totals and pending designers |
| D10 | Admin generates monthly invoice (period) | Invoice created; orders locked/invoiced |
| D11 | Admin generates invoice from builder (selected orders) | Line items match selection |
| D12 | Admin marks designer invoice paid | Status `Paid`; `PaidDate` set |
| D13 | Admin edits unpaid invoice item / adjustment | Totals recalculated |
| D14 | Edit paid invoice | 400 |

### Designer invoices (designer)

| # | Step | Expected |
|---|------|----------|
| D15 | Designer lists `me/invoices` | Own invoices only |
| D16 | Designer opens own invoice | 200 |
| D17 | Designer opens another designer's invoice id | **404** |
| D18 | Designer calls `designer-invoice/payout-summary` | 403 |

### Production safety

| # | Step | Expected |
|---|------|----------|
| D19 | `ProductionSafety.DisableDesignerPayout` enabled | Generation endpoints return error |

---

## 2. Security test focus

| ID | Risk | Coverage |
|----|------|----------|
| SEC-DP-01 | Designer reads another designer's payout invoice | `DesignerPayoutControllerPrivacyTests` |
| SEC-DP-02 | Client accesses admin payout APIs | Authorization tests |
| SEC-DP-03 | Designer generates admin payout invoice | 403 on `generate-invoice` |
| SEC-DP-04 | Cross-designer eligible order enumeration | Admin routes role-guarded |

---

## 3. Automated tests

See `docs/TESTING_DESIGNER_PAYOUT_MODULE.md`.
