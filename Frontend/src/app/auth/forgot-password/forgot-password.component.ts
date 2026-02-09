import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.scss']
})
export class ForgotPasswordComponent {
  logoExists: boolean = true; // Set to false if logo file doesn't exist
  forgotPasswordForm: FormGroup;
  loading = false;
  emailSent = false;
  resetLink: string = '';
  linkCopied = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private messageService: MessageService,
    private router: Router
  ) {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.forgotPasswordForm.invalid) {
      this.forgotPasswordForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const email = this.forgotPasswordForm.get('email')?.value;

    this.authService.forgotPassword(email).subscribe({
      next: (response: any) => {
        this.emailSent = true;
        // Store reset link for display (development mode only)
        if (response.resetLink) {
          this.resetLink = `${window.location.origin}${response.resetLink}`;
        }
        
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Password reset link has been sent. Please check your email.'
        });
        this.loading = false;
      },
      error: (error) => {
        const errorMessage = error.error?.error || 'Failed to send password reset link. Please try again.';
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: errorMessage
        });
        this.loading = false;
        this.emailSent = false; // Reset so user can try again
      }
    });
  }

  backToLogin(): void {
    this.router.navigate(['/login']);
  }

  copyResetLink(): void {
    if (this.resetLink) {
      navigator.clipboard.writeText(this.resetLink).then(() => {
        this.linkCopied = true;
        this.messageService.add({
          severity: 'success',
          summary: 'Copied!',
          detail: 'Reset link copied to clipboard',
          life: 2000
        });
        setTimeout(() => {
          this.linkCopied = false;
        }, 2000);
      }).catch(err => {
        console.error('Failed to copy:', err);
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to copy link. Please copy it manually.'
        });
      });
    }
  }

  openResetPage(): void {
    if (this.resetLink) {
      window.open(this.resetLink, '_blank');
    }
  }
}
