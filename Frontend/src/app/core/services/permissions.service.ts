import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError, take } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { Permission } from '@shared/models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private permissionsCache = new BehaviorSubject<Permission[]>([]);
  public permissions$ = this.permissionsCache.asObservable();

  /** Current user's permission names (from /api/users/me/permissions). Used for Admin and other roles. */
  private userPermissionsCache = new BehaviorSubject<string[]>([]);

  constructor(
    private apiService: ApiService,
    private authService: AuthService
  ) {
    if (this.authService.isAuthenticated()) {
      const user = this.authService.getCurrentUser();
      if (user && (user.role === 'SuperAdmin' || user.roleName === 'SuperAdmin')) {
        this.loadPermissions().subscribe();
      }
      this.loadUserPermissions().subscribe();
    }
    // Reload user permissions when auth state changes (e.g. on login); clear on logout
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.loadUserPermissions().pipe(take(1)).subscribe();
      } else {
        this.userPermissionsCache.next([]);
      }
    });
  }

  loadPermissions(): Observable<Permission[]> {
    return this.apiService.get<Permission[]>('permissions').pipe(
      map(permissions => {
        this.permissionsCache.next(permissions);
        return permissions;
      }),
      catchError(error => {
        console.error('Error loading permissions:', error);
        return of([]);
      })
    );
  }

  /** Load current user's permissions from /api/users/me/permissions */
  loadUserPermissions(): Observable<string[]> {
    return this.apiService.get<string[]>('users/me/permissions').pipe(
      map(permissions => {
        this.userPermissionsCache.next(permissions);
        return permissions;
      }),
      catchError(() => of([]))
    );
  }

  hasPermission(permissionName: string): boolean {
    const user = this.authService.getCurrentUser();
    if (!user) return false;

    // SuperAdmin has all permissions (backend is source of truth)
    if (user.role === 'SuperAdmin' || user.roleName === 'SuperAdmin') {
      return true;
    }

    // Check against cached user permissions (Admin and other roles with granted permissions)
    const userPerms = this.userPermissionsCache.value;
    if (userPerms.includes('*')) return true; // SuperAdmin returns "*"
    return userPerms.includes(permissionName);
  }

  hasAnyPermission(permissionNames: string[]): boolean {
    return permissionNames.some(permission => this.hasPermission(permission));
  }

  clearCache(): void {
    this.permissionsCache.next([]);
    this.userPermissionsCache.next([]);
  }
}

