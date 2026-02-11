import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { MainLayoutComponent } from '../layout/main-layout/main-layout.component';

const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', loadChildren: () => import('./order-list/order-list.module').then(m => m.OrderListModule) },
      { path: 'create', loadChildren: () => import('./order-create/order-create.module').then(m => m.OrderCreateModule) },
      { path: ':id', loadChildren: () => import('./order-detail/order-detail.module').then(m => m.OrderDetailModule) },
      { path: ':orderId/upload', loadChildren: () => import('./file-upload/file-upload.module').then(m => m.FileUploadModule) }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
