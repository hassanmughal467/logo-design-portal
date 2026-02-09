import { Component, OnInit, OnDestroy } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface BusinessSettings {
  businessName: string;
  businessEmail: string;
  businessPhone: string;
  businessAddress: string;
  taxId?: string;
  website?: string;
}

export interface BrandSettings {
  logoUrl?: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
}

export interface InvoiceTemplateSettings {
  headerText: string;
  footerText: string;
  termsAndConditions: string;
  showLogo: boolean;
  showTaxId: boolean;
}

export interface PaymentMethod {
  id: string;
  name: string;
  type: 'bank' | 'paypal' | 'stripe' | 'other';
  accountDetails: string;
  isActive: boolean;
}

export interface NotificationPreferences {
  emailNotifications: boolean;
  orderNotifications: boolean;
  paymentNotifications: boolean;
  clientMessageNotifications: boolean;
  reviewNotifications: boolean;
}

@Component({
  selector: 'app-settings-list',
  templateUrl: './settings-list.component.html',
  styleUrls: ['./settings-list.component.scss']
})
export class SettingsListComponent implements OnInit, OnDestroy {
  activeTab: number = 0;
  loading = false;
  saving = false;

  // Business Info
  businessSettings: BusinessSettings = {
    businessName: '',
    businessEmail: '',
    businessPhone: '',
    businessAddress: '',
    taxId: '',
    website: ''
  };

  // Brand Settings
  brandSettings: BrandSettings = {
    primaryColor: '#0d47a1',
    secondaryColor: '#8b5cf6',
    accentColor: '#10b981'
  };
  logoFile: File | null = null;
  logoPreview: string | null = null;

  // Invoice Template
  invoiceTemplate: InvoiceTemplateSettings = {
    headerText: 'Thank you for your business!',
    footerText: 'Payment is due within 30 days.',
    termsAndConditions: '',
    showLogo: true,
    showTaxId: true
  };

  // Payment Methods
  paymentMethods: PaymentMethod[] = [];
  newPaymentMethod: PaymentMethod = {
    id: '',
    name: '',
    type: 'bank',
    accountDetails: '',
    isActive: true
  };
  showPaymentDialog = false;
  editingPaymentIndex: number | null = null;

  // Notification Preferences
  notificationPrefs: NotificationPreferences = {
    emailNotifications: true,
    orderNotifications: true,
    paymentNotifications: true,
    clientMessageNotifications: true,
    reviewNotifications: true
  };

  // Password Change
  passwordForm = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };
  changingPassword = false;

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadSettings();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadSettings(): void {
    this.loading = true;
    // Load from API
    this.apiService.get<any>('settings')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          // Map backend response to frontend structure
          if (data.business) {
            this.businessSettings.businessName = data.business['businessName'] || '';
            this.businessSettings.businessEmail = data.business['businessEmail'] || '';
            this.businessSettings.businessPhone = data.business['businessPhone'] || '';
            this.businessSettings.businessAddress = data.business['businessAddress'] || '';
            this.businessSettings.taxId = data.business['taxId'] || '';
            this.businessSettings.website = data.business['website'] || '';
          }
          if (data.brand) {
            this.brandSettings.primaryColor = data.brand['primaryColor'] || '#0d47a1';
            this.brandSettings.secondaryColor = data.brand['secondaryColor'] || '#8b5cf6';
            this.brandSettings.accentColor = data.brand['accentColor'] || '#10b981';
            if (data.brand['logoUrl']) {
              this.logoPreview = data.brand['logoUrl'];
              this.brandSettings.logoUrl = data.brand['logoUrl'];
            }
          }
          if (data.invoiceTemplate) {
            this.invoiceTemplate.headerText = data.invoiceTemplate['headerText'] || '';
            this.invoiceTemplate.footerText = data.invoiceTemplate['footerText'] || '';
            this.invoiceTemplate.termsAndConditions = data.invoiceTemplate['termsAndConditions'] || '';
            this.invoiceTemplate.showLogo = data.invoiceTemplate['showLogo'] === 'true' || data.invoiceTemplate['showLogo'] === true;
            this.invoiceTemplate.showTaxId = data.invoiceTemplate['showTaxId'] === 'true' || data.invoiceTemplate['showTaxId'] === true;
          }
          if (data.paymentMethods && Array.isArray(data.paymentMethods)) {
            this.paymentMethods = data.paymentMethods;
          }
          if (data.notifications) {
            this.notificationPrefs.emailNotifications = data.notifications['emailNotifications'] === true || data.notifications['emailNotifications'] === 'true';
            this.notificationPrefs.orderNotifications = data.notifications['orderNotifications'] === true || data.notifications['orderNotifications'] === 'true';
            this.notificationPrefs.paymentNotifications = data.notifications['paymentNotifications'] === true || data.notifications['paymentNotifications'] === 'true';
            this.notificationPrefs.clientMessageNotifications = data.notifications['clientMessageNotifications'] === true || data.notifications['clientMessageNotifications'] === 'true';
            this.notificationPrefs.reviewNotifications = data.notifications['reviewNotifications'] === true || data.notifications['reviewNotifications'] === 'true';
          }
          this.loading = false;
        },
        error: () => {
          // Use defaults if API doesn't exist yet
          this.loading = false;
        }
      });
  }

  saveBusinessSettings(): void {
    this.saving = true;
    const data = {
      businessName: this.businessSettings.businessName,
      businessEmail: this.businessSettings.businessEmail,
      businessPhone: this.businessSettings.businessPhone,
      businessAddress: this.businessSettings.businessAddress,
      taxId: this.businessSettings.taxId,
      website: this.businessSettings.website
    };
    this.apiService.post('settings/business', data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Business settings saved successfully'
          });
          this.saving = false;
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save business settings'
          });
          this.saving = false;
        }
      });
  }

  onLogoSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (file.size > 5 * 1024 * 1024) {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Logo file size must be less than 5MB'
        });
        return;
      }
      this.logoFile = file;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.logoPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  uploadLogo(): void {
    if (!this.logoFile) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please select a logo file'
      });
      return;
    }

    this.saving = true;
    const formData = new FormData();
    formData.append('logo', this.logoFile);

    this.apiService.post('settings/logo', formData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response: any) => {
          this.brandSettings.logoUrl = response.logoUrl;
          this.logoPreview = response.logoUrl;
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Logo uploaded successfully'
          });
          this.saving = false;
        },
        error: (error) => {
          console.error('Logo upload error:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to upload logo'
          });
          this.saving = false;
        }
      });
  }

  saveBrandSettings(): void {
    this.saving = true;
    const data = {
      primaryColor: this.brandSettings.primaryColor,
      secondaryColor: this.brandSettings.secondaryColor,
      accentColor: this.brandSettings.accentColor,
      logoUrl: this.brandSettings.logoUrl
    };
    this.apiService.post('settings/brand', data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Brand settings saved successfully'
          });
          this.saving = false;
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save brand settings'
          });
          this.saving = false;
        }
      });
  }

  saveInvoiceTemplate(): void {
    this.saving = true;
    const data = {
      headerText: this.invoiceTemplate.headerText,
      footerText: this.invoiceTemplate.footerText,
      termsAndConditions: this.invoiceTemplate.termsAndConditions,
      showLogo: this.invoiceTemplate.showLogo,
      showTaxId: this.invoiceTemplate.showTaxId
    };
    this.apiService.post('settings/invoice-template', data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Invoice template saved successfully'
          });
          this.saving = false;
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save invoice template'
          });
          this.saving = false;
        }
      });
  }

  addPaymentMethod(): void {
    this.newPaymentMethod = {
      id: '',
      name: '',
      type: 'bank',
      accountDetails: '',
      isActive: true
    };
    this.editingPaymentIndex = null;
    this.showPaymentDialog = true;
  }

  editPaymentMethod(index: number): void {
    this.newPaymentMethod = { ...this.paymentMethods[index] };
    this.editingPaymentIndex = index;
    this.showPaymentDialog = true;
  }

  savePaymentMethod(): void {
    if (!this.newPaymentMethod.name || !this.newPaymentMethod.accountDetails) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please fill in all required fields'
      });
      return;
    }

    if (this.editingPaymentIndex !== null) {
      this.paymentMethods[this.editingPaymentIndex] = { ...this.newPaymentMethod };
    } else {
      this.newPaymentMethod.id = Date.now().toString();
      this.paymentMethods.push({ ...this.newPaymentMethod });
    }

    this.savePaymentMethods();
    this.showPaymentDialog = false;
  }

  deletePaymentMethod(index: number): void {
    this.paymentMethods.splice(index, 1);
    this.savePaymentMethods();
  }

  savePaymentMethods(): void {
    this.saving = true;
    this.apiService.post('settings/payment-methods', { paymentMethods: this.paymentMethods })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Payment methods saved successfully'
          });
          this.saving = false;
        },
        error: (error) => {
          console.error('Payment methods save error:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save payment methods'
          });
          this.saving = false;
        }
      });
  }

  saveNotificationPreferences(): void {
    this.saving = true;
    const data = {
      emailNotifications: this.notificationPrefs.emailNotifications,
      orderNotifications: this.notificationPrefs.orderNotifications,
      paymentNotifications: this.notificationPrefs.paymentNotifications,
      clientMessageNotifications: this.notificationPrefs.clientMessageNotifications,
      reviewNotifications: this.notificationPrefs.reviewNotifications
    };
    this.apiService.post('settings/notifications', data)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Notification preferences saved successfully'
          });
          this.saving = false;
        },
        error: () => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: 'Failed to save notification preferences'
          });
          this.saving = false;
        }
      });
  }

  getPaymentTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'bank': 'Bank Transfer',
      'paypal': 'PayPal',
      'stripe': 'Stripe',
      'other': 'Other'
    };
    return labels[type] || type;
  }

  changePassword(event?: Event): void {
    if (event) {
      event.preventDefault();
    }

    // Get and trim all fields
    const currentPassword = this.passwordForm.currentPassword ? String(this.passwordForm.currentPassword).trim() : '';
    const newPassword = this.passwordForm.newPassword ? String(this.passwordForm.newPassword).trim() : '';
    const confirmPassword = this.passwordForm.confirmPassword ? String(this.passwordForm.confirmPassword).trim() : '';

    // Validate all fields are filled
    if (!currentPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter your current password'
      });
      return;
    }

    if (!newPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please enter a new password'
      });
      return;
    }

    if (!confirmPassword) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'Please confirm your new password'
      });
      return;
    }

    // Validate password length (relaxed to 8 characters minimum)
    if (newPassword.length < 8) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warning',
        detail: 'New password must be at least 8 characters long'
      });
      return;
    }

    if (newPassword !== confirmPassword) {
      this.messageService.add({
        severity: 'error',
        summary: 'Error',
        detail: 'New password and confirmation password do not match'
      });
      return;
    }

    this.changingPassword = true;
    
    // Log the request for debugging
    console.log('Changing password for user...');
    console.log('New password length:', newPassword.length);
    console.log('New password meets requirements:', {
      hasUppercase: /[A-Z]/.test(newPassword),
      hasLowercase: /[a-z]/.test(newPassword),
      hasNumber: /\d/.test(newPassword),
      hasSpecial: /[@$!%*?&#]/.test(newPassword),
      minLength: newPassword.length >= 12
    });
    
    this.apiService.post('auth/change-password', {
      currentPassword: currentPassword,
      newPassword: newPassword,
      confirmPassword: confirmPassword
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          console.log('Password change successful:', response);
          this.messageService.add({
            severity: 'success',
            summary: 'Success',
            detail: 'Password changed successfully'
          });
          // Clear form
          this.passwordForm.currentPassword = '';
          this.passwordForm.newPassword = '';
          this.passwordForm.confirmPassword = '';
          this.changingPassword = false;
        },
        error: (error) => {
          console.error('Password change error - Full error object:', error);
          console.error('Error status:', error?.status);
          console.error('Error error:', error?.error);
          
          // Extract error message from different possible response formats
          let errorMessage = 'Failed to change password';
          
          if (error?.error) {
            if (typeof error.error === 'string') {
              errorMessage = error.error;
            } else if (error.error.error) {
              errorMessage = error.error.error;
            } else if (error.error.message) {
              errorMessage = error.error.message;
            } else if (Array.isArray(error.error) && error.error.length > 0) {
              // Handle validation errors array
              errorMessage = error.error.map((e: any) => e.message || e).join(', ');
            } else if (error.error.title) {
              // ASP.NET Core validation errors
              errorMessage = error.error.title;
              if (error.error.errors) {
                const validationErrors = Object.values(error.error.errors).flat().join(', ');
                if (validationErrors) {
                  errorMessage += ': ' + validationErrors;
                }
              }
            }
          } else if (error?.message) {
            errorMessage = error.message;
          }
          
          // Check for specific error types
          if (error?.status === 401) {
            errorMessage = 'Current password is incorrect. Please verify and try again.';
          } else if (error?.status === 400) {
            // Keep the backend error message for 400 errors, but make it more user-friendly
            if (!errorMessage || errorMessage === 'Failed to change password') {
              errorMessage = 'Password validation failed. Please ensure your password meets all requirements.';
            }
          } else if (error?.status === 0) {
            errorMessage = 'Unable to connect to the server. Please ensure the backend is running.';
          }
          
          this.messageService.add({
            severity: 'error',
            summary: 'Password Change Failed',
            detail: errorMessage,
            life: 7000
          });
          this.changingPassword = false;
        }
      });
  }

  checkPasswordStrength(password: string): { isStrong: boolean; message: string; strength: number } {
    let strength = 0;
    const issues: string[] = [];

    // Length check (relaxed)
    if (password.length >= 12) strength += 2;
    else if (password.length >= 8) strength += 2; // Give full points for 8+
    else issues.push('at least 8 characters');

    // Uppercase check (optional - just adds strength)
    if (/[A-Z]/.test(password)) strength += 1;

    // Lowercase check (optional - just adds strength)
    if (/[a-z]/.test(password)) strength += 1;

    // Number check (optional - just adds strength)
    if (/\d/.test(password)) strength += 1;

    // Special character check (optional - just adds strength)
    if (/[@$!%*?&#]/.test(password)) strength += 1;

    // Always consider password strong if it meets minimum length
    const isStrong = password.length >= 8;
    const message = isStrong ? 'Password meets minimum requirements' : 'Password must be at least 8 characters';

    return { isStrong, message, strength };
  }

  hasSequentialChars(str: string): boolean {
    for (let i = 0; i < str.length - 3; i++) {
      const seq = str.substring(i, i + 4).toLowerCase();
      let isSeq = true;
      for (let j = 1; j < seq.length; j++) {
        const diff = seq.charCodeAt(j) - seq.charCodeAt(j - 1);
        if (diff !== 1 && diff !== -1) {
          isSeq = false;
          break;
        }
      }
      if (isSeq) return true;
    }
    return false;
  }

  getPasswordStrengthLabel(): string {
    if (!this.passwordForm.newPassword) return '';
    const strength = this.checkPasswordStrength(this.passwordForm.newPassword);
    if (strength.strength <= 2) return 'Weak';
    if (strength.strength <= 4) return 'Medium';
    if (strength.strength <= 5) return 'Strong';
    return 'Very Strong';
  }

  getPasswordStrengthColor(): string {
    if (!this.passwordForm.newPassword) return '';
    const strength = this.checkPasswordStrength(this.passwordForm.newPassword);
    if (strength.strength <= 2) return '#ef4444'; // red
    if (strength.strength <= 4) return '#f59e0b'; // orange
    if (strength.strength <= 5) return '#10b981'; // green
    return '#059669'; // dark green
  }

  hasSpecialChar(): boolean {
    if (!this.passwordForm.newPassword) return false;
    return /[@$!%*?&#]/.test(this.passwordForm.newPassword);
  }

  hasUppercase(): boolean {
    if (!this.passwordForm.newPassword) return false;
    return /[A-Z]/.test(this.passwordForm.newPassword);
  }

  hasLowercase(): boolean {
    if (!this.passwordForm.newPassword) return false;
    return /[a-z]/.test(this.passwordForm.newPassword);
  }

  hasNumber(): boolean {
    if (!this.passwordForm.newPassword) return false;
    return /\d/.test(this.passwordForm.newPassword);
  }

  hasMinLength(): boolean {
    if (!this.passwordForm.newPassword) return false;
    return this.passwordForm.newPassword.length >= 8;
  }
}
