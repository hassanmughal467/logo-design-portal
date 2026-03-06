import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  
  // Auth routes (lazy loaded) - use a single parent route
  {
    path: 'auth',
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },
  // Redirect old paths to new structure for backward compatibility
  { path: 'login', redirectTo: '/auth/login', pathMatch: 'full' },
  { path: 'register', redirectTo: '/auth/register', pathMatch: 'full' },
  { path: 'forgot-password', redirectTo: '/auth/forgot-password', pathMatch: 'full' },
  { path: 'reset-password', redirectTo: '/auth/reset-password', pathMatch: 'full' },
  
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
    canActivate: [AuthGuard]
  },
  
  // Analytics (lazy loaded)
  {
    path: 'analytics',
    loadChildren: () => import('./analytics/analytics.module').then(m => m.AnalyticsModule),
    canActivate: [AuthGuard]
  },
  
  // Financial Overview (lazy loaded)
  {
    path: 'financial',
    loadChildren: () => import('./financial/financial.module').then(m => m.FinancialModule),
    canActivate: [AuthGuard]
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

  // Settings (lazy loaded)
  {
    path: 'settings',
    loadChildren: () => import('./settings/settings.module').then(m => m.SettingsModule),
    canActivate: [AuthGuard]
  },

  // Notifications (lazy loaded)
  {
    path: 'notifications',
    loadChildren: () => import('./notifications/notifications.module').then(m => m.NotificationsModule),
    canActivate: [AuthGuard],
    data: { breadcrumb: 'Notifications' }
  },

  // Gallery (lazy loaded)
  {
    path: 'gallery',
    loadChildren: () => import('./gallery/gallery.module').then(m => m.GalleryModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Client'] }
  },

  // Wildcard route
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
