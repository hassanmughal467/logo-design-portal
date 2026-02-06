import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { Permission } from '@shared/models/permission.model';

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private permissionsCache = new BehaviorSubject<Permission[]>([]);
  public permissions$ = this.permissionsCache.asObservable();

  constructor(
    private apiService: ApiService,
    private authService: AuthService
  ) {
    // Load permissions when service initializes (if user is authenticated)
    if (this.authService.isAuthenticated()) {
      this.loadPermissions().subscribe();
    }
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

  hasPermission(permissionName: string): boolean {
    const user = this.authService.getCurrentUser();
    if (!user) return false;

    // For now, return true if user is SuperAdmin (backend will validate)
    // In a real implementation, you'd check against cached permissions
    // This is just for UI visibility - backend is source of truth
    if (user.role === 'SuperAdmin') {
      return true;
    }

    // TODO: Implement proper permission checking against cached permissions
    // This requires backend to return user's permissions in JWT or via separate endpoint
    return false;
  }

  hasAnyPermission(permissionNames: string[]): boolean {
    return permissionNames.some(permission => this.hasPermission(permission));
  }

  clearCache(): void {
    this.permissionsCache.next([]);
  }
}
