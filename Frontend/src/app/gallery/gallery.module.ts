import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { GalleryComponent } from './gallery.component';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';

const routes: Routes = [
  {
    path: '',
    component: GalleryComponent
  }
];

@NgModule({
  declarations: [GalleryComponent],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ButtonModule,
    DialogModule
  ]
})
export class GalleryModule { }
