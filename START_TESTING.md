# 🚀 Quick Start - Local Network Testing

## ✅ **Everything is Configured!**

I've set up your web portal for local network testing with IP: **192.168.100.54**

---

## 🎯 **3 Simple Steps:**

### **1. Configure Firewall** (One-time setup)

**Run this in PowerShell as Administrator:**
```powershell
New-NetFirewallRule -DisplayName "Web Portal - Backend API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Web Portal - Frontend Dev" -Direction Inbound -LocalPort 4200 -Protocol TCP -Action Allow
```

**OR** use the Windows Firewall GUI (see `LOCAL_NETWORK_TESTING_STEPS.md` for details)

---

### **2. Start Backend** 

**Option A: Double-click this file:**
```
start-backend.bat
```

**Option B: Run in terminal:**
```bash
cd Backend\src\LogoDesignPortal.API
dotnet run
```

**Wait for:** `Now listening on: http://0.0.0.0:5000`

---

### **3. Start Frontend**

**Open a NEW terminal window, then:**

**Option A: Double-click this file:**
```
start-frontend.bat
```

**Option B: Run in terminal:**
```bash
cd Frontend
ng serve --host 0.0.0.0
```

**Wait for:** `Angular Live Development Server is listening on 0.0.0.0:4200`

---

## 🌐 **Access Your Portal:**

**On your computer:**
- http://localhost:4200
- http://192.168.100.54:4200

**Share with other users (same network):**
- http://192.168.100.54:4200

---

## 🔑 **Login Credentials:**

- **Email:** `superadmin@logodesign.com`
- **Password:** `SuperAdmin@123`

---

## ⚠️ **Important:**

1. **Keep both terminal windows open** while testing
2. **Both servers must be running** for the portal to work
3. **Users must be on the same WiFi/network** as you
4. **If firewall wasn't configured**, other devices won't be able to connect

---

## 🐛 **Quick Troubleshooting:**

**Can't access from other devices?**
- ✅ Check Windows Firewall (Step 1)
- ✅ Verify both servers are running
- ✅ Make sure devices are on same WiFi

**See `LOCAL_NETWORK_TESTING_STEPS.md` for detailed troubleshooting**

---

## 📋 **What I Changed:**

✅ Updated CORS in `Backend/src/LogoDesignPortal.API/Program.cs`  
✅ Updated backend to listen on `0.0.0.0:5000`  
✅ Updated frontend environment to use your IP  
✅ Created helper scripts for easy startup  

**Everything is ready! Just start both servers! 🎉**
