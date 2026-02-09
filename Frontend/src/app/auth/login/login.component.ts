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
  loginError: string = '';
  logoExists: boolean = true; // Set to false if logo file doesn't exist

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
    
    // Clear login errors when user starts typing in password field
    this.loginForm.get('password')?.valueChanges.subscribe(() => {
      if (this.loginError) {
        this.loginError = '';
      }
    });
    
    // Also clear when email changes
    this.loginForm.get('email')?.valueChanges.subscribe(() => {
      if (this.loginError) {
        this.loginError = '';
      }
    });
  }

  onSubmit(): void {
    // Clear previous login error
    this.loginError = '';
    
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
        // Clear any errors on success
        this.loginError = '';
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
        
        // Extract error message from backend response
        // Backend returns { error: "message" } for 401 Unauthorized
        let errorMessage = 'Invalid email or password';
        
        if (error?.error?.error) {
          errorMessage = error.error.error;
        } else if (error?.error?.message) {
          errorMessage = error.error.message;
        } else if (error?.message) {
          errorMessage = error.message;
        } else if (error?.status === 0 || error?.status === 500) {
          errorMessage = 'Unable to connect to the server. Please ensure the backend is running.';
        }
        
        // Set login error message (will show below password field)
        this.loginError = errorMessage;
        
        // Show toast notification for login failure
        this.messageService.add({
          severity: 'error',
          summary: 'Login Failed',
          detail: errorMessage,
          life: 5000
        });
        
        // Mark password field as touched to show red outline
        this.loginForm.get('password')?.markAsTouched();
      }
    });
  }
}
