import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  registerForm: FormGroup;
  loading = false;
  logoExists: boolean = true; // Set to false if logo file doesn't exist

  countries = [
    { label: 'United States', value: 'United States' },
    { label: 'Canada', value: 'Canada' },
    { label: 'United Kingdom', value: 'United Kingdom' },
    { label: 'Australia', value: 'Australia' },
    { label: 'Germany', value: 'Germany' },
    { label: 'France', value: 'France' },
    { label: 'Other', value: 'Other' }
  ];

  references = [
    { label: 'Google Search', value: 'Google Search' },
    { label: 'Social Media', value: 'Social Media' },
    { label: 'Friend/Colleague', value: 'Friend/Colleague' },
    { label: 'Advertisement', value: 'Advertisement' },
    { label: 'Other', value: 'Other' }
  ];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private messageService: MessageService
  ) {
    this.registerForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: [''], // Optional - not in template
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      secondaryEmail: ['', [Validators.email]],
      invoiceEmail: ['', [Validators.required, Validators.email]],
      companyName: ['', [Validators.required]],
      contactName: ['', [Validators.required]],
      phone: ['', [Validators.required]],
      cell: [''],
      fax: [''],
      country: [''],
      city: [''],
      zipCode: [''],
      state: [''],
      address: [''],
      website: [''],
      reference: [''],
      agreeToTerms: [false, [Validators.requiredTrue]]
    });
  }


  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const { confirmPassword, ...registerData } = this.registerForm.value;

    this.authService.register(registerData).subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Registration successful. Please login.'
        });
        this.router.navigate(['/login']);
      },
      error: (error) => {
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Registration Failed',
          detail: error.error?.error || error.error?.message || 'Registration failed. Please try again.'
        });
      }
    });
  }
}
