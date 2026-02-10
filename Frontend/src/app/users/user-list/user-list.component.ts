import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Dropdown } from 'primeng/dropdown';
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
  displayViewDialog = false;
  displayEditDialog = false;
  displayDeleteDialog = false;
  createUserForm: FormGroup;
  editUserForm: FormGroup;
  resetPasswordForm: FormGroup;
  selectedUserForReset: User | null = null;
  selectedUserForView: User | null = null;
  selectedUserForEdit: User | null = null;
  selectedUserForDelete: User | null = null;
  resettingPassword = false;
  updatingUser = false;
  deletingUser = false;
  loadingUserDetails = false;
  selectedUsers: User[] = [];
  roles = Object.values(UserRole);
  
  // Role ID mapping
  private roleIdMap: { [key: string]: string } = {
    'SuperAdmin': '11111111-1111-1111-1111-111111111111',
    'Admin': '22222222-2222-2222-2222-222222222222',
    'Designer': '33333333-3333-3333-3333-333333333333',
    'Client': '44444444-4444-4444-4444-444444444444'
  };
  
  @ViewChild('editRoleDropdown') editRoleDropdown?: Dropdown;
  
  // Table settings
  globalFilter = '';
  first = 0;
  rows = 10;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private messageService: MessageService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
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

    this.editUserForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      role: ['', Validators.required],
      isActive: [true]
    });
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
    // Reset form and clear all values
    this.createUserForm.reset({
      email: '',
      password: '',
      firstName: '',
      lastName: '',
      role: ''
    });
    // Mark all fields as untouched to clear validation states
    Object.keys(this.createUserForm.controls).forEach(key => {
      const control = this.createUserForm.get(key);
      if (control) {
        control.setValue('', { emitEvent: false });
        control.markAsUntouched();
        control.markAsPristine();
        control.updateValueAndValidity({ emitEvent: false });
      }
    });
    // Force change detection
    this.cdr.detectChanges();
    this.displayCreateDialog = true;
    
    // Additional reset after dialog is visible to prevent browser autofill
    setTimeout(() => {
      this.createUserForm.patchValue({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        role: ''
      }, { emitEvent: false });
      // Clear email field explicitly to prevent autofill
      const emailControl = this.createUserForm.get('email');
      if (emailControl) {
        emailControl.setValue('', { emitEvent: false });
      }
      this.cdr.detectChanges();
    }, 100);
    
    // One more reset after a longer delay to catch any late autofill
    setTimeout(() => {
      const emailControl = this.createUserForm.get('email');
      if (emailControl && emailControl.value) {
        emailControl.setValue('', { emitEvent: false });
        this.cdr.detectChanges();
      }
    }, 300);
  }

  createUser(): void {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    const formValue = this.createUserForm.value;
    
    // Convert role name to RoleId (Guid)
    const roleName = formValue.role;
    const roleId = this.roleIdMap[roleName];
    
    if (!roleId) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Invalid role selected'
      });
      return;
    }

    // Prepare request with RoleId instead of role
    const createRequest = {
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      password: formValue.password,
      roleId: roleId
    };

    this.apiService.post<User>('users', createRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User created successfully'
          });
          this.displayCreateDialog = false;
          this.createUserForm.reset();
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

  onPageChange(event: any): void {
    this.first = event.first;
    this.rows = event.rows;
  }

  viewUser(user: User): void {
    this.selectedUserForView = user;
    this.displayViewDialog = true;
  }

  editUser(user: User): void {
    this.selectedUserForEdit = user;
    this.loadingUserDetails = true;
    
    // Fetch full user details from API
    this.apiService.get<User>(`users/${user.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (fullUser) => {
          // Get role value from API
          const apiRole = fullUser.roleName || fullUser.role;
          const roleValue = apiRole ? String(apiRole).trim() : '';
          
          // Find exact match in roles array
          let selectedRole: string | null = null;
          
          if (roleValue) {
            // Try exact match first
            selectedRole = this.roles.find(r => String(r) === roleValue) || null;
            
            // If no match, try case-insensitive
            if (!selectedRole) {
              selectedRole = this.roles.find(r => 
                String(r).toLowerCase() === roleValue.toLowerCase()
              ) || null;
            }
          }
          
          this.loadingUserDetails = false;
          
          // Set all form values at once
          this.editUserForm.patchValue({
            email: fullUser.email || '',
            firstName: fullUser.firstName || '',
            lastName: fullUser.lastName || '',
            role: selectedRole || null,
            isActive: fullUser.isActive !== undefined ? fullUser.isActive : true
          });
          
          // Open dialog
          this.displayEditDialog = true;
          
          // Ensure dropdown value is set after dialog is visible
          setTimeout(() => {
            const roleControl = this.editUserForm.get('role');
            if (roleControl && selectedRole && roleControl.value !== selectedRole) {
              roleControl.setValue(selectedRole, { emitEvent: false });
              this.cdr.detectChanges();
            }
          }, 100);
        },
        error: (error) => {
          console.error('Error loading user details:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to load user details'
          });
          this.loadingUserDetails = false;
        }
      });
  }

  onEditDialogShow(): void {
    // Ensure dropdown value is set after dialog is fully rendered
    setTimeout(() => {
      const roleControl = this.editUserForm.get('role');
      if (!roleControl || !this.selectedUserForEdit) return;
      
      const roleValue = String(this.selectedUserForEdit.roleName || this.selectedUserForEdit.role || '').trim();
      if (!roleValue) return;
      
      // Find matching role
      const matchingRole: string | undefined = this.roles.find(r => String(r) === roleValue) || 
                          this.roles.find(r => String(r).toLowerCase() === roleValue.toLowerCase());
      
      if (matchingRole && roleControl.value !== matchingRole) {
        roleControl.setValue(matchingRole, { emitEvent: false });
        this.cdr.detectChanges();
      }
    }, 150);
  }

  updateUser(): void {
    if (this.editUserForm.invalid || !this.selectedUserForEdit) {
      this.editUserForm.markAllAsTouched();
      return;
    }

    this.updatingUser = true;
    const formValue = this.editUserForm.value;
    
    // Note: Update endpoint may not exist yet, so we'll try PUT first
    this.apiService.put<User>(`users/${this.selectedUserForEdit.id}`, formValue)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedUser) => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'User updated successfully'
          });
          this.displayEditDialog = false;
          this.selectedUserForEdit = null;
          this.updatingUser = false;
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error updating user:', error);
          if (error.status === 404 || error.status === 405) {
            this.messageService.add({
              severity: 'warn',
              summary: 'Not Available',
              detail: 'User update functionality is not yet implemented on the server'
            });
          } else {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: error.error?.error || 'Failed to update user'
            });
          }
          this.updatingUser = false;
        }
      });
  }

  canResetPassword(): boolean {
    return this.authService.hasAnyRole(['SuperAdmin', 'Admin']);
  }

  canDeleteUser(): boolean {
    return this.authService.hasRole('SuperAdmin');
  }

  openDeleteDialog(user: User): void {
    this.selectedUserForDelete = user;
    this.displayDeleteDialog = true;
  }

  deleteUser(): void {
    if (!this.selectedUserForDelete) {
      return;
    }

    this.deletingUser = true;
    this.apiService.delete(`users/${this.selectedUserForDelete.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `User ${this.selectedUserForDelete?.email} has been deleted successfully`
          });
          this.displayDeleteDialog = false;
          this.selectedUserForDelete = null;
          this.deletingUser = false;
          this.loadUsers();
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to delete user'
          });
          this.deletingUser = false;
        }
      });
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
