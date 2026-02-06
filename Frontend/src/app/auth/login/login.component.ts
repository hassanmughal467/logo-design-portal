import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  returnUrl: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private messageService: MessageService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    // Get return url from route parameters or default to '/dashboard'
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
    
    // If already logged in, redirect
    if (this.authService.isAuthenticated()) {
      this.router.navigate([this.returnUrl]);
    }
  }

  onSubmit(): void {
    // Mark all fields as touched to show validation errors
    if (this.loginForm.invalid) {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    const { email, password } = this.loginForm.value;

    console.log('Attempting login with:', { email });

    this.authService.login({ email, password }).subscribe({
      next: (response) => {
        console.log('Login successful:', response);
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: 'Login successful'
        });
        this.router.navigate([this.returnUrl]);
      },
      error: (error) => {
        console.error('Login error:', error);
        this.loading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Login Failed',
          detail: error.error?.message || error.message || 'Invalid email or password. Please check your credentials and ensure the backend is running.'
        });
      }
    });
  }
}
