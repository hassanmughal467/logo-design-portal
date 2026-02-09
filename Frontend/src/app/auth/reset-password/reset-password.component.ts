import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.scss']
})
export class ResetPasswordComponent implements OnInit {
  resetPasswordForm: FormGroup;
  loading = false;
  email: string = '';
  token: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.resetPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  ngOnInit(): void {
    // Get email and token from query parameters
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';
      
      if (!this.email || !this.token) {
        this.messageService.add({
          severity: 'error',
          summary: 'Invalid Link',
          detail: 'The reset link is invalid or missing required parameters.'
        });
        this.router.navigate(['/forgot-password']);
      }
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

  onSubmit(): void {
    if (this.resetPasswordForm.invalid) {
      this.resetPasswordForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const formValue = this.resetPasswordForm.value;

    this.authService.resetPasswordWithToken({
      email: this.email,
      token: this.token,
      newPassword: formValue.newPassword,
      confirmPassword: formValue.confirmPassword
    }).subscribe({
      next: (response) => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Password has been reset successfully. You can now login with your new password.'
        });
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: error.error?.error || 'Failed to reset password. The link may have expired.'
        });
        this.loading = false;
      }
    });
  }

  checkPasswordStrength(password: string): number {
    let strength = 0;
    if (password.length >= 8) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[^a-zA-Z0-9]/.test(password)) strength++;
    return strength;
  }

  getPasswordStrengthLabel(strength: number): string {
    if (strength <= 1) return 'Weak';
    if (strength <= 3) return 'Medium';
    return 'Strong';
  }

  getPasswordStrengthColor(strength: number): string {
    if (strength <= 1) return 'red';
    if (strength <= 3) return 'orange';
    return 'green';
  }
}
