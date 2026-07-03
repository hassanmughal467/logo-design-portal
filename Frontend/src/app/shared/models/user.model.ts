export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role?: UserRole;
  roleName?: string; // From API response
  isActive: boolean;
  isRootAdmin?: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

export enum UserRole {
  SuperAdmin = 'SuperAdmin',
  Admin = 'Admin',
  Designer = 'Designer',
  Client = 'Client'
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;           // Backend returns 'token' not 'accessToken'
  refreshToken?: string;
  user: User;
  expiresAt: string;      // Backend returns 'expiresAt' (DateTime) not 'expiresIn' (number)
  /** Set when cookie auth is enabled (API host cookies + SPA sessionStorage). */
  csrfToken?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  companyName: string;
  secondaryEmail?: string | null;
  agreeToTerms: boolean;
}
