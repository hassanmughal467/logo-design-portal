import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DesignCategory, DesignType } from '@shared/models/design-pricing.model';

export interface DesignerOption {
  id: string;
  label: string;
}

export interface DesignerLogoPricing {
  id: string;
  designerId: string;
  designerName?: string;
  designCategory: number;
  designCategoryName: string;
  designType: number;
  designTypeName: string;
  defaultPrice: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt?: Date;
}

@Component({
  selector: 'app-designer-pricing-management',
  templateUrl: './designer-pricing-management.component.html',
  styleUrls: ['./designer-pricing-management.component.scss']
})
export class DesignerPricingManagementComponent implements OnInit, OnDestroy {
  designers: DesignerOption[] = [];
  selectedDesigner: DesignerOption | null = null;
  pricings: DesignerLogoPricing[] = [];
  loading = false;
  loadingPricings = false;
  displayAddDialog = false;
  displayEditDialog = false;
  selectedPricing: DesignerLogoPricing | null = null;
  saving = false;

  addForm: FormGroup;
  editForm: FormGroup;

  designCategoryOptions = [
    { label: 'Embroidery Digitizing', value: DesignCategory.EmbroideryDigitizing },
    { label: 'Vector / Screen Printing', value: DesignCategory.VectorScreenPrinting },
    { label: 'Custom Patch', value: DesignCategory.CustomPatch }
  ];

  designTypeOptions: { label: string; value: number; category?: number }[] = [
    { label: 'Left Chest', value: DesignType.LeftChest, category: DesignCategory.EmbroideryDigitizing },
    { label: 'Jacket Back', value: DesignType.JacketBack, category: DesignCategory.EmbroideryDigitizing },
    { label: 'Simple Vector', value: DesignType.SimpleVector, category: DesignCategory.VectorScreenPrinting },
    { label: 'Complex Vector', value: DesignType.ComplexVector, category: DesignCategory.VectorScreenPrinting },
    { label: 'Left Chest', value: DesignType.LeftChest, category: DesignCategory.CustomPatch },
    { label: 'Jacket Back', value: DesignType.JacketBack, category: DesignCategory.CustomPatch }
  ];

  filteredDesignTypeOptions: { label: string; value: number }[] = [];

  private destroy$ = new Subject<void>();

  constructor(
    private apiService: ApiService,
    private messageService: MessageService,
    private fb: FormBuilder
  ) {
    this.addForm = this.fb.group({
      designerId: ['', Validators.required],
      designCategory: [null, Validators.required],
      designType: [null, Validators.required],
      defaultPrice: [0, [Validators.required, Validators.min(0)]],
      isActive: [true]
    });

    this.editForm = this.fb.group({
      defaultPrice: [0, [Validators.required, Validators.min(0)]],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadDesigners();
    this.addForm.get('designCategory')?.valueChanges.subscribe(cat => {
      this.updateDesignTypeOptions(cat);
      this.addForm.patchValue({ designType: null });
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateDesignTypeOptions(category: number | null): void {
    if (!category) {
      this.filteredDesignTypeOptions = [];
      return;
    }
    this.filteredDesignTypeOptions = this.designTypeOptions
      .filter(opt => opt.category === category)
      .map(opt => ({ label: opt.label, value: opt.value }));
  }

  loadDesigners(): void {
    this.loading = true;
    this.apiService.get<any[]>('users/designer-profiles')
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (profiles) => {
          this.designers = (profiles || []).map(p => ({
            id: p.id,
            label: `${p.userFirstName || ''} ${p.userLastName || ''}`.trim()
              ? `${(p.userFirstName || '').trim()} ${(p.userLastName || '').trim()} (${p.userEmail || 'Designer'})`.trim()
              : (p.userEmail || 'Designer')
          }));
          this.loading = false;
        },
        error: (err) => {
          this.designers = [];
          this.loading = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Failed to load designers',
            detail: err.error?.message || err.error?.error || 'Could not load designer list. Please try again.'
          });
        }
      });
  }

  onDesignerSelect(): void {
    if (this.selectedDesigner) {
      this.loadPricings(this.selectedDesigner.id);
    } else {
      this.pricings = [];
    }
  }

  loadPricings(designerId: string): void {
    this.loadingPricings = true;
    this.apiService.get<DesignerLogoPricing[]>(`designer-logo-pricing/designer/${designerId}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.pricings = data;
          this.loadingPricings = false;
        },
        error: (err) => {
          this.pricings = [];
          this.loadingPricings = false;
          this.messageService.add({
            severity: 'error',
            summary: 'Failed to load pricing',
            detail: err.error?.message || err.error?.error || 'Could not load designer pricing. Please try again.'
          });
        }
      });
  }

  openAddDialog(): void {
    if (!this.selectedDesigner) {
      this.messageService.add({
        severity: 'warn',
        summary: 'Select Designer',
        detail: 'Please select a designer first.'
      });
      return;
    }
    this.addForm.patchValue({
      designerId: this.selectedDesigner.id,
      designCategory: null,
      designType: null,
      defaultPrice: 0,
      isActive: true
    });
    this.filteredDesignTypeOptions = [];
    this.displayAddDialog = true;
  }

  saveAdd(): void {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }
    this.saving = true;
    const val = this.addForm.value;
    this.apiService.post<any>('designer-logo-pricing', {
      designerId: val.designerId,
      designCategory: val.designCategory,
      designType: val.designType,
      defaultPrice: val.defaultPrice,
      isActive: val.isActive
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing added.' });
        this.displayAddDialog = false;
        this.saving = false;
        if (this.selectedDesigner) this.loadPricings(this.selectedDesigner.id);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to add pricing.'
        });
        this.saving = false;
      }
    });
  }

  openEditDialog(pricing: DesignerLogoPricing): void {
    this.selectedPricing = pricing;
    this.editForm.patchValue({
      defaultPrice: pricing.defaultPrice,
      isActive: pricing.isActive
    });
    this.displayEditDialog = true;
  }

  saveEdit(): void {
    if (this.editForm.invalid || !this.selectedPricing) return;
    this.saving = true;
    const val = this.editForm.value;
    this.apiService.put<any>(`designer-logo-pricing/${this.selectedPricing.id}`, {
      defaultPrice: val.defaultPrice,
      isActive: val.isActive
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing updated.' });
        this.displayEditDialog = false;
        this.selectedPricing = null;
        this.saving = false;
        if (this.selectedDesigner) this.loadPricings(this.selectedDesigner.id);
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: err.error?.error || 'Failed to update pricing.'
        });
        this.saving = false;
      }
    });
  }

  deletePricing(pricing: DesignerLogoPricing): void {
    if (!confirm('Delete this pricing? Designer will fall back to global pricing for this category/type.')) return;
    this.apiService.delete<any>(`designer-logo-pricing/${pricing.id}`)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Pricing deleted.' });
          if (this.selectedDesigner) this.loadPricings(this.selectedDesigner.id);
        },
        error: (err) => {
          this.messageService.add({
            severity: 'error',
            summary: 'Error',
            detail: err.error?.error || 'Failed to delete pricing.'
          });
        }
      });
  }

  formatPrice(amount: number): string {
    return new Intl.NumberFormat('en-PK', {
      style: 'currency',
      currency: 'PKR',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0
    }).format(amount);
  }
}
