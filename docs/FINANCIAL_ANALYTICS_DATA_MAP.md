# Financial Analytics Data Model

## Overview

This document maps the financial data model for the Logo Design Portal financial analytics dashboard.

---

## 1. Order (LogoOrder)

| Field | Type | Analytics Use |
|-------|------|---------------|
| Id | Guid | Order identification |
| ClientId | Guid | Client revenue analytics |
| DesignerId | Guid? | Designer revenue analytics |
| Price | decimal | Revenue, AOV |
| ProposedPrice | decimal? | Price proposal |
| Status | OrderStatus | Completed = revenue |
| CreatedAt | DateTime | Order date |
| UpdatedAt | DateTime? | Completion timestamp when Status=Completed |
| IsRefunded | bool | Refund filter |
| RefundAmount | decimal? | Refund analytics |

**Package Type:** Derived from Price when not stored:
- Basic: < $200
- Standard: $200 - $500
- Premium: $500 - $1000
- Custom: >= $1000

---

## 2. Invoice

| Field | Type | Analytics Use |
|-------|------|---------------|
| Id | Guid | Invoice identification |
| ClientId | Guid | Client billing |
| TotalAmount | decimal | Invoice value |
| Amount | decimal | Tax-exclusive |
| TaxAmount | decimal | Tax |
| Status | InvoiceStatus | Paid, Pending, Overdue |
| IssueDate | DateTime | Invoice date |
| DueDate | DateTime? | Overdue detection |
| PaidDate | DateTime? | Paid date |

**InvoiceStatus:** Pending=1, Paid=2, Due=3, Overdue=4

---

## 3. Payment

| Field | Type | Analytics Use |
|-------|------|---------------|
| Id | Guid | Payment identification |
| InvoiceId | Guid | Invoice link |
| Amount | decimal | Payment amount |
| Status | PaymentStatus | Completed |
| CompletedAt | DateTime? | Payment date |

**PaymentStatus:** Pending=0, Processing=1, Completed=2, Failed=3, Cancelled=4, Refunded=5

---

## 4. Revenue Metrics

| Metric | Source |
|--------|--------|
| Total Revenue | Sum(LogoOrder.Price) where Status=Completed |
| Collected Revenue | Sum(Invoice.TotalAmount) where Status=Paid |
| Refunded Amount | Sum(LogoOrder.RefundAmount) where IsRefunded |
| Outstand Balance | Sum(Invoice.TotalAmount) where Status in (Pending, Overdue) |

---

## 5. Invoice Status Counts

| Status | Count |
|--------|-------|
| Paid | Invoice.Status = Paid |
| Pending | Invoice.Status = Pending |
| Overdue | Invoice.Status = Overdue |
| Cancelled | N/A (not in enum) - use 0 or exclude |
