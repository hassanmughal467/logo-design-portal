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

@Component({
  selector: 'app-permission-list',
  templateUrl: './permission-list.component.html',
  styleUrls: ['./permission-list.component.scss']
})
export class PermissionListComponent implements OnInit, OnDestroy {
  permissions: Permission[] = [];
  loading = false;
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
    this.loadPermissions();
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
          this.permissions = permissions;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading permissions:', error);
          this.permissions = [];
          this.loading = false;
        }
      });
  }

  toggleMatrixView(): void {
    this.displayMatrix = !this.displayMatrix;
  }
}
