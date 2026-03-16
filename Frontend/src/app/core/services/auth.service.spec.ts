import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { ApiService } from './api.service';

describe('AuthService', () => {
  let service: AuthService;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigate']);
    localStorage.clear();
    sessionStorage.clear();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        ApiService,
        { provide: Router, useValue: router }
      ]
    });

    service = TestBed.inject(AuthService);
  });

  afterEach(() => {
    localStorage.clear();
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return false for isAuthenticated when no user', () => {
    expect(service.isAuthenticated()).toBe(false);
  });

  it('should return null for getCurrentUser when not logged in', () => {
    expect(service.getCurrentUser()).toBeNull();
  });

  it('should return false for hasRole when no user', () => {
    expect(service.hasRole('Admin')).toBe(false);
  });

  it('should return false for hasAnyRole when no user', () => {
    expect(service.hasAnyRole(['Admin', 'Client'])).toBe(false);
  });
});
