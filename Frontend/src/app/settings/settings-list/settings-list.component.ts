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
    primaryColor: '#6366f1',
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
            this.brandSettings.primaryColor = data.brand['primaryColor'] || '#6366f1';
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
}
