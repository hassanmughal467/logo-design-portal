import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

// PrimeNG
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ButtonModule } from 'primeng/button';
import { PasswordModule } from 'primeng/password';
import { TooltipModule } from 'primeng/tooltip';
import { DropdownModule } from 'primeng/dropdown';
import { CheckboxModule } from 'primeng/checkbox';

// Components
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { ForgotPasswordComponent } from './forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { LayoutModule } from '../layout/layout.module';
import { AuthRoutingModule } from './auth-routing.module';

@NgModule({
  declarations: [
    LoginComponent,
    RegisterComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    AuthRoutingModule,
    LayoutModule,
    InputTextModule,
    InputTextareaModule,
    ButtonModule,
    PasswordModule,
    TooltipModule,
    DropdownModule,
    CheckboxModule
  ]
})
export class AuthModule { }
