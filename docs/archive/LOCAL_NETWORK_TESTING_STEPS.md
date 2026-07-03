# 🌐 Local Network Testing - Step-by-Step Guide

## ✅ **Configuration Complete!**

I've already configured your files with your local IP address: **192.168.100.54**

---

## 📋 **Step-by-Step Instructions**

### **STEP 1: Configure Windows Firewall** ⚠️ IMPORTANT

**Option A: Using PowerShell (Run as Administrator)**

Open PowerShell as Administrator and run:
```powershell
New-NetFirewallRule -DisplayName "Web Portal - Backend API" -Direction Inbound -LocalPort 5000 -Protocol TCP -Action Allow
New-NetFirewallRule -DisplayName "Web Portal - Frontend Dev" -Direction Inbound -LocalPort 4200 -Protocol TCP -Action Allow
```

**Option B: Using Windows Firewall GUI**

1. Press `Windows Key` and type "Firewall"
2. Click "Windows Defender Firewall with Advanced Security"
3. Click "Inbound Rules" → "New Rule"
4. Select "Port" → Next
5. Select "TCP" and enter port **5000** → Next
6. Select "Allow the connection" → Next
7. Check all profiles → Next
8. Name it "Web Portal - Backend API" → Finish
9. Repeat for port **4200** (name it "Web Portal - Frontend Dev")

---

### **STEP 2: Start the Backend Server**

Open a **new terminal/PowerShell window** and run:

```bash
cd "Backend\src\LogoDesignPortal.API"
dotnet run
```

**Wait for this message:**
```
Now listening on: http://0.0.0.0:5000
```

✅ **Keep this terminal open!** The backend must stay running.

---

### **STEP 3: Start the Frontend Server**

Open **another new terminal/PowerShell window** and run:

```bash
cd Frontend
ng serve --host 0.0.0.0
```

**Wait for this message:**
```
Angular Live Development Server is listening on 0.0.0.0:4200
```

✅ **Keep this terminal open too!** The frontend must stay running.

---

### **STEP 4: Test on Your Computer**

Open your browser and go to:
```
http://192.168.100.54:4200
```

You should see the login page. Try logging in with:
- **Email:** `superadmin@logodesign.com`
- **Password:** `SuperAdmin@123`

---

### **STEP 5: Share with Other Users**

**Share this URL with users on your network:**
```
http://192.168.100.54:4200
```

**Requirements for other users:**
- ✅ Must be on the **same WiFi/network** as you
- ✅ Can access from any device (phone, tablet, laptop)
- ✅ No special software needed - just a web browser

---

## 🔍 **Troubleshooting**

### **Problem: Can't access from other devices**

**Solutions:**
1. ✅ Check Windows Firewall (Step 1) - This is the most common issue!
2. ✅ Verify both servers are running (Steps 2 & 3)
3. ✅ Make sure devices are on the same WiFi network
4. ✅ Try accessing from your own computer first: `http://192.168.100.54:4200`
5. ✅ Check if your IP changed: Run `ipconfig` and look for IPv4 Address

### **Problem: CORS Error in Browser**

**Solution:**
- The CORS is already configured, but if you see errors:
  - Make sure backend is running on `0.0.0.0:5000` (not just localhost)
  - Restart the backend server

### **Problem: Connection Refused**

**Solutions:**
1. ✅ Check Windows Firewall settings
2. ✅ Verify backend is running: Try `http://192.168.100.54:5000/swagger` in browser
3. ✅ Check if ports 5000 and 4200 are already in use

### **Problem: IP Address Changed**

If your IP address changes (common with DHCP), you need to:

1. **Find new IP:**
   ```powershell
   ipconfig | findstr /i "IPv4"
   ```

2. **Update these files:**
   - `Backend/src/LogoDesignPortal.API/Program.cs` (line ~94, add new IP to CORS)
   - `Frontend/src/environments/environment.ts` (line 3, update apiUrl)

3. **Restart both servers**

---

## 📝 **Quick Reference**

### **Your Configuration:**
- **Your Local IP:** `192.168.100.54`
- **Backend URL:** `http://192.168.100.54:5000`
- **Frontend URL:** `http://192.168.100.54:4200`
- **Share with users:** `http://192.168.100.54:4200`

### **Start Commands:**
```bash
# Terminal 1 - Backend
cd "Backend\src\LogoDesignPortal.API"
dotnet run

# Terminal 2 - Frontend  
cd Frontend
ng serve --host 0.0.0.0
```

### **Test URLs:**
- **Frontend:** http://192.168.100.54:4200
- **Backend API:** http://192.168.100.54:5000/swagger
- **Local Frontend:** http://localhost:4200 (still works)

---

## ✅ **Success Checklist**

- [ ] Windows Firewall configured (Step 1)
- [ ] Backend running on `0.0.0.0:5000` (Step 2)
- [ ] Frontend running on `0.0.0.0:4200` (Step 3)
- [ ] Can access from your computer (Step 4)
- [ ] Can access from another device on same network (Step 5)

---

## 🎉 **You're Ready!**

Once both servers are running, share `http://192.168.100.54:4200` with your test users!

**Note:** Both terminals must stay open while testing. Close them when done.

---

## 📞 **Need Help?**

If you encounter issues:
1. Check the Troubleshooting section above
2. Verify both servers show "listening on 0.0.0.0"
3. Check Windows Firewall settings
4. Make sure devices are on the same network

**Happy Testing! 🚀**
