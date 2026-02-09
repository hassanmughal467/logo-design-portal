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
  displayResetPasswordDialog = false;
  createUserForm: FormGroup;
  resetPasswordForm: FormGroup;
  selectedUserForReset: User | null = null;
  resettingPassword = false;
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

    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    } else {
      if (confirmPassword?.hasError('passwordMismatch')) {
        confirmPassword.setErrors(null);
      }
      return null;
    }
  }

  ngOnInit(): void {
    console.log('UserListComponent ngOnInit called');
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    console.log('loadUsers() called');
    this.loading = true;
    const user = this.authService.getCurrentUser();
    console.log('Current user:', user);
    console.log('Loading users from endpoint: users');
    
    this.apiService.get<User[]>('users')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (users) => {
          console.log('Raw users from API:', users);
          if (!users || !Array.isArray(users)) {
            console.warn('Users is not an array:', users);
            this.users = [];
            this.loading = false;
            return;
          }
          this.users = users;
          console.log('Final users array:', this.users);
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading users:', error);
          console.error('Error details:', {
            status: error.status,
            statusText: error.statusText,
            message: error.message,
            error: error.error
          });
          
          let errorMessage = 'Failed to load users';
          if (error.status === 403) {
            errorMessage = 'You do not have permission to view users';
          } else if (error.status === 401) {
            errorMessage = 'Please log in to view users';
          } else if (error.error?.error) {
            errorMessage = error.error.error;
          }
          
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMessage
          });
          this.loading = false;
          this.users = [];
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

  canResetPassword(): boolean {
    return this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
  }

  openResetPasswordDialog(user: User): void {
    this.selectedUserForReset = user;
    this.resetPasswordForm.reset();
    this.displayResetPasswordDialog = true;
  }

  resetUserPassword(): void {
    if (this.resetPasswordForm.invalid || !this.selectedUserForReset) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    this.resettingPassword = true;
    const newPassword = this.resetPasswordForm.get('newPassword')?.value;

    this.authService.resetUserPassword(this.selectedUserForReset.id, newPassword).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Password has been reset for ${this.selectedUserForReset?.email}`
        });
        this.displayResetPasswordDialog = false;
        this.selectedUserForReset = null;
        this.resettingPassword = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to reset password'
        });
        this.resettingPassword = false;
      }
    });
  }
}
