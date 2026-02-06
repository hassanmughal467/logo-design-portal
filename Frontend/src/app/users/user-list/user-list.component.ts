import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { User, UserRole } from '@shared/models/user.model';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit, OnDestroy {
  users: User[] = [];
  loading = false;
  displayCreateDialog = false;
  createUserForm: FormGroup;
  selectedUsers: User[] = [];
  roles = Object.values(UserRole);
  
  // Table settings
  globalFilter = '';
  first = 0;
  rows = 10;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private fb: FormBuilder
  ) {
    this.createUserForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      role: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    this.loading = true;
    this.apiService.get<User[]>('users')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (users) => {
          this.users = users;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading users:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load users'
          });
          this.loading = false;
        }
      });
  }

  openCreateDialog(): void {
    this.createUserForm.reset();
    this.displayCreateDialog = true;
  }

  createUser(): void {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    const formValue = this.createUserForm.value;
    this.apiService.post<User>('users', formValue)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User created successfully'
          });
          this.displayCreateDialog = false;
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error creating user:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to create user'
          });
        }
      });
  }

  canCreateUser(): boolean {
    return this.authService.hasRole('SuperAdmin');
  }

  getRoleSeverity(role: string): string {
    const severityMap: { [key: string]: string } = {
      'SuperAdmin': 'danger',
      'Admin': 'info',
      'Designer': 'secondary',
      'Client': 'success'
    };
    return severityMap[role] || 'secondary';
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
