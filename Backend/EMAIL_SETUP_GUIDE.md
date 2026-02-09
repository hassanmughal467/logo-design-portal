# 📧 Email Configuration Guide

This guide explains how to configure email sending for password reset functionality.

## 🚀 Quick Setup Options

### Option 1: Gmail (Recommended for Development)

1. **Enable 2-Factor Authentication** on your Gmail account
   - Go to: https://myaccount.google.com/security
   - Enable 2-Step Verification

2. **Generate App Password**
   - Go to: https://myaccount.google.com/apppasswords
   - Select "Mail" and "Other (Custom name)"
   - Enter "Hawk Merchandising Portal"
   - Copy the 16-character password

3. **Update `appsettings.Development.json`**:
   ```json
   "Email": {
     "SmtpServer": "smtp.gmail.com",
     "SmtpPort": 587,
     "SmtpUsername": "your-email@gmail.com",
     "SmtpPassword": "xxxx xxxx xxxx xxxx",  // Your 16-char app password
     "FromEmail": "your-email@gmail.com",
     "FromName": "Hawk Merchandising Web Portal",
     "FrontendUrl": "http://localhost:4200"
   }
   ```

### Option 2: Outlook/Hotmail

```json
"Email": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@outlook.com",
  "SmtpPassword": "your-password",
  "FromEmail": "your-email@outlook.com",
  "FromName": "Hawk Merchandising Web Portal",
  "FrontendUrl": "http://localhost:4200"
}
```

### Option 3: Mailtrap (Testing - No Real Emails Sent)

1. Sign up at: https://mailtrap.io (Free tier available)
2. Get SMTP credentials from your inbox
3. Update configuration:

```json
"Email": {
  "SmtpServer": "smtp.mailtrap.io",
  "SmtpPort": 2525,
  "SmtpUsername": "your-mailtrap-username",
  "SmtpPassword": "your-mailtrap-password",
  "FromEmail": "noreply@hawkmerchandising.com",
  "FromName": "Hawk Merchandising Web Portal",
  "FrontendUrl": "http://localhost:4200"
}
```

### Option 4: SendGrid (Production Recommended)

1. Sign up at: https://sendgrid.com (Free tier: 100 emails/day)
2. Create API Key
3. Update configuration:

```json
"Email": {
  "SmtpServer": "smtp.sendgrid.net",
  "SmtpPort": 587,
  "SmtpUsername": "apikey",
  "SmtpPassword": "your-sendgrid-api-key",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "Hawk Merchandising Web Portal",
  "FrontendUrl": "https://yourdomain.com"
}
```

### Option 5: Custom SMTP Server

```json
"Email": {
  "SmtpServer": "mail.yourdomain.com",
  "SmtpPort": 587,
  "SmtpUsername": "noreply@yourdomain.com",
  "SmtpPassword": "your-password",
  "FromEmail": "noreply@yourdomain.com",
  "FromName": "Hawk Merchandising Web Portal",
  "FrontendUrl": "https://yourdomain.com"
}
```

## 🔒 Security Notes

- **Never commit** `appsettings.Development.json` or `appsettings.Production.json` with real credentials to Git
- Use environment variables or Azure Key Vault for production
- For Gmail, always use App Passwords, never your regular password
- Consider using a dedicated email account for sending system emails

## ✅ Testing

After configuration:

1. Restart the backend server
2. Go to Forgot Password page
3. Enter a valid email address
4. Check your email inbox (and spam folder)
5. You should receive a password reset email

## 🐛 Troubleshooting

**Email not sending?**
- Check backend logs for error messages
- Verify SMTP credentials are correct
- Ensure firewall allows outbound SMTP (port 587)
- For Gmail: Make sure "Less secure app access" is enabled OR use App Password
- Check spam folder

**Connection timeout?**
- Verify SMTP server and port are correct
- Check if your network blocks SMTP ports
- Try port 465 with SSL instead of 587 with TLS

## 📝 Environment Variables (Production)

For production, use environment variables instead of appsettings:

```bash
Email__SmtpServer=smtp.sendgrid.net
Email__SmtpPort=587
Email__SmtpUsername=apikey
Email__SmtpPassword=your-api-key
Email__FromEmail=noreply@yourdomain.com
Email__FromName=Hawk Merchandising Web Portal
Email__FrontendUrl=https://yourdomain.com
```
