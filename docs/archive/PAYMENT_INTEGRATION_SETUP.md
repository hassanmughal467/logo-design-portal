# Payment Integration Setup Guide

This guide explains how to set up and use the payment integration system that supports PayPal, Wise, and Bank Transfer payments.

## Features

- **PayPal Integration**: Direct payment processing with PayPal
- **Wise Integration**: Payment links for Wise (formerly TransferWise) transfers
- **Bank Transfer**: Display bank details for manual transfers
- **Payment Links**: Generate and share payment links with clients
- **Payment Tracking**: Track payment status and transactions

## Backend Setup

### 1. Database Migration

The payment system requires a new `Payment` table. Create and run a migration:

```bash
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef migrations add AddPaymentEntity --startup-project ../LogoDesignPortal.API
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

### 2. Configure Payment Settings

Configure payment gateway credentials in the Settings page of the application:

#### PayPal Configuration
- **PayPalClientId**: Your PayPal Client ID
- **PayPalClientSecret**: Your PayPal Client Secret
- **PayPalUseSandbox**: Set to "true" for testing, "false" for production

#### Wise Configuration
- **WiseApiKey**: Your Wise API key
- **WiseProfileId**: Your Wise profile ID

#### Bank Details Configuration
- **BankName**: Your bank name
- **AccountHolderName**: Account holder name
- **AccountNumber**: Bank account number
- **IBAN**: International Bank Account Number (if applicable)
- **SWIFT**: SWIFT/BIC code (if applicable)
- **RoutingNumber**: Routing number (for US banks)
- **BranchAddress**: Bank branch address
- **BankCurrency**: Currency code (default: USD)

### 3. API Endpoints

The payment system exposes the following endpoints:

- `POST /api/payments` - Create a new payment
- `GET /api/payments/{id}` - Get payment details
- `GET /api/payments/invoice/{invoiceId}` - Get payments for an invoice
- `POST /api/payments/link` - Generate payment link
- `POST /api/payments/process` - Process a payment
- `GET /api/payments/bank-details` - Get bank transfer details
- `POST /api/payments/verify/paypal` - Verify PayPal payment
- `PUT /api/payments/{id}/status` - Update payment status (Admin only)

## Frontend Usage

### Payment Component

The payment component is integrated into the invoice payment dialog. When a user clicks "Pay" on a single invoice, they'll see:

1. **Payment Method Selection**: Choose between PayPal, Wise, or Bank Transfer
2. **Payment Processing**: 
   - PayPal: Redirects to PayPal checkout
   - Wise: Generates a payment link to share
   - Bank Transfer: Displays bank details with copy functionality

### Generating Payment Links

Admins can generate payment links to share with clients:

1. Open an invoice
2. Click "Pay" button
3. Select payment method (PayPal or Wise)
4. Click "Generate Payment Link" (for Wise) or "Pay with PayPal"
5. Copy and share the link with the client

### Bank Transfer Details

For bank transfers:

1. Select "Bank Transfer" as payment method
2. Bank details are automatically loaded
3. Click "Copy Details" to copy all bank information
4. Share with client via email or message

## Payment Flow

### PayPal Flow
1. User selects PayPal payment method
2. System creates PayPal order
3. User is redirected to PayPal checkout
4. After payment, PayPal redirects back
5. System verifies payment and marks invoice as paid

### Wise Flow
1. User selects Wise payment method
2. System generates payment link
3. Link is shared with client
4. Client completes transfer on Wise
5. Admin verifies and marks invoice as paid

### Bank Transfer Flow
1. User selects Bank Transfer method
2. System displays bank details
3. Client makes transfer with reference number
4. Admin verifies transfer and marks invoice as paid

## Testing

### PayPal Sandbox
1. Set `PayPalUseSandbox` to "true" in settings
2. Use PayPal sandbox test accounts
3. Test payment flow end-to-end

### Manual Testing
1. Create a test invoice
2. Try each payment method
3. Verify payment links work
4. Test bank details display

## Security Considerations

1. **API Keys**: Store payment API keys securely in settings (not in code)
2. **HTTPS**: Always use HTTPS in production
3. **Webhooks**: Configure PayPal webhooks for automatic payment verification
4. **Validation**: Always verify payments before marking invoices as paid

## Troubleshooting

### PayPal Issues
- Verify Client ID and Secret are correct
- Check sandbox vs production mode
- Ensure return URLs are configured correctly

### Wise Issues
- Verify API key and profile ID
- Check API rate limits
- Ensure payment links are accessible

### Bank Details
- Verify all bank details are configured
- Check currency matches invoice currency
- Ensure reference number is included

## Next Steps

1. Configure payment settings in the application
2. Test each payment method
3. Set up webhooks for automatic verification
4. Train staff on payment processing
5. Monitor payment transactions

## Support

For issues or questions:
1. Check application logs
2. Verify payment gateway credentials
3. Review payment transaction history
4. Contact payment gateway support if needed
