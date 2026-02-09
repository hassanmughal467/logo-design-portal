# Logo and Favicon Setup Guide

## 📁 File Locations

### Logo File
**Location:** `Frontend/src/assets/images/logo.png`

**Supported Formats:**
- PNG (recommended - supports transparency)
- JPG/JPEG
- SVG (scalable vector)

**Recommended Size:**
- Width: 200-300px
- Height: 80-120px
- Aspect Ratio: 2:1 to 3:1 (landscape)

**File Name:** `logo.png` (or `logo.jpg`, `logo.svg`)

### Favicon File
**Location:** `Frontend/src/favicon.ico`

**Format:** ICO file (or PNG)
**Recommended Size:** 32x32px or 16x16px

## 📝 Steps to Add Your Logo

1. **Place your logo file:**
   - Copy your logo file to: `Frontend/src/assets/images/`
   - Name it: `logo.png` (or change the extension if using JPG/SVG)

2. **If using a different file name or format:**
   - Open `Frontend/src/app/auth/login/login.component.html`
   - Find: `src="assets/images/logo.png"`
   - Change to your file name (e.g., `src="assets/images/my-logo.jpg"`)
   - Do the same in `Frontend/src/app/auth/register/register.component.html`

3. **If logo doesn't exist (to hide it temporarily):**
   - Open `Frontend/src/app/auth/login/login.component.ts`
   - Find: `logoExists: boolean = true;`
   - Change to: `logoExists: boolean = false;`
   - Do the same in `Frontend/src/app/auth/register/register.component.ts`

## 📝 Steps to Add Your Favicon

1. **Replace the existing favicon:**
   - Copy your favicon file to: `Frontend/src/favicon.ico`
   - Replace the existing file

2. **If using PNG format:**
   - Place your PNG file at: `Frontend/src/favicon.png`
   - Open `Frontend/src/index.html`
   - Find: `<link rel="icon" type="image/x-icon" href="favicon.ico">`
   - Change to: `<link rel="icon" type="image/png" href="favicon.png">`

3. **For multiple favicon sizes (optional):**
   Add these lines in `Frontend/src/index.html` before the closing `</head>` tag:
   ```html
   <link rel="icon" type="image/png" sizes="32x32" href="favicon-32x32.png">
   <link rel="icon" type="image/png" sizes="16x16" href="favicon-16x16.png">
   <link rel="apple-touch-icon" sizes="180x180" href="apple-touch-icon.png">
   ```

## ✅ Current Configuration

- **Logo Path:** `assets/images/logo.png`
- **Favicon Path:** `favicon.ico` (in src folder)
- **Logo Display:** Shown on both Login and Registration pages
- **Logo Styling:** Automatically sized and centered

## 🎨 Logo Styling

The logo is automatically styled with:
- Max width: 180px (login) / 200px (register)
- Max height: 80px (login) / 100px (register)
- Centered alignment
- Responsive sizing

To adjust the logo size, edit:
- `Frontend/src/app/auth/login/login.component.scss` (`.logo` class)
- `Frontend/src/app/auth/register/register.component.scss` (`.logo` class)
