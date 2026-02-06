import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { User } from '@shared/models/user.model';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  user: User | null = null;
  sidebarVisible = true;
  hasNotifications = false; // TODO: Implement notification service
  globalSearchQuery = '';

  menuItems: MenuItem[] = [];
  userMenuItems: MenuItem[] = [];
  breadcrumbItems: MenuItem[] = [];
  breadcrumbHome: MenuItem = { icon: 'pi pi-home', routerLink: '/dashboard' };

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Ensure sidebar is visible by default
    this.sidebarVisible = true;
    
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.user = user;
        if (user) {
          this.buildMenuItems();
          this.buildUserMenu();
          console.log('Menu items built:', this.menuItems);
        }
      });

    // Update breadcrumbs on route change
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateBreadcrumbs();
      });

    // Initial breadcrumb update
    this.updateBreadcrumbs();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateBreadcrumbs(): void {
    const route = this.activatedRoute;
    const breadcrumbs: MenuItem[] = [];

    // Get current route segments
    let currentRoute = route.root;
    while (currentRoute.firstChild) {
      currentRoute = currentRoute.firstChild;
      const routeData = currentRoute.snapshot.data;
      const routeUrl = currentRoute.snapshot.url.map(segment => segment.path).join('/');

      if (routeUrl && routeData['breadcrumb']) {
        breadcrumbs.push({
          label: routeData['breadcrumb'],
          routerLink: '/' + routeUrl
        });
      }
    }

    this.breadcrumbItems = breadcrumbs;
  }

  private buildUserMenu(): void {
    this.userMenuItems = [
      {
        label: 'My Profile',
        icon: 'pi pi-user',
        command: () => {
          // TODO: Navigate to profile page
          console.log('Navigate to profile');
        }
      },
      {
        label: 'Settings',
        icon: 'pi pi-cog',
        command: () => {
          // TODO: Navigate to settings
          console.log('Navigate to settings');
        }
      },
      {
        separator: true
      },
      {
        label: 'Logout',
        icon: 'pi pi-sign-out',
        command: () => {
          this.logout();
        }
      }
    ];
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  performGlobalSearch(): void {
    if (!this.globalSearchQuery || this.globalSearchQuery.trim().length === 0) {
      return;
    }

    const query = this.globalSearchQuery.trim().toLowerCase();
    const user = this.authService.getCurrentUser();
    
    // Determine search scope based on user role
    if (user?.role === 'SuperAdmin' || user?.role === 'Admin') {
      // Admin can search across all modules
      // Try orders first
      this.router.navigate(['/orders'], { queryParams: { search: query } });
    } else if (user?.role === 'Client') {
      // Client searches their orders
      this.router.navigate(['/orders'], { queryParams: { search: query } });
    } else if (user?.role === 'Designer') {
      // Designer searches assigned orders
      this.router.navigate(['/orders'], { queryParams: { search: query } });
    }
  }

  private buildMenuItems(): void {
    const user = this.authService.getCurrentUser();
    console.log('buildMenuItems - user:', user, 'role:', user?.role);
    
    if (!user) {
      console.warn('No user found');
      this.menuItems = [];
      return;
    }
    
    if (!user.role) {
      console.warn('User has no role:', user);
      this.menuItems = [];
      return;
    }
    
    // Convert role to string for comparison (handles enum and string)
    const userRole = String(user.role);
    console.log('User role as string:', userRole);

    this.menuItems = [
      {
        label: 'Dashboard',
        icon: 'pi pi-home',
        routerLink: '/dashboard',
        routerLinkActiveOptions: { exact: false },
        command: () => {
          this.router.navigate(['/dashboard']);
          this.sidebarVisible = false;
        }
      }
    ];

    // Add menu items based on role
    if (userRole === 'SuperAdmin' || userRole === 'Admin') {
      this.menuItems.push(
        {
          label: 'Users',
          icon: 'pi pi-users',
          routerLink: '/users',
          badge: userRole === 'SuperAdmin' ? 'Admin' : undefined,
          command: () => {
            this.router.navigate(['/users']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Clients',
          icon: 'pi pi-user',
          routerLink: '/clients',
          command: () => {
            this.router.navigate(['/clients']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Projects',
          icon: 'pi pi-palette',
          routerLink: '/projects',
          command: () => {
            this.router.navigate(['/projects']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Orders',
          icon: 'pi pi-shopping-cart',
          routerLink: '/orders',
          command: () => {
            this.router.navigate(['/orders']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Invoices',
          icon: 'pi pi-file-pdf',
          routerLink: '/invoices',
          command: () => {
            this.router.navigate(['/invoices']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Designers',
          icon: 'pi pi-user-edit',
          routerLink: '/designers',
          command: () => {
            this.router.navigate(['/designers']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Messages',
          icon: 'pi pi-inbox',
          routerLink: '/messages',
          command: () => {
            this.router.navigate(['/messages']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Reviews',
          icon: 'pi pi-star',
          routerLink: '/reviews',
          command: () => {
            this.router.navigate(['/reviews']);
            this.sidebarVisible = false;
          }
        }
      );
    }

    if (userRole === 'SuperAdmin') {
      this.menuItems.push({
        label: 'Permissions',
        icon: 'pi pi-key',
        routerLink: '/permissions',
        badge: 'Admin',
        command: () => {
          this.router.navigate(['/permissions']);
          this.sidebarVisible = false;
        }
      });
    }

    if (userRole === 'Client') {
      this.menuItems.push(
        {
          label: 'My Projects',
          icon: 'pi pi-palette',
          routerLink: '/projects',
          command: () => {
            this.router.navigate(['/projects']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'My Orders',
          icon: 'pi pi-shopping-cart',
          routerLink: '/orders',
          command: () => {
            this.router.navigate(['/orders']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Messages',
          icon: 'pi pi-inbox',
          routerLink: '/messages',
          command: () => {
            this.router.navigate(['/messages']);
            this.sidebarVisible = false;
          }
        }
      );
    }

    if (userRole === 'Designer') {
      this.menuItems.push(
        {
          label: 'My Projects',
          icon: 'pi pi-palette',
          routerLink: '/projects',
          command: () => {
            this.router.navigate(['/projects']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Assigned Orders',
          icon: 'pi pi-shopping-cart',
          routerLink: '/orders',
          command: () => {
            this.router.navigate(['/orders']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Messages',
          icon: 'pi pi-inbox',
          routerLink: '/messages',
          command: () => {
            this.router.navigate(['/messages']);
            this.sidebarVisible = false;
          }
        }
      );
    }

    this.menuItems.push({
      label: 'Files',
      icon: 'pi pi-file',
      routerLink: '/files',
      command: () => {
        this.router.navigate(['/files']);
        this.sidebarVisible = false;
      }
    });
    
    console.log('Menu items built. Total:', this.menuItems.length, 'Items:', this.menuItems.map(m => m.label));
  }
}
