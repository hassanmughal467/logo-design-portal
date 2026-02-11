import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { NotificationsComponent } from './notifications.component';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';

const routes: Routes = [
  {
    path: '',
    component: NotificationsComponent
  }
];

@NgModule({
  declarations: [NotificationsComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ButtonModule,
    TagModule
  ]
})
export class NotificationsModule { }
