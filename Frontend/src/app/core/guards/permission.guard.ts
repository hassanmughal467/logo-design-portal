import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { PermissionsService } from '../services/permissions.service';
import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class PermissionGuard implements CanActivate {
  constructor(
    private permissionsService: PermissionsService,
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): boolean {
    // Get required permissions from route data
    const requiredPermissions = route.data['permissions'] as string[];

    if (!requiredPermissions || requiredPermissions.length === 0) {
      return true; // No permission requirement
    }

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
      return false;
    }

    // Check if user has any of the required permissions
    const hasPermission = this.permissionsService.hasAnyPermission(requiredPermissions);

    if (!hasPermission) {
      // User doesn't have required permission, redirect to dashboard
      this.router.navigate(['/dashboard']);
      return false;
    }

    return true;
  }
}
