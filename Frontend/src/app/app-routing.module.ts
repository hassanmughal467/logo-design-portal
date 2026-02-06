import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  
  // Auth routes (lazy loaded)
  {
    path: 'login',
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'register',
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },
  
  // Dashboard (lazy loaded)
  {
    path: 'dashboard',
    loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule),
    canActivate: [AuthGuard]
  },
  
  // Users (lazy loaded)
  {
    path: 'users',
    loadChildren: () => import('./users/users.module').then(m => m.UsersModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['SuperAdmin', 'Admin'] }
  },
  
  // Orders (lazy loaded)
  {
    path: 'orders',
    loadChildren: () => import('./orders/orders.module').then(m => m.OrdersModule),
    canActivate: [AuthGuard]
  },
  
  // Designers (lazy loaded)
  {
    path: 'designers',
    loadChildren: () => import('./designers/designers.module').then(m => m.DesignersModule),
    canActivate: [AuthGuard]
  },
  
  // Permissions (lazy loaded)
  {
    path: 'permissions',
    loadChildren: () => import('./permissions/permissions.module').then(m => m.PermissionsModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['SuperAdmin'] }
  },
  
  // Files (lazy loaded)
  {
    path: 'files',
    loadChildren: () => import('./files/files.module').then(m => m.FilesModule),
    canActivate: [AuthGuard]
  },
  
  // Clients (lazy loaded)
  {
    path: 'clients',
    loadChildren: () => import('./clients/clients.module').then(m => m.ClientsModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['SuperAdmin', 'Admin'] }
  },
  
  // Projects (lazy loaded)
  {
    path: 'projects',
    loadChildren: () => import('./projects/projects.module').then(m => m.ProjectsModule),
    canActivate: [AuthGuard]
  },
  
  // Invoices (lazy loaded)
  {
    path: 'invoices',
    loadChildren: () => import('./invoices/invoices.module').then(m => m.InvoicesModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['SuperAdmin', 'Admin'] }
  },
  
  // Messages (lazy loaded)
  {
    path: 'messages',
    loadChildren: () => import('./messages/messages.module').then(m => m.MessagesModule),
    canActivate: [AuthGuard]
  },
  
  // Reviews (lazy loaded)
  {
    path: 'reviews',
    loadChildren: () => import('./reviews/reviews.module').then(m => m.ReviewsModule),
    canActivate: [AuthGuard]
  },
  
  // Wildcard route
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
