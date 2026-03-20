import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface Permission {
  id: string;
  name: string;
  description: string;
  resource: string;
  action: string;
}

export interface RolePermission {
  roleId: string;
  roleName: string;
  permissions: Permission[];
}

export interface Role {
  id: string;
  name: string;
  description: string;
}

export interface PermissionMatrixCell {
  permissionId: string;
  roleId: string;
  hasPermission: boolean;
  loading: boolean;
}

@Component({
  selector: 'app-permission-list',
  templateUrl: './permission-list.component.html',
  styleUrls: ['./permission-list.component.scss']
})
export class PermissionListComponent implements OnInit, OnDestroy {
  permissions: Permission[] = [];
  roles: Role[] = [];
  rolePermissions: Map<string, Set<string>> = new Map(); // roleId -> Set of permissionIds
  checkboxStates: Map<string, boolean> = new Map(); // "roleId-permissionId" -> boolean
  loading = false;
  loadingMatrix = false;
  globalFilter = '';
  first = 0;
  rows = 10;
  displayMatrix = false;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadPermissions();
  }

  loadRoles(): void {
    this.apiService.get<Role[]>('permissions/roles')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (roles) => {
          this.roles = roles || [];
          if (this.displayMatrix && this.permissions.length > 0) {
            this.loadRolePermissions();
          }
        },
        error: () => {
          this.roles = [];
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPermissions(): void {
    this.loading = true;
    this.apiService.get<Permission[]>('permissions')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (permissions) => {
          this.permissions = permissions || [];
          this.loading = false;
          if (this.permissions.length === 0) {
            this.messageService.add({
              severity: 'info',
              summary: 'No Permissions',
              detail: 'No permissions found in the system.',
              life: 3000
            });
          } else if (this.displayMatrix && this.roles.length > 0) {
            this.loadRolePermissions();
          }
        },
        error: (error) => {
          console.error('Error loading permissions:', error);
          this.permissions = [];
          this.loading = false;
          
          // Show error message (403 errors are handled silently by interceptor, but we can still show info)
          if (error?.status === 403) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Access Denied',
              detail: 'You do not have permission to view permissions. SuperAdmin access required.',
              life: 5000
            });
          } else if (error?.status !== 0) {
            // Don't show error for network errors (status 0) as interceptor handles it
            const errorMessage = error?.error?.error || error?.error?.message || 'Failed to load permissions. Please try again.';
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: errorMessage,
              life: 5000
            });
          }
        }
      });
  }

  loadRolePermissions(): void {
    if (this.permissions.length === 0 || this.roles.length === 0) return;
    
    this.loadingMatrix = true;
    this.rolePermissions.clear();
    
    // Initialize all roles with empty permission sets
    this.roles.forEach(role => {
      this.rolePermissions.set(role.id, new Set<string>());
    });

    // SuperAdmin has all permissions automatically
    const superAdminRole = this.roles.find(r => r.name === 'SuperAdmin');
    const superAdminId = superAdminRole?.id;

    if (superAdminId) {
      this.permissions.forEach(permission => {
        this.rolePermissions.get(superAdminId)?.add(permission.id);
        const key = `${superAdminId}-${permission.id}`;
        this.checkboxStates.set(key, true);
      });
    }

    // Load permissions for other roles
    const rolesToLoad = this.roles.filter(r => r.id !== superAdminId);
    let loadedCount = 0;
    const totalRoles = rolesToLoad.length;

    if (totalRoles === 0) {
      this.loadingMatrix = false;
      return;
    }

    rolesToLoad.forEach(role => {
      this.apiService.get<RolePermission>(`permissions/role/${role.id}`)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: (rolePermission) => {
            const permissionSet = new Set<string>();
            rolePermission.permissions.forEach(p => {
              permissionSet.add(p.id);
              // Initialize checkbox states
              const key = `${role.id}-${p.id}`;
              this.checkboxStates.set(key, true);
            });
            this.rolePermissions.set(role.id, permissionSet);
            loadedCount++;
            if (loadedCount === totalRoles) {
              this.loadingMatrix = false;
            }
          },
          error: (error) => {
            // 404 means role has no permissions, which is fine
            if (error?.status !== 404) {
              console.error(`Error loading permissions for role ${role.name}:`, error);
            }
            loadedCount++;
            if (loadedCount === totalRoles) {
              this.loadingMatrix = false;
            }
          }
        });
    });
  }

  toggleMatrixView(): void {
    this.displayMatrix = !this.displayMatrix;
    
    if (this.displayMatrix) {
      // If permissions are loaded but role permissions aren't, load them
      if (this.permissions.length > 0 && this.rolePermissions.size === 0) {
        this.loadRolePermissions();
      } else if (this.permissions.length === 0) {
        // If permissions aren't loaded yet, load them first
        this.loadPermissions();
      }
    }
  }

  hasPermission(roleId: string, permissionId: string): boolean {
    const permissionSet = this.rolePermissions.get(roleId);
    return permissionSet ? permissionSet.has(permissionId) : false;
  }

  getCheckboxState(roleId: string, permissionId: string): boolean {
    const key = `${roleId}-${permissionId}`;
    if (this.checkboxStates.has(key)) {
      return this.checkboxStates.get(key)!;
    }
    const state = this.hasPermission(roleId, permissionId);
    this.checkboxStates.set(key, state);
    return state;
  }

  setCheckboxState(roleId: string, permissionId: string, value: boolean): void {
    const key = `${roleId}-${permissionId}`;
    this.checkboxStates.set(key, value);
  }

  isSuperAdmin(roleId: string): boolean {
    const role = this.roles.find(r => r.id === roleId);
    return role?.name === 'SuperAdmin';
  }

  togglePermission(roleId: string, permissionId: string, value: boolean): void {
    if (this.isSuperAdmin(roleId)) {
      this.messageService.add({
        severity: 'info',
        summary: 'SuperAdmin',
        detail: 'SuperAdmin automatically has all permissions and cannot be modified.',
        life: 3000
      });
      // Revert checkbox state
      this.setCheckboxState(roleId, permissionId, true);
      return;
    }

    const permissionSet = this.rolePermissions.get(roleId);
    
    // Optimistically update UI
    if (value) {
      // Assign permission
      permissionSet?.add(permissionId);
      this.setCheckboxState(roleId, permissionId, true);
      this.assignPermission(roleId, permissionId);
    } else {
      // Revoke permission
      permissionSet?.delete(permissionId);
      this.setCheckboxState(roleId, permissionId, false);
      this.revokePermission(roleId, permissionId);
    }
  }

  assignPermission(roleId: string, permissionId: string): void {
    this.apiService.post('permissions/assign', {
      roleId: roleId,
      permissionId: permissionId
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Permission assigned successfully.',
          life: 3000
        });
      },
      error: (error) => {
        const errorMessage = error?.error?.error || error?.error?.message || 'Failed to assign permission.';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMessage,
          life: 5000
        });
        // Revert the change
        const permissionSet = this.rolePermissions.get(roleId);
        permissionSet?.delete(permissionId);
        this.setCheckboxState(roleId, permissionId, false);
      }
    });
  }

  revokePermission(roleId: string, permissionId: string): void {
    this.apiService.delete('permissions/revoke', {
      roleId: roleId,
      permissionId: permissionId
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Permission revoked successfully.',
          life: 3000
        });
      },
      error: (error) => {
        const errorMessage = error?.error?.error || error?.error?.message || 'Failed to revoke permission.';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMessage,
          life: 5000
        });
        // Revert the change
        const permissionSet = this.rolePermissions.get(roleId);
        permissionSet?.add(permissionId);
        this.setCheckboxState(roleId, permissionId, true);
      }
    });
  }
}
