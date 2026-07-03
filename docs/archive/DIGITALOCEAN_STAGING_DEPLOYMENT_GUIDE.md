# DigitalOcean Staging Deployment Guide

Complete guide to deploy the Logo Design Portal to DigitalOcean for staging and testing before production.

---

## Table of Contents
1. [DigitalOcean Package Recommendations](#1-digitalocean-package-recommendations)
2. [Architecture Overview](#2-architecture-overview)
3. [Database Consideration (Important)](#3-database-consideration-important)
4. [Deployment Option A: App Platform (Easiest)](#4-deployment-option-a-app-platform-easiest)
5. [Deployment Option B: Droplets (More Control)](#5-deployment-option-b-droplets-more-control)
6. [Pre-Deployment Checklist](#6-pre-deployment-checklist)
7. [Step-by-Step: App Platform Deployment](#7-step-by-step-app-platform-deployment)
8. [Step-by-Step: Droplet Deployment](#8-step-by-step-droplet-deployment)
9. [Post-Deployment Verification](#9-post-deployment-verification)
10. [Cost Summary](#10-cost-summary)

---

## 1. DigitalOcean Package Recommendations
 
### For Staging & Testing (Budget-Friendly)

| Component | Recommended Package | Monthly Cost | Notes |
|-----------|---------------------|--------------|-------|
| **Backend API** | Basic Droplet 1 GB or App Platform $5 | $5–6 | Sufficient for staging traffic |
| **Frontend** | Static Site (Free) or same droplet | $0–5 | Angular SPA can be static |
| **Database** | Azure SQL Free Tier OR Droplet with SQL Server | $0–6 | See Database section below |
| **Total (Staging)** | | **$5–17/month** | |

### For Production (After Staging Validation)

| Component | Recommended Package | Monthly Cost |
|-----------|---------------------|--------------|
| **Backend API** | Basic Droplet 2 GB or App Platform $12 | $12–15 |
| **Frontend** | Static Site or CDN | $0–5 |
| **Database** | Managed DB or dedicated droplet | $15–25 |
| **Total (Production)** | | **$27–45/month** |

### DigitalOcean Pricing Quick Reference

- **Droplets**: $4/mo (512MB) → $6/mo (1GB) → $12/mo (2GB) → $18/mo (4GB)
- **App Platform**: $5/mo (Basic) → $12/mo (Professional) per component
- **Managed PostgreSQL**: $15/mo (1GB) — *Note: Your app uses SQL Server, not PostgreSQL*
- **Spaces (Object Storage)**: $5/mo for 250GB — useful for file uploads
- **Free Credits**: New accounts often get $200 credit for 60 days

---

## 2. Architecture Overview

```
                    ┌─────────────────────────────────────┐
                    │         DigitalOcean                 │
                    │                                      │
   User Browser ──► │  Frontend (Angular SPA)              │
                    │  - Static site or Nginx              │
                    │  - https://staging.yourdomain.com    │
                    │                                      │
                    │  Backend (ASP.NET Core 8 API)        │
                    │  - https://api-staging.yourdomain.com│
                    │                                      │
                    │  Database (SQL Server)               │
                    │  - Azure SQL / Self-hosted droplet   │
                    └─────────────────────────────────────┘
```

---

## 3. Database Consideration (Important)

**DigitalOcean does NOT offer managed SQL Server.** Your app uses SQL Server with Entity Framework. You have three options:

### Option A: Azure SQL Database (Recommended for Staging)
- **Cost**: Free tier (up to 32GB) or ~$5/mo
- **Pros**: Fully managed, no server maintenance, works with your existing EF Core code
- **Setup**: Create at [portal.azure.com](https://portal.azure.com) → SQL databases → Create
- **Connection**: Use connection string from Azure in your DigitalOcean app

### Option B: SQL Server on a Droplet (Self-Hosted)
- **Cost**: $6/mo (1GB droplet) + droplet cost
- **Pros**: Everything on DigitalOcean, full control
- **Cons**: You manage backups, updates, security
- **Setup**: Install SQL Server Express (free) on Ubuntu droplet

### Option C: Migrate to PostgreSQL (Future Consideration)
- **Cost**: $15/mo DigitalOcean Managed PostgreSQL
- **Pros**: Managed, backups, HA — all on DigitalOcean
- **Cons**: Requires code changes (EF Core supports both, but migrations differ)

**For staging: Use Option A (Azure SQL Free) or Option B (SQL Server on droplet).**

---

## 4. Deployment Option A: App Platform (Easiest)

**Best for**: Quick setup, less DevOps, automatic SSL, GitHub integration

### Pros
- Automatic SSL certificates
- GitHub auto-deploy on push
- Environment variables in UI
- No server management

### Cons
- Slightly higher cost
- SQL Server must be external (Azure, etc.)
- Less control over server

### Components Needed
1. **Web Service** (Backend) — .NET 8 API
2. **Static Site** (Frontend) — Angular build output
3. **Database** — External (Azure SQL) or add-on

---

## 5. Deployment Option B: Droplets (More Control)

**Best for**: Cost control, full control, SQL Server on same provider

### Pros
- Cheaper at scale
- Can run SQL Server on same/different droplet
- Full control over configuration

### Cons
- Manual SSL setup (Let's Encrypt)
- Manual deployment (Git, scripts)
- You manage updates and security

### Components Needed
1. **Droplet 1** (2GB): Backend API + Nginx reverse proxy
2. **Droplet 2** (1GB, optional): SQL Server OR use Azure SQL
3. **Frontend**: Served by Nginx on Droplet 1 or separate static hosting

---

## 6. Pre-Deployment Checklist

Complete these **before** deploying. See `DEPLOYMENT_CHECKLIST.md` for full list.

### Critical (Must Do)

- [ ] **Create `appsettings.Production.json`** — Production-specific config
- [ ] **CORS**: Add staging frontend URL to `Program.cs`
- [ ] **JWT Key**: Generate strong key, use environment variable
- [ ] **Database**: Set up Azure SQL or SQL Server on droplet
- [ ] **Frontend `environment.prod.ts`**: Set `apiUrl` to your backend URL
- [ ] **File Storage Path**: Use absolute path (e.g., `/var/www/files`)
- [ ] **Run migrations**: `dotnet ef database update`

### Generate JWT Key (PowerShell)
```powershell
[Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Maximum 256 }))
```

### Update CORS in Program.cs
```csharp
policy.WithOrigins(
    "http://localhost:4200", 
    "https://localhost:4200",
    "https://staging.yourdomain.com",      // Add your staging URL
    "https://your-app.ondigitalocean.app"  // App Platform URL
)
```

---

## 7. Step-by-Step: App Platform Deployment

### Step 1: Prepare Your Repository

1. Ensure code is pushed to GitHub/GitLab
2. Add `Dockerfile` (optional but recommended) or use buildpacks

**Backend Dockerfile** (create in `Backend/src/LogoDesignPortal.API/`):
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["LogoDesignPortal.API/LogoDesignPortal.API.csproj", "LogoDesignPortal.API/"]
COPY ["LogoDesignPortal.Application/LogoDesignPortal.Application.csproj", "LogoDesignPortal.Application/"]
COPY ["LogoDesignPortal.Domain/LogoDesignPortal.Domain.csproj", "LogoDesignPortal.Domain/"]
COPY ["LogoDesignPortal.Infrastructure/LogoDesignPortal.Infrastructure.csproj", "LogoDesignPortal.Infrastructure/"]
RUN dotnet restore "LogoDesignPortal.API/LogoDesignPortal.API.csproj"
COPY . .
RUN dotnet build "LogoDesignPortal.API/LogoDesignPortal.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LogoDesignPortal.API/LogoDesignPortal.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LogoDesignPortal.API.dll"]
```

### Step 2: Create App on DigitalOcean

1. Log in to [cloud.digitalocean.com](https://cloud.digitalocean.com)
2. **Apps** → **Create App**
3. **Choose Source**: GitHub → Authorize → Select your repo
4. **Branch**: `develop` (or your staging branch)

### Step 3: Configure Backend Component

1. **Resource Type**: Web Service
2. **Source**: `Backend/src/LogoDesignPortal.API` (or root if Dockerfile at root)
3. **Build Command**: 
   - With Dockerfile: (auto-detected)
   - Without: `dotnet publish -c Release -o ./publish`
4. **Run Command**: `dotnet LogoDesignPortal.API.dll`
5. **HTTP Port**: 8080

### Step 4: Add Frontend Component

1. **Add Resource** → **Static Site**
2. **Source**: `Frontend`
3. **Build Command**: `npm ci && npm run build`
4. **Output Directory**: `dist/logo-design-portal-frontend` (check your Angular config)

### Step 5: Set Environment Variables

In App Platform → Your App → Settings → App-Level Environment Variables:

| Variable | Value | Encrypted |
|----------|-------|-----------|
| `ASPNETCORE_ENVIRONMENT` | Production | No |
| `ConnectionStrings__DefaultConnection` | Your Azure SQL connection string | Yes |
| `Jwt__Key` | Your generated JWT key | Yes |
| `FileStorage__Path` | /tmp/files (or persistent volume path) | No |
| `AllowedOrigins` | https://your-frontend-url.ondigitalocean.app | No |

### Step 6: Add Persistent Storage (for file uploads)

1. **Add Resource** → **Volume**
2. Mount path: `/app/files`
3. Attach to Backend component

### Step 7: Deploy

1. Click **Create Resources** or **Deploy**
2. Wait 5–10 minutes for first build
3. Note your URLs: `https://your-backend-xxxxx.ondigitalocean.app`

### Step 8: Update Frontend API URL

After backend deploys, update frontend build to use backend URL:
- Add env var or update `environment.prod.ts` with backend URL
- Rebuild/redeploy frontend

---

## 8. Step-by-Step: Droplet Deployment

### Step 1: Create Droplet

1. **Droplets** → **Create Droplet**
2. **Image**: Ubuntu 24.04 LTS
3. **Plan**: Basic $12/mo (2 GB RAM, 1 vCPU) for staging
4. **Datacenter**: Choose nearest to users
5. **Authentication**: SSH key (recommended)
6. **Hostname**: `logoportal-staging`

### Step 2: Initial Server Setup

```bash
# SSH into droplet
ssh root@your-droplet-ip

# Update system
apt update && apt upgrade -y

# Create app user
adduser logoportal
usermod -aG sudo logoportal
su - logoportal
```

### Step 3: Install .NET 8 Runtime

```bash
wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-8.0
```

### Step 4: Install Nginx

```bash
sudo apt install -y nginx
sudo systemctl enable nginx
```

### Step 5: Install SQL Server (if self-hosting)

```bash
# Add Microsoft repo
curl https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -
curl https://packages.microsoft.com/config/ubuntu/24.04/mssql-server-2022.list | sudo tee /etc/apt/sources.list.d/mssql-server.list

sudo apt update
sudo apt install -y mssql-server

# Configure SQL Server
sudo /opt/mssql/bin/mssql-conf setup
# Choose edition (2 for Express - free), set SA password

sudo systemctl status mssql-server
```

### Step 6: Deploy Backend

```bash
# On your local machine - publish
cd Backend/src/LogoDesignPortal.API
dotnet publish -c Release -o ./publish

# Copy to server (from local)
scp -r ./publish/* logoportal@your-droplet-ip:/home/logoportal/api/

# On server - create systemd service
sudo nano /etc/systemd/system/logoportal-api.service
```

**Service file** (`/etc/systemd/system/logoportal-api.service`):
```ini
[Unit]
Description=Logo Design Portal API
After=network.target

[Service]
WorkingDirectory=/home/logoportal/api
ExecStart=/usr/bin/dotnet /home/logoportal/api/LogoDesignPortal.API.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=logoportal-api
User=logoportal
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl daemon-reload
sudo systemctl enable logoportal-api
sudo systemctl start logoportal-api
sudo systemctl status logoportal-api
```

### Step 7: Configure Nginx

```bash
sudo nano /etc/nginx/sites-available/logoportal
```

```nginx
server {
    listen 80;
    server_name api-staging.yourdomain.com;  # or droplet IP for testing

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
sudo ln -s /etc/nginx/sites-available/logoportal /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

### Step 8: Install SSL (Let's Encrypt)

```bash
sudo apt install -y certbot python3-certbot-nginx
sudo certbot --nginx -d api-staging.yourdomain.com
# Follow prompts
```

### Step 9: Deploy Frontend

```bash
# On local - build
cd Frontend
ng build --configuration production

# Copy to server
scp -r dist/logo-design-portal-frontend/* logoportal@your-droplet-ip:/var/www/logoportal-frontend/

# Add to Nginx (separate server block or same server)
# Serve static files from /var/www/logoportal-frontend
```

### Step 10: Create File Storage Directory

```bash
sudo mkdir -p /var/www/logoportal/files
sudo chown -R logoportal:logoportal /var/www/logoportal
```

---

## 9. Post-Deployment Verification

### Backend Checks
- [ ] `https://your-api-url/swagger` loads (if enabled in production)
- [ ] `POST /api/v1/auth/login` works with SuperAdmin credentials
- [ ] Health/status endpoint responds
- [ ] CORS allows frontend origin

### Frontend Checks
- [ ] Login page loads
- [ ] Can log in and see dashboard
- [ ] API calls succeed (check browser Network tab)
- [ ] File upload works (if applicable)

### Security
- [ ] Change SuperAdmin default password immediately
- [ ] HTTPS works (no mixed content)
- [ ] JWT tokens are validated

---

## 10. Cost Summary

### Minimal Staging Setup (App Platform)
| Item | Cost |
|------|------|
| Backend (Basic) | $5/mo |
| Frontend (Static) | $0 (free tier) |
| Azure SQL (Free tier) | $0 |
| **Total** | **~$5/mo** |

### Staging with Droplet
| Item | Cost |
|------|------|
| Droplet 2GB | $12/mo |
| Azure SQL Free | $0 |
| **Total** | **~$12/mo** |

### Staging with SQL Server on Droplet
| Item | Cost |
|------|------|
| Droplet 2GB (API + Nginx) | $12/mo |
| Droplet 1GB (SQL Server) | $6/mo |
| **Total** | **~$18/mo** |

---

## Quick Start Commands

```bash
# Backend - Publish
cd Backend/src/LogoDesignPortal.API
dotnet publish -c Release -o ./publish

# Frontend - Build
cd Frontend
npm run build -- --configuration production

# Run migrations (with connection string)
cd Backend/src/LogoDesignPortal.Infrastructure
dotnet ef database update --startup-project ../LogoDesignPortal.API
```

---

## Additional Resources

- [DigitalOcean App Platform Docs](https://docs.digitalocean.com/products/app-platform/)
- [DigitalOcean Droplet Guide](https://docs.digitalocean.com/products/droplets/)
- [Azure SQL Free Tier](https://azure.microsoft.com/free/)
- [SQL Server on Linux](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-overview)
- Your existing: `DEPLOYMENT_CHECKLIST.md`, `DEPLOYMENT_READINESS_REPORT.md`

---

**Next Steps**: Complete the Pre-Deployment Checklist, choose App Platform or Droplet, then follow the corresponding step-by-step section.
