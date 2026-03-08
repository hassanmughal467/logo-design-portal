import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', loadChildren: () => import('./order-list/order-list.module').then(m => m.OrderListModule) },
  { path: 'create', loadChildren: () => import('./order-create/order-create.module').then(m => m.OrderCreateModule) },
  // More specific route must come before :id so /orders/123/upload loads FileUpload, not OrderDetail
  { path: ':orderId/upload', loadChildren: () => import('./file-upload/file-upload.module').then(m => m.FileUploadModule) },
  { path: ':id', loadChildren: () => import('./order-detail/order-detail.module').then(m => m.OrderDetailModule) }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OrdersRoutingModule { }
