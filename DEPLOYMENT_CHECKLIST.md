# ✅ DigitalOcean Deployment Checklist

## 🔴 CRITICAL - Must Fix Before Deployment

- [ ] **CORS Configuration** - Update `Backend/src/LogoDesignPortal.API/Program.cs` with production frontend URL
- [ ] **JWT Secret** - Generate strong key and store in environment variable
- [ ] **Database Connection** - Set up SQL Server and update connection string
- [ ] **Frontend API URL** - Update `Frontend/src/environments/environment.prod.ts` with backend URL
- [ ] **File Storage Path** - Change to absolute path (e.g., `/var/www/logodesignportal/files`)
- [ ] **Email Configuration** - Configure SMTP settings and update FrontendUrl
- [ ] **Production Config** - Create `appsettings.Production.json`
- [ ] **AllowedHosts** - Restrict to your domain(s) instead of "*"
- [ ] **Database Migrations** - Run pending migrations
- [ ] **Default Password** - Change SuperAdmin password after first login

## 🟡 RECOMMENDED - Should Fix

- [ ] **Environment Variables** - Move all secrets to environment variables
- [ ] **SSL Certificates** - Configure HTTPS/SSL
- [ ] **Logging** - Set up production logging
- [ ] **Database Backups** - Configure automated backups
- [ ] **Monitoring** - Set up health checks and alerts
- [ ] **Input Sanitization** - Re-enable middleware after fixing stream handling

## 🧪 Testing

- [ ] **Local Testing** - Test with production-like configuration locally
- [ ] **Backend Endpoints** - Verify all API endpoints work
- [ ] **Frontend Build** - Test production build locally
- [ ] **Role Testing** - Test all user roles (SuperAdmin, Admin, Designer, Client)
- [ ] **File Upload/Download** - Test file operations
- [ ] **Email** - Test password reset email
- [ ] **Order Workflow** - Test complete order lifecycle

## 🚀 Deployment Steps

### Pre-Deployment
- [ ] Review `DEPLOYMENT_READINESS_REPORT.md`
- [ ] Fix all critical issues
- [ ] Complete local testing
- [ ] Prepare deployment scripts/configs

### DigitalOcean Setup
- [ ] Create droplet or App Platform app
- [ ] Set up SQL Server database
- [ ] Configure domain names and DNS
- [ ] Set up SSL certificates
- [ ] Configure firewall rules
- [ ] Create file storage directory

### Backend Deployment
- [ ] Deploy backend application
- [ ] Configure environment variables
- [ ] Run database migrations
- [ ] Test API endpoints
- [ ] Verify CORS is working
- [ ] Check logs for errors

### Frontend Deployment
- [ ] Build production bundle
- [ ] Deploy to web server
- [ ] Update API URL in environment
- [ ] Test frontend-backend connection
- [ ] Verify all pages load correctly

### Post-Deployment
- [ ] Change default SuperAdmin password
- [ ] Test all features end-to-end
- [ ] Monitor logs for 24-48 hours
- [ ] Set up monitoring alerts
- [ ] Document deployment process

## 📝 Quick Commands

### Backend
```bash
# Run migrations
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef database update --startup-project ../LogoDesignPortal.API

# Build for production
cd Backend/src/LogoDesignPortal.API
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
# Build production bundle
cd Frontend
ng build --configuration production

# Output will be in: Frontend/dist/logo-design-portal-frontend/
```

### Generate JWT Key
```bash
# Linux/Mac
openssl rand -base64 64

# Windows PowerShell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
```

---

**Status:** ⚠️ Not Ready - Complete Critical Items First
