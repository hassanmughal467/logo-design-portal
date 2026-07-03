# 🔄 Restart Guide & IP Change Handling

## 📋 **Quick Restart After Making Changes**

### **Scenario 1: You Made Code Changes**

#### **For Backend Changes (.NET/C#):**
1. **Stop the backend server:**
   - Close the "Backend Server" PowerShell window
   - OR press `Ctrl+C` in the backend terminal

2. **Start backend again:**
   ```bash
   cd Backend\src\LogoDesignPortal.API
   dotnet run --urls http://0.0.0.0:5000
   ```
   OR double-click: `start-backend.bat`

3. **Wait for:** `Now listening on: http://0.0.0.0:5000`

#### **For Frontend Changes (Angular/TypeScript/HTML/SCSS):**
1. **Frontend auto-reloads!** 
   - Most changes (HTML, SCSS, TypeScript) will automatically refresh
   - Just save your file and the browser will update
   - No need to restart!

2. **If changes don't appear:**
   - Hard refresh browser: `Ctrl+F5` or `Ctrl+Shift+R`
   - Check the frontend terminal for errors

3. **Only restart if:**
   - You changed `environment.ts` or `angular.json`
   - You installed new npm packages
   - Frontend server crashed

   **To restart frontend:**
   - Close the "Frontend Server" PowerShell window
   - OR press `Ctrl+C` in the frontend terminal
   - Then start again:
     ```bash
     cd Frontend
     npm run start -- --host 0.0.0.0
     ```
     OR double-click: `start-frontend-fixed.bat

---

## 🌐 **Scenario 2: Network/IP Address Changed**

### **How to Check Your Current IP:**
```powershell
ipconfig | findstr /i "IPv4"
```

**OR in PowerShell:**
```powershell
Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.InterfaceAlias -notlike "*Loopback*"} | Select-Object IPAddress
```

### **If Your IP Changed, Update These Files:**

#### **Step 1: Update Backend CORS**

**File:** `Backend/src/LogoDesignPortal.API/Program.cs`

**Find this section (around line 87-97):**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200", 
            "https://localhost:4200",
            "http://192.168.100.54:4200"  // ← Update this IP
        )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

**Replace `192.168.100.54` with your new IP address.**

**Example if new IP is `192.168.1.50`:**
```csharp
policy.WithOrigins(
    "http://localhost:4200", 
    "https://localhost:4200",
    "http://192.168.1.50:4200"  // ← New IP
)
```

#### **Step 2: Update Frontend Environment**

**File:** `Frontend/src/environments/environment.ts`

**Find this:**
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://192.168.100.54:5000',  // ← Update this IP
  apiVersion: ''
};
```

**Replace `192.168.100.54` with your new IP address.**

**Example if new IP is `192.168.1.50`:**
```typescript
apiUrl: 'http://192.168.1.50:5000',  // ← New IP
```

#### **Step 3: Restart Both Servers**

1. **Stop backend** (close window or Ctrl+C)
2. **Stop frontend** (close window or Ctrl+C)
3. **Start backend again:**
   ```bash
   cd Backend\src\LogoDesignPortal.API
   dotnet run --urls http://0.0.0.0:5000
   ```
4. **Start frontend again:**
   ```bash
   cd Frontend
   npm run start -- --host 0.0.0.0
   ```

#### **Step 4: Update Shared URL**

**New URL to share:** `http://YOUR_NEW_IP:4200`

**Example:** `http://192.168.1.50:4200`

---

## 🔧 **Quick IP Update Script**

I'll create a script to help you update the IP automatically. But for now, here's the manual process:

### **Manual Update Checklist:**

- [ ] Find new IP: `ipconfig | findstr /i "IPv4"`
- [ ] Update `Backend/src/LogoDesignPortal.API/Program.cs` (line ~94)
- [ ] Update `Frontend/src/environments/environment.ts` (line 3)
- [ ] Restart backend server
- [ ] Restart frontend server
- [ ] Update shared URL with new IP

---

## 📝 **Common Scenarios**

### **Scenario A: Just Testing Locally (No Network Access Needed)**

**No changes needed!** Just:
- Keep servers running
- Use: http://localhost:4200
- No IP updates required

### **Scenario B: Testing with Network Users**

**If IP changed:**
1. Update CORS in `Program.cs`
2. Update `environment.ts`
3. Restart both servers
4. Share new URL

### **Scenario C: Changed WiFi Network**

**Same steps as IP change:**
1. Get new IP address
2. Update both files
3. Restart servers
4. Share new URL

### **Scenario D: Backend Code Changes**

1. Stop backend (Ctrl+C or close window)
2. Start backend again
3. Frontend will reconnect automatically

### **Scenario E: Frontend Code Changes**

**Usually no restart needed:**
- Save file → Browser auto-refreshes
- If not working: Hard refresh (Ctrl+F5)

**Restart only if:**
- Changed environment files
- Installed new packages
- Server crashed

---

## 🚀 **Quick Restart Commands**

### **Restart Backend Only:**
```bash
# Stop: Press Ctrl+C or close window
# Start:
cd Backend\src\LogoDesignPortal.API
dotnet run --urls http://0.0.0.0:5000
```

### **Restart Frontend Only:**
```bash
# Stop: Press Ctrl+C or close window
# Start:
cd Frontend
npm run start -- --host 0.0.0.0
```

### **Restart Both:**
1. Stop both servers (close windows or Ctrl+C)
2. Start backend (use command above or `start-backend.bat`)
3. Start frontend (use command above or `start-frontend-fixed.bat`)

---

## 🔍 **How to Know If IP Changed**

### **Signs Your IP Changed:**
- ❌ Other devices can't connect (but were working before)
- ❌ You see connection errors
- ❌ You switched WiFi networks
- ❌ Router was restarted

### **Check Current IP:**
```powershell
ipconfig | findstr /i "IPv4"
```

**Compare with:**
- Current IP in `Program.cs` (line ~94)
- Current IP in `environment.ts` (line 3)

**If different → Update both files!**

---

## 📋 **Quick Reference Table**

| Situation | Backend Restart? | Frontend Restart? | Update IP? |
|-----------|----------------|-------------------|------------|
| Changed C# code | ✅ Yes | ❌ No | ❌ No |
| Changed TypeScript/HTML/SCSS | ❌ No | ❌ No (auto-reload) | ❌ No |
| Changed environment.ts | ❌ No | ✅ Yes | ❌ No |
| IP address changed | ✅ Yes | ✅ Yes | ✅ Yes |
| Switched WiFi | ✅ Yes | ✅ Yes | ✅ Yes |
| Installed npm packages | ❌ No | ✅ Yes | ❌ No |
| Server crashed | ✅ Yes | ✅ Yes | ❌ No |

---

## 🎯 **Step-by-Step: After Making Changes**

### **1. Made Backend Changes?**
```
Stop Backend → Start Backend → Test
```

### **2. Made Frontend Changes?**
```
Save File → Browser Auto-Refreshes → Test
(No restart needed usually)
```

### **3. IP Changed?**
```
Check IP → Update Program.cs → Update environment.ts → Restart Both → Share New URL
```

---

## 💡 **Pro Tips**

1. **Keep both terminal windows visible** so you can see errors
2. **Check terminal for errors** if something doesn't work
3. **Hard refresh browser** (Ctrl+F5) if frontend changes don't appear
4. **Save IP address** in a note for quick reference
5. **Use batch files** (`start-backend.bat`, `start-frontend-fixed.bat`) for easy restart

---

## ✅ **Summary**

### **After Code Changes:**
- **Backend changes:** Restart backend only
- **Frontend changes:** Usually no restart (auto-reload)
- **Environment changes:** Restart affected server

### **After IP Change:**
1. Find new IP: `ipconfig`
2. Update `Program.cs` (CORS)
3. Update `environment.ts` (apiUrl)
4. Restart both servers
5. Share new URL

**That's it! 🎉**
