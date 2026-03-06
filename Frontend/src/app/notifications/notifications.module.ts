import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationsComponent } from './notifications.component';
import { NotificationsRoutingModule } from './notifications-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { ButtonModule } from 'primeng/button';
import { TooltipModule } from 'primeng/tooltip';
import { InputTextModule } from 'primeng/inputtext';
import { SharedModule } from '@shared/shared.module';

@NgModule({
  declarations: [NotificationsComponent],
  imports: [
    CommonModule,
    FormsModule,
    NotificationsRoutingModule,
    LayoutModule,
    ButtonModule,
    TooltipModule,
    InputTextModule,
    SharedModule
  ]
})
export class NotificationsModule { }
