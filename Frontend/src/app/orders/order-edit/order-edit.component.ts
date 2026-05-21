import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '@core/services/api.service';
import { MessageService } from 'primeng/api';
import { Order, OrderPriority } from '@shared/models/order.model';

@Component({
  selector: 'app-order-edit',
  templateUrl: './order-edit.component.html',
  styleUrls: ['./order-edit.component.scss']
})
export class OrderEditComponent implements OnInit, OnChanges {
  @Input() order: Order | null = null;
  @Output() orderUpdated = new EventEmitter<Order>();
  @Output() cancel = new EventEmitter<void>();

  orderForm: FormGroup;
  loading = false;
  minDate: Date = new Date();
  priorityOptions = [
    { label: 'Normal', value: OrderPriority.Medium },
    { label: 'Rush', value: OrderPriority.Urgent }
  ];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private messageService: MessageService
  ) {
    this.orderForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      price: [0, [Validators.required, Validators.min(0.01)]],
      priority: [OrderPriority.Medium],
      deadline: [null],
      requiredFormats: [''],
      requirements: [''],
      referenceWebsite: [''],
      colorPreferences: [''],
      stylePreferences: ['']
    });
  }

  ngOnInit(): void {
    this.loadOrderData();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['order'] && changes['order'].currentValue) {
      this.loadOrderData();
    }
  }

  /** Client-facing: price is set through approval flow; hide editor until finalized. */
  get showPriceField(): boolean {
    return this.order?.priceApproved === true;
  }

  loadOrderData(): void {
    if (!this.order) return;

    // Parse deadline if it exists (check both deadline and dueDate for compatibility)
    let deadlineDate: Date | null = null;
    const deadlineValue = this.order.deadline || this.order.dueDate;
    if (deadlineValue) {
      deadlineDate = new Date(deadlineValue);
    }

    // Map priority: only Normal and Rush supported; map Low/High to Normal
    let priority: OrderPriority = OrderPriority.Medium;
    if (this.order.priority) {
      const p = this.order.priority as OrderPriority;
      priority = (p === OrderPriority.Urgent) ? OrderPriority.Urgent : OrderPriority.Medium;
    }

    this.orderForm.patchValue({
      title: this.order.title || '',
      description: this.order.description || '',
      price: this.order.price || 0,
      priority: priority,
      deadline: deadlineDate,
      requiredFormats: this.order.requiredFormats || '',
      requirements: this.order.requirements || '',
      colorPreferences: this.order.colorPreferences || '',
      stylePreferences: this.order.stylePreferences || ''
    });
    this.updatePriceValidators();
  }

  private updatePriceValidators(): void {
    const priceCtrl = this.orderForm.get('price');
    if (!priceCtrl) return;
    if (this.showPriceField) {
      priceCtrl.setValidators([Validators.required, Validators.min(0.01)]);
    } else {
      priceCtrl.clearValidators();
    }
    priceCtrl.updateValueAndValidity({ emitEvent: false });
  }

  onSubmit(): void {
    if (this.orderForm.invalid) {
      this.markFormGroupTouched(this.orderForm);
      return;
    }

    if (!this.order) return;

    this.loading = true;
    const formValue = this.orderForm.value;
    
    const orderData = {
      title: formValue.title,
      description: formValue.description,
      price: this.showPriceField
        ? formValue.price
        : Math.max(Number(this.order.price) || 0, 0.01),
      priority: formValue.priority || OrderPriority.Medium,
      deadline: formValue.deadline ? new Date(formValue.deadline) : undefined,
      instructions: this.order.instructions || undefined,
      requiredFormats: formValue.requiredFormats || undefined,
      requirements: formValue.requirements || undefined,
      colorPreferences: formValue.colorPreferences || undefined,
      stylePreferences: formValue.stylePreferences || undefined
    };

    this.apiService.put<Order>(`orders/${this.order.id}`, orderData).subscribe({
      next: (updatedOrder) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Order updated successfully'
        });
        this.loading = false;
        this.orderUpdated.emit(updatedOrder);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to update order'
        });
        this.loading = false;
      }
    });
  }

  onCancel(): void {
    this.cancel.emit();
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  get f() {
    return this.orderForm.controls;
  }
}
