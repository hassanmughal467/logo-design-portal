# Logo Design Portal - Frontend

Angular 17+ premium admin portal with PrimeNG UI framework.

## 🚀 Getting Started

### Prerequisites
- Node.js 18+ and npm
- Angular CLI 17+

### Installation

1. Install dependencies:
```bash
npm install
```

2. Start development server:
```bash
npm start
```

The app will be available at `http://localhost:4200`

## 📁 Project Structure

```
src/app/
├── core/              # Core services, guards, interceptors
├── auth/              # Authentication module (lazy loaded)
├── dashboard/         # Dashboard module (lazy loaded)
├── users/             # Users management (lazy loaded)
├── orders/            # Orders management (lazy loaded)
├── designers/         # Designer profiles (lazy loaded)
├── permissions/       # Permissions management (lazy loaded)
├── files/             # File management (lazy loaded)
├── shared/            # Shared components, directives, pipes
└── layout/            # Layout components
```

## 🔧 Configuration

### API Configuration
Update `src/environments/environment.ts` to configure API URL:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  apiVersion: 'v1'
};
```

## 🎨 UI Framework

This project uses **PrimeNG exclusively** - no other UI libraries.

## 🔐 Authentication

- Access tokens stored in memory (secure)
- Refresh tokens in HttpOnly cookies (if backend supports)
- Route guards for authentication and authorization
- Backend is the source of truth for all security checks

## 📦 Build

```bash
npm run build
```

## 🧪 Testing

```bash
npm test
```
