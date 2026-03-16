import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { PermissionsService } from './permissions.service';
import { AuthService } from './auth.service';
import { ApiService } from './api.service';

describe('PermissionsService', () => {
  let service: PermissionsService;
  let authService: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authService = jasmine.createSpyObj('AuthService', ['isAuthenticated', 'getCurrentUser']);
    authService.isAuthenticated.and.returnValue(false);
    authService.getCurrentUser.and.returnValue(null);
    (authService as any).currentUser$ = of(null);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        PermissionsService,
        ApiService,
        { provide: AuthService, useValue: authService }
      ]
    });

    service = TestBed.inject(PermissionsService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return false for hasPermission when no user', () => {
    expect(service.hasPermission('ViewAllOrders')).toBe(false);
  });

  it('should return false for hasAnyPermission when no user', () => {
    expect(service.hasAnyPermission(['ViewAllOrders', 'AssignOrder'])).toBe(false);
  });

  it('should return true for SuperAdmin hasPermission', () => {
    authService.getCurrentUser.and.returnValue({ role: 'SuperAdmin', roleName: 'SuperAdmin' } as any);
    expect(service.hasPermission('ViewAllOrders')).toBe(true);
  });

  it('should return true for hasAnyPermission when user has one', () => {
    authService.getCurrentUser.and.returnValue({ role: 'Admin' } as any);
    (service as any).userPermissionsCache?.next(['ViewAllOrders']);
    expect(service.hasAnyPermission(['ViewAllOrders', 'AssignOrder'])).toBe(true);
  });
});
