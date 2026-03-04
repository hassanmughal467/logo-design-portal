# ⚡ Quick Restart Guide

## 🔄 **After Making Code Changes**

### **Backend Changes (C#/.NET):**
```bash
# 1. Stop: Press Ctrl+C in backend terminal
# 2. Start:
cd Backend\src\LogoDesignPortal.API
dotnet run --urls http://0.0.0.0:5000
```

### **Frontend Changes (Angular/TypeScript):**
- ✅ **Auto-reloads!** Just save and browser refreshes
- ❌ **No restart needed** (unless you changed environment.ts)

---

## 🌐 **If IP Address Changed**

### **Option 1: Use Script (Easiest)**
```powershell
.\update-ip.ps1
```
This will:
- ✅ Detect your new IP automatically
- ✅ Update `Program.cs` (CORS)
- ✅ Update `environment.ts` (apiUrl)
- ✅ Show you the new URL to share

**Then restart both servers!**

### **Option 2: Manual Update**

1. **Find new IP:**
   ```powershell
   ipconfig | findstr /i "IPv4"
   ```

2. **Update 2 files:**
   - `Backend/src/LogoDesignPortal.API/Program.cs` (line ~94)
   - `Frontend/src/environments/environment.ts` (line 3)

3. **Replace old IP with new IP**

4. **Restart both servers**

5. **Share new URL:** `http://NEW_IP:4200`

---

## 📋 **Quick Checklist**

### **After Code Changes:**
- [ ] Backend changed? → Restart backend
- [ ] Frontend changed? → Usually auto-reloads (no restart)
- [ ] Environment changed? → Restart affected server

### **After IP Change:**
- [ ] Run `update-ip.ps1` OR update files manually
- [ ] Restart backend
- [ ] Restart frontend
- [ ] Share new URL

---

## 🎯 **One-Liner Commands**

### **Restart Backend:**
```bash
cd Backend\src\LogoDesignPortal.API && dotnet run --urls http://0.0.0.0:5000
```

### **Restart Frontend:**
```bash
cd Frontend && npm run start -- --host 0.0.0.0
```

### **Check Current IP:**
```powershell
ipconfig | findstr /i "IPv4"
```

---

**See `RESTART_AND_IP_CHANGE_GUIDE.md` for detailed instructions!**
