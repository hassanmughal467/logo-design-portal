export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role?: UserRole;
  roleName?: string; // From API response
  isActive: boolean;
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
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  companyName: string;
  phoneNumber: string;
}
