import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RoleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';

describe('RoleGuard', () => {
  let guard: RoleGuard;
  let authService: jasmine.SpyObj<AuthService>;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'hasAnyRole']);
    router = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        RoleGuard,
        { provide: AuthService, useValue: authService },
        { provide: Router, useValue: router }
      ]
    });

    guard = TestBed.inject(RoleGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  it('should allow activation when no roles required', () => {
    const route = { data: {} } as any;
    const state = {} as any;

    expect(guard.canActivate(route, state)).toBe(true);
  });

  it('should redirect to login when not authenticated', () => {
    authService.isAuthenticated.and.returnValue(false);
    const route = { data: { roles: ['Admin'] } } as any;
    const state = { url: '/users' } as any;

    expect(guard.canActivate(route, state)).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/users' } });
  });

  it('should allow activation when user has required role', () => {
    authService.isAuthenticated.and.returnValue(true);
    authService.hasAnyRole.and.returnValue(true);
    const route = { data: { roles: ['Admin'] } } as any;
    const state = {} as any;

    expect(guard.canActivate(route, state)).toBe(true);
  });

  it('should redirect to dashboard when user lacks required role', () => {
    authService.isAuthenticated.and.returnValue(true);
    authService.hasAnyRole.and.returnValue(false);
    const route = { data: { roles: ['Admin'] } } as any;
    const state = {} as any;

    expect(guard.canActivate(route, state)).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/dashboard']);
  });
});
