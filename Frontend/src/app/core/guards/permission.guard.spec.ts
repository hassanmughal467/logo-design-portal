import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { PermissionGuard } from './permission.guard';
import { AuthService } from '../services/auth.service';
import { PermissionsService } from '../services/permissions.service';

describe('PermissionGuard', () => {
  let guard: PermissionGuard;
  let authService: jasmine.SpyObj<AuthService>;
  let permissionsService: jasmine.SpyObj<PermissionsService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['isAuthenticated']);
    permissionsService = jasmine.createSpyObj('PermissionsService', ['hasAnyPermission']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        PermissionGuard,
        { provide: AuthService, useValue: authService },
        { provide: PermissionsService, useValue: permissionsService },
        { provide: Router, useValue: router }
      ]
    });

    guard = TestBed.inject(PermissionGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  it('should allow activation when no permissions required', () => {
    const route = { data: {} } as any;
    const state = {} as any;

    expect(guard.canActivate(route, state)).toBe(true);
  });

  it('should redirect to login when not authenticated', () => {
    authService.isAuthenticated.and.returnValue(false);
    const route = { data: { permissions: ['ViewAllOrders'] } } as any;
    const state = { url: '/orders' } as any;

    expect(guard.canActivate(route, state)).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/orders' } });
  });

  it('should allow activation when user has required permission', () => {
    authService.isAuthenticated.and.returnValue(true);
    permissionsService.hasAnyPermission.and.returnValue(true);
    const route = { data: { permissions: ['ViewAllOrders'] } } as any;
    const state = {} as any;

    expect(guard.canActivate(route, state)).toBe(true);
  });

  it('should redirect to dashboard when user lacks required permission', () => {
    authService.isAuthenticated.and.returnValue(true);
    permissionsService.hasAnyPermission.and.returnValue(false);
    const route = { data: { permissions: ['ViewAllOrders'] } } as any;
    const state = {} as any;

    expect(guard.canActivate(route, state)).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });
});
