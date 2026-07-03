# 🚀 Quick Reference - Multi-User Testing

## ✅ **Everything is Working!**

Your web portal is now set up for local network testing.

---

## 🌐 **Access URLs**

### **On Your Computer:**
- **Frontend:** http://localhost:4200
- **Backend API:** http://localhost:5000/swagger

### **Share with Other Users (Same Network):**
- **URL:** http://192.168.100.54:4200
- **Login Credentials:**
  - Email: `superadmin@logodesign.com`
  - Password: `SuperAdmin@123`

---

## ⚠️ **Important Reminders**

### **For Network Access (Other Devices):**
1. **Configure Firewall** (One-time setup)
   - Run `configure-firewall-admin.ps1` as Administrator
   - OR manually allow ports 5000 and 4200 in Windows Firewall

2. **Keep Servers Running**
   - Don't close the PowerShell windows
   - Both servers must stay running while testing

3. **Same Network Required**
   - Test users must be on the same WiFi/network as you

---

## 🎯 **Quick Commands**

### **Start Backend:**
```bash
cd Backend\src\LogoDesignPortal.API
dotnet run --urls http://0.0.0.0:5000
```

### **Start Frontend:**
```bash
cd Frontend
npm run start -- --host 0.0.0.0
```

**OR use the batch files:**
- `start-backend.bat`
- `start-frontend-fixed.bat`

---

## 📋 **Testing Checklist**

- [x] Backend server running
- [x] Frontend server running
- [x] Can access locally
- [ ] Firewall configured (for network access)
- [ ] Tested from another device
- [ ] Multiple users can login simultaneously

---

## 🐛 **If Something Stops Working**

### **Servers Stopped?**
- Restart using the batch files or commands above
- Check for error messages in the PowerShell windows

### **Can't Access from Other Devices?**
- ✅ Configure firewall (see above)
- ✅ Verify both servers show "0.0.0.0" not just "localhost"
- ✅ Check devices are on same WiFi network
- ✅ Verify IP address hasn't changed: `ipconfig`

### **IP Address Changed?**
If your IP changes, update:
1. `Backend/src/LogoDesignPortal.API/Program.cs` (CORS - line ~94)
2. `Frontend/src/environments/environment.ts` (apiUrl - line 3)

---

## 📚 **Documentation**

- **Full Guide:** `MULTI_USER_TESTING_GUIDE.md`
- **Step-by-Step:** `LOCAL_NETWORK_TESTING_STEPS.md`
- **Fixed Issues:** `FIXED_STARTUP_GUIDE.md`
- **Quick Start:** `START_TESTING.md`

---

## 🎉 **You're All Set!**

**Current Status:**
- ✅ Backend: Running on http://0.0.0.0:5000
- ✅ Frontend: Running on http://0.0.0.0:4200
- ✅ Local Access: Working
- ⚠️ Network Access: Configure firewall to enable

**Next Steps:**
1. Configure firewall for network access
2. Test from another device
3. Share URL with test users
4. Start multi-user testing!

---

**Happy Testing! 🚀**
