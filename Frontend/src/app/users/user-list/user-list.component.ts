import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Dropdown } from 'primeng/dropdown';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { AuthService } from '@core/services/auth.service';
import { DashboardService } from '@core/services/dashboard.service';
import { PermissionsService } from '@core/services/permissions.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { User } from '@shared/models/user.model';
import { RoleOption } from '@shared/models/role.model';
import { DEFAULT_CLIENT_CURRENCY, SUPPORTED_CURRENCY_OPTIONS } from '@core/constants/currency-options';
import { TagSeverity } from '@shared/types/primeng.types';

@Component({
  selector: 'app-user-list',
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss']
})
export class UserListComponent implements OnInit, OnDestroy {
  users: User[] = [];
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
  deletePermanent = false;
  loadingUserDetails = false;
  selectedUsers: User[] = [];
  /** Roles from GET /api/roles (standard roles are seeded on the API with fixed IDs). */
  availableRoles: RoleOption[] = [];

  billingTypeOptions = [
    { label: 'Per Logo', value: 1 },
    { label: 'Weekly', value: 2 },
    { label: 'Monthly', value: 3 }
  ];

  customerTypeOptions = [
    { label: 'Residential', value: 'Residential' },
    { label: 'Business', value: 'Business' },
    { label: 'Student', value: 'Student' }
  ];

  readonly currencyOptions = SUPPORTED_CURRENCY_OPTIONS;
  
  @ViewChild('editRoleDropdown') editRoleDropdown?: Dropdown;
  
  // Table settings
  globalFilter = '';
  first = 0;
  rows = 10;
  totalUsers = 0;
  currentPage = 1;
  pageSize = 10;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private authService: AuthService,
    private permissionsService: PermissionsService,
    private messageService: MessageService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private dashboardService: DashboardService
  ) {
    this.createUserForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(6)]],
      roleId: ['', Validators.required],
      // Additional user fields
      secondaryEmail: ['', [Validators.email]],
      invoiceEmail: ['', [Validators.email]],
      // Client profile fields
      companyName: [''],
      billingType: [1], // 1=PerLogo, 2=Weekly, 3=Monthly
      currencyCode: [DEFAULT_CLIENT_CURRENCY],
      contactName: [''],
      phoneNumber: [''],
      cell: [''],
      fax: [''],
      address: [''],
      city: [''],
      state: [''],
      country: [''],
      postalCode: [''],
      website: [''],
      reference: [''],
      customerType: [null],
      // Designer profile fields
      specialization: [''],
      bio: [''],
      hourlyRate: [null],
      isAvailable: [true]
    });

    // Update validators when role changes
    this.createUserForm.get('roleId')?.valueChanges.subscribe(() => {
      const roleName = this.getRoleNameById(this.createUserForm.get('roleId')?.value);
      if (roleName === 'Client') {
        this.createUserForm.get('invoiceEmail')?.setValidators([Validators.required, Validators.email]);
        this.createUserForm.get('companyName')?.setValidators([Validators.required]);
        this.createUserForm.get('contactName')?.setValidators([Validators.required]);
        this.createUserForm.get('phoneNumber')?.setValidators([Validators.required]);
        this.createUserForm.get('currencyCode')?.setValidators([Validators.required]);
      } else {
        this.createUserForm.get('invoiceEmail')?.setValidators([Validators.email]);
        this.createUserForm.get('companyName')?.clearValidators();
        this.createUserForm.get('contactName')?.clearValidators();
        this.createUserForm.get('phoneNumber')?.clearValidators();
        this.createUserForm.get('currencyCode')?.clearValidators();
      }
      this.createUserForm.get('invoiceEmail')?.updateValueAndValidity({ emitEvent: false });
      this.createUserForm.get('companyName')?.updateValueAndValidity({ emitEvent: false });
      this.createUserForm.get('contactName')?.updateValueAndValidity({ emitEvent: false });
      this.createUserForm.get('phoneNumber')?.updateValueAndValidity({ emitEvent: false });
      this.createUserForm.get('currencyCode')?.updateValueAndValidity({ emitEvent: false });
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
      isActive: [true],
      // Additional user fields
      secondaryEmail: ['', [Validators.email]],
      invoiceEmail: ['', [Validators.email]],
      // Client profile fields
      companyName: [''],
      billingType: [1], // 1=PerLogo, 2=Weekly, 3=Monthly
      currencyCode: [DEFAULT_CLIENT_CURRENCY],
      contactName: [''],
      phoneNumber: [''],
      cell: [''],
      fax: [''],
      address: [''],
      city: [''],
      state: [''],
      country: [''],
      postalCode: [''],
      website: [''],
      reference: [''],
      customerType: [null],
      // Designer profile fields
      specialization: [''],
      bio: [''],
      hourlyRate: [null],
      isAvailable: [true]
    });

    // Update validators when role changes in edit form (only companyName required for Client when editing)
    this.editUserForm.get('role')?.valueChanges.subscribe(role => {
      if (role === 'Client') {
        this.editUserForm.get('companyName')?.setValidators([Validators.required]);
        this.editUserForm.get('invoiceEmail')?.setValidators([Validators.email]);
        this.editUserForm.get('contactName')?.clearValidators();
        this.editUserForm.get('phoneNumber')?.clearValidators();
      } else {
        this.editUserForm.get('companyName')?.clearValidators();
        this.editUserForm.get('invoiceEmail')?.setValidators([Validators.email]);
        this.editUserForm.get('contactName')?.clearValidators();
        this.editUserForm.get('phoneNumber')?.clearValidators();
      }
      this.editUserForm.get('companyName')?.updateValueAndValidity({ emitEvent: false });
      this.editUserForm.get('invoiceEmail')?.updateValueAndValidity({ emitEvent: false });
      this.editUserForm.get('contactName')?.updateValueAndValidity({ emitEvent: false });
      this.editUserForm.get('phoneNumber')?.updateValueAndValidity({ emitEvent: false });
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
    this.loadAvailableRoles();
    this.loadUsers();
  }

  loadAvailableRoles(): void {
    this.apiService.get<any[]>('roles')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (rows) => {
          this.availableRoles = (rows || []).map((r: any) => ({
            id: String(r.id ?? r.Id),
            name: String(r.name ?? r.Name ?? ''),
            description: r.description ?? r.Description ?? undefined
          })).filter(r => r.name);
        },
        error: () => {
          this.availableRoles = [];
        }
      });
  }

  getRoleNameById(roleId: string | null | undefined): string | null {
    if (!roleId) return null;
    return this.availableRoles.find(r => r.id === roleId)?.name ?? null;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUsers(): void {
    const user = this.authService.getCurrentUser();
    
    this.apiService.get<any>(`users?page=${this.currentPage}&pageSize=${this.rows}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.users = ApiService.extractItems<User>(response);
          const meta = ApiService.extractPagedMeta(response);
          this.totalUsers = meta.total;
        },
        error: (error) => {
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
          this.users = [];
        }
      });
  }

  openCreateDialog(): void {
    // Reset form and clear all values
    this.createUserForm.reset({
      email: '',
      firstName: '',
      lastName: '',
      password: '',
      roleId: '',
      secondaryEmail: '',
      invoiceEmail: '',
      companyName: '',
      billingType: 1,
      currencyCode: DEFAULT_CLIENT_CURRENCY,
      contactName: '',
      phoneNumber: '',
      cell: '',
      fax: '',
      address: '',
      city: '',
      state: '',
      country: '',
      postalCode: '',
      website: '',
      reference: '',
      specialization: '',
      bio: '',
      hourlyRate: null,
      isAvailable: true
    });
    // Mark all fields as untouched to clear validation states
    Object.keys(this.createUserForm.controls).forEach(key => {
      const control = this.createUserForm.get(key);
      if (control) {
        const defaultValue =
          key === 'isAvailable' ? true
          : key === 'hourlyRate' ? null
          : key === 'billingType' ? 1
          : key === 'currencyCode' ? DEFAULT_CLIENT_CURRENCY
          : '';
        control.setValue(defaultValue, { emitEvent: false });
        control.markAsUntouched();
        control.markAsPristine();
        control.updateValueAndValidity({ emitEvent: false });
      }
    });
    // Force change detection
    this.cdr.detectChanges();
    this.displayCreateDialog = true;
  }

  isClientRole(): boolean {
    return this.getRoleNameById(this.createUserForm.get('roleId')?.value) === 'Client';
  }

  isDesignerRole(): boolean {
    return this.getRoleNameById(this.createUserForm.get('roleId')?.value) === 'Designer';
  }

  getDesignerHourlyRate(): number | null {
    if (!this.selectedUserForView) return null;
    const profile = (this.selectedUserForView as any).designerProfile;
    return profile?.hourlyRate || null;
  }

  getDesignerIsAvailable(): boolean {
    if (!this.selectedUserForView) return false;
    const profile = (this.selectedUserForView as any).designerProfile;
    return profile?.isAvailable ?? false;
  }

  isEditClientRole(): boolean {
    const role = this.editUserForm.get('role')?.value;
    return role === 'Client';
  }

  /** Apply role-based validators to edit form (ensures Update button enables correctly after patch) */
  private applyEditRoleValidators(role: string): void {
    if (role === 'Client') {
      this.editUserForm.get('companyName')?.setValidators([Validators.required]);
      this.editUserForm.get('invoiceEmail')?.setValidators([Validators.email]);
    } else {
      this.editUserForm.get('companyName')?.clearValidators();
      this.editUserForm.get('invoiceEmail')?.setValidators([Validators.email]);
    }
    this.editUserForm.get('companyName')?.updateValueAndValidity({ emitEvent: false });
    this.editUserForm.get('invoiceEmail')?.updateValueAndValidity({ emitEvent: false });
  }

  isEditDesignerRole(): boolean {
    const role = this.editUserForm.get('role')?.value;
    return role === 'Designer';
  }

  getSecondaryEmail(): string | null {
    if (!this.selectedUserForView) return null;
    return (this.selectedUserForView as any).secondaryEmail || null;
  }

  getInvoiceEmail(): string | null {
    if (!this.selectedUserForView) return null;
    return (this.selectedUserForView as any).invoiceEmail || null;
  }

  getBillingTypeLabel(value: number | undefined): string {
    const opt = this.billingTypeOptions.find(o => o.value === value);
    return opt?.label ?? 'Per Logo';
  }

  getClientProfile(): any {
    if (!this.selectedUserForView) return null;
    return (this.selectedUserForView as any).clientProfile || null;
  }

  getDesignerProfile(): any {
    if (!this.selectedUserForView) return null;
    return (this.selectedUserForView as any).designerProfile || null;
  }

  getCurrentUserRole(): string {
    if (!this.selectedUserForView) return '';
    return (this.selectedUserForView.roleName || (this.selectedUserForView as any).role || '').trim();
  }

  shouldShowClientSection(): boolean {
    return this.getCurrentUserRole() === 'Client' && this.getClientProfile() !== null;
  }

  shouldShowDesignerSection(): boolean {
    return this.getCurrentUserRole() === 'Designer' && this.getDesignerProfile() !== null;
  }

  createUser(): void {
    if (this.createUserForm.invalid) {
      this.createUserForm.markAllAsTouched();
      return;
    }

    const formValue = this.createUserForm.value;
    const roleId = formValue.roleId;
    const roleName = this.getRoleNameById(roleId);
    if (!roleId || !roleName) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Select a role. If the list is empty, ensure the API has seeded roles and try refreshing.'
      });
      return;
    }

    const createRequest: any = {
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      password: formValue.password,
      roleId: roleId
    };

    // Add optional user fields
    if (formValue.secondaryEmail) {
      createRequest.secondaryEmail = formValue.secondaryEmail;
    }
    if (formValue.invoiceEmail) {
      createRequest.invoiceEmail = formValue.invoiceEmail;
    }

    // Add profile fields based on role
    if (roleName === 'Client') {
      // Mandatory fields for Client
      createRequest.companyName = formValue.companyName || '';
      createRequest.billingType = formValue.billingType ?? 1;
      createRequest.currencyCode = formValue.currencyCode || DEFAULT_CLIENT_CURRENCY;
      createRequest.contactName = formValue.contactName || '';
      createRequest.phoneNumber = formValue.phoneNumber || '';
      // Optional fields
      if (formValue.cell) createRequest.cell = formValue.cell;
      if (formValue.fax) createRequest.fax = formValue.fax;
      if (formValue.address) createRequest.address = formValue.address;
      if (formValue.city) createRequest.city = formValue.city;
      if (formValue.state) createRequest.state = formValue.state;
      if (formValue.country) createRequest.country = formValue.country;
      if (formValue.postalCode) createRequest.postalCode = formValue.postalCode;
      if (formValue.website) createRequest.website = formValue.website;
      if (formValue.reference) createRequest.reference = formValue.reference;
      if (formValue.customerType) createRequest.customerType = formValue.customerType;
    } else if (roleName === 'Designer') {
      // Optional fields for Designer
      if (formValue.specialization) createRequest.specialization = formValue.specialization;
      if (formValue.bio) createRequest.bio = formValue.bio;
      if (formValue.hourlyRate) createRequest.hourlyRate = parseFloat(formValue.hourlyRate);
      if (formValue.isAvailable !== undefined) createRequest.isAvailable = formValue.isAvailable;
    }

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
          this.dashboardService.invalidateDashboardCache();
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
    if (this.authService.hasRole('SuperAdmin')) {
      return true;
    }
    return this.authService.hasRole('Admin') && this.permissionsService.hasPermission('CreateUser');
  }

  /** Dropdown options for create-user (value = role id). */
  get rolesForCreateDropdown(): { label: string; value: string }[] {
    const rows = this.availableRoles.filter(r => {
      if (this.authService.hasRole('SuperAdmin')) return true;
      if (this.authService.hasRole('Admin') && this.permissionsService.hasPermission('CreateUser')) {
        return r.name.toLowerCase() !== 'superadmin';
      }
      return false;
    });
    return rows.map(r => ({ label: r.name, value: r.id })).sort((a, b) => a.label.localeCompare(b.label));
  }

  /** Role names allowed when editing (Admin cannot assign SuperAdmin). */
  get rolesAssignableOnEdit(): string[] {
    const names = this.availableRoles.map(r => r.name);
    if (this.authService.hasRole('SuperAdmin')) {
      return [...names].sort((a, b) => a.localeCompare(b));
    }
    return names.filter(n => n.toLowerCase() !== 'superadmin').sort((a, b) => a.localeCompare(b));
  }

  getRoleSeverity(role: string): TagSeverity {
    const severityMap: Record<string, TagSeverity> = {
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
    this.currentPage = Math.floor(event.first / event.rows) + 1;
    this.loadUsers();
  }

  viewUser(user: User): void {
    this.selectedUserForView = user;
    this.loadingUserDetails = true;
    
    // Fetch full user details from API to get profile information
    this.apiService.get<User>(`users/${user.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (fullUser) => {
          this.selectedUserForView = fullUser;
          this.loadingUserDetails = false;
          this.displayViewDialog = true;
        },
        error: (error) => {
          console.error('Error loading user details:', error);
          // Still show the dialog with basic info if API fails
          this.loadingUserDetails = false;
          this.displayViewDialog = true;
          this.messageService.add({
            severity: 'warn',
            summary: 'Warning',
            detail: 'Could not load full user details. Showing basic information only.'
          });
        }
      });
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
          
          const pool = this.availableRoles.map(r => r.name);
          if (roleValue) {
            selectedRole = pool.find(r => String(r) === roleValue) || null;
            if (!selectedRole) {
              selectedRole = pool.find(r => String(r).toLowerCase() === roleValue.toLowerCase()) || null;
            }
            if (!selectedRole) {
              selectedRole = roleValue;
            }
          }
          
          this.loadingUserDetails = false;
          
          // Set all form values including profile information
          this.editUserForm.patchValue({
            email: fullUser.email || '',
            firstName: fullUser.firstName || '',
            lastName: fullUser.lastName || '',
            role: selectedRole || null,
            isActive: fullUser.isActive !== undefined ? fullUser.isActive : true,
            secondaryEmail: (fullUser as any).secondaryEmail || '',
            invoiceEmail: (fullUser as any).invoiceEmail || '',
            // Client profile fields
            companyName: (fullUser as any).clientProfile?.companyName || '',
            billingType: (fullUser as any).clientProfile?.billingType ?? 1,
            currencyCode: (fullUser as any).clientProfile?.currencyCode || DEFAULT_CLIENT_CURRENCY,
            contactName: (fullUser as any).clientProfile?.contactName || '',
            phoneNumber: (fullUser as any).clientProfile?.phoneNumber || '',
            cell: (fullUser as any).clientProfile?.cell || '',
            fax: (fullUser as any).clientProfile?.fax || '',
            address: (fullUser as any).clientProfile?.address || '',
            city: (fullUser as any).clientProfile?.city || '',
            state: (fullUser as any).clientProfile?.state || '',
            country: (fullUser as any).clientProfile?.country || '',
            postalCode: (fullUser as any).clientProfile?.postalCode || '',
            website: (fullUser as any).clientProfile?.website || '',
            reference: (fullUser as any).clientProfile?.reference || '',
            customerType: (fullUser as any).clientProfile?.customerType || null,
            // Designer profile fields
            specialization: (fullUser as any).designerProfile?.specialization || '',
            bio: (fullUser as any).designerProfile?.bio || '',
            hourlyRate: (fullUser as any).designerProfile?.hourlyRate || null,
            isAvailable: (fullUser as any).designerProfile?.isAvailable !== undefined ? (fullUser as any).designerProfile.isAvailable : true
          });

          // Apply role-based validators after patch (fixes Update button disabled when validators not yet applied)
          this.applyEditRoleValidators(selectedRole || '');
          
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
      
      const pool = this.availableRoles.map(r => r.name);
      const matchingRole: string | undefined =
        pool.find(r => String(r) === roleValue) ||
        pool.find(r => String(r).toLowerCase() === roleValue.toLowerCase());
      
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
    const formValue = { ...this.editUserForm.value };
    // Root Admin: ensure role and isActive cannot be changed (disabled controls are excluded from form value)
    if (this.selectedUserForEdit.isRootAdmin) {
      formValue.role = 'SuperAdmin';
      formValue.isActive = true;
    }
    
    // Prepare update request with all fields
    const updateRequest: any = {
      email: formValue.email,
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      role: formValue.role,
      isActive: formValue.isActive,
      secondaryEmail: formValue.secondaryEmail || null,
      invoiceEmail: formValue.invoiceEmail || null
    };

    // Add profile fields based on role
    if (formValue.role === 'Client') {
      updateRequest.companyName = formValue.companyName || null;
      updateRequest.billingType = formValue.billingType ?? 1;
      updateRequest.currencyCode = formValue.currencyCode || DEFAULT_CLIENT_CURRENCY;
      updateRequest.contactName = formValue.contactName || null;
      updateRequest.phoneNumber = formValue.phoneNumber || null;
      updateRequest.cell = formValue.cell || null;
      updateRequest.fax = formValue.fax || null;
      updateRequest.address = formValue.address || null;
      updateRequest.city = formValue.city || null;
      updateRequest.state = formValue.state || null;
      updateRequest.country = formValue.country || null;
      updateRequest.postalCode = formValue.postalCode || null;
      updateRequest.website = formValue.website || null;
      updateRequest.reference = formValue.reference || null;
      updateRequest.customerType = formValue.customerType || null;
    } else if (formValue.role === 'Designer') {
      updateRequest.specialization = formValue.specialization || null;
      updateRequest.bio = formValue.bio || null;
      updateRequest.hourlyRate = formValue.hourlyRate || null;
      updateRequest.isAvailable = formValue.isAvailable !== undefined ? formValue.isAvailable : true;
    }
    
    // Note: Update endpoint may not exist yet, so we'll try PUT first
    this.apiService.put<User>(`users/${this.selectedUserForEdit.id}`, updateRequest)
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
          this.dashboardService.invalidateDashboardCache();
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

  /** Whether the delete button should be shown for this user (not Root Admin, not self, and active) */
  canDeleteUserFor(user: User): boolean {
    if (!this.canDeleteUser() || !user.isActive) return false;
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || user.id === currentUser.id) return false;
    if (user.isRootAdmin) return false;
    return true;
  }

  /** Whether the user is an admin (SuperAdmin or Admin) - used for confirmation message */
  isAdminUser(user: User): boolean {
    const role = user.roleName || user.role || '';
    return role === 'SuperAdmin' || role === 'Admin';
  }

  /** Whether the reactivate button should be shown for this user (not Root Admin, not self, and inactive) */
  canReactivateUserFor(user: User): boolean {
    if (!this.canDeleteUser() || user.isActive) return false;
    const currentUser = this.authService.getCurrentUser();
    if (!currentUser || user.id === currentUser.id) return false;
    if (user.isRootAdmin) return false;
    return true;
  }

  openDeleteDialog(user: User): void {
    const currentUser = this.authService.getCurrentUser();
    if (currentUser && user.id === currentUser.id) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Not Allowed',
        detail: 'You cannot delete your own account.'
      });
      return;
    }
    if (user.isRootAdmin) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Not Allowed',
        detail: 'Super Admin cannot be deleted.'
      });
      return;
    }
    this.selectedUserForDelete = user;
    this.displayDeleteDialog = true;
  }

  deleteUser(permanent: boolean): void {
    if (!this.selectedUserForDelete) {
      return;
    }

    this.deletingUser = true;
    this.deletePermanent = permanent;
    const deletedUserId = this.selectedUserForDelete.id;
    const endpoint = permanent
      ? `users/${deletedUserId}?permanent=true`
      : `users/${deletedUserId}`;

    this.apiService.delete(endpoint)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          const msg = permanent
            ? `User ${this.selectedUserForDelete?.email} has been permanently removed`
            : `User ${this.selectedUserForDelete?.email} has been deactivated`;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: msg
          });
          this.displayDeleteDialog = false;
          this.selectedUserForDelete = null;
          this.deletingUser = false;
          this.deletePermanent = false;

          this.dashboardService.invalidateDashboardCache();
          if (permanent) {
            this.users = this.users.filter(u => u.id !== deletedUserId);
            this.cdr.detectChanges();
          } else {
            this.loadUsers();
          }
        },
        error: (error) => {
          console.error('Error deleting user:', error);
          const errorMsg = error.error?.error || 'Failed to delete user';
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: errorMsg
          });
          this.deletingUser = false;
          this.deletePermanent = false;
        }
      });
  }

  reactivateUser(user: User): void {
    this.apiService.put(`users/${user.id}/activate`, {})
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: `User ${user.email} has been reactivated`
          });
          this.dashboardService.invalidateDashboardCache();
          this.loadUsers();
        },
        error: (error) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: error.error?.error || 'Failed to reactivate user'
          });
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
