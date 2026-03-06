import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { NotificationService } from '@core/services/notification.service';
import { User } from '@shared/models/user.model';
import { Notification } from '@shared/models/notification.model';
import { getNotificationIcon } from '@shared/utils/notification-helpers';
import { Observable, Subject } from 'rxjs';
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
  globalSearchQuery = '';
  recentNotifications: Notification[] = [];
  loadingNotifications = false;

  unreadCount$!: Observable<number>;

  menuItems: MenuItem[] = [];
  userMenuItems: MenuItem[] = [];
  breadcrumbItems: MenuItem[] = [];
  breadcrumbHome: MenuItem = { icon: 'pi pi-home', routerLink: '/dashboard' };

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private activatedRoute: ActivatedRoute
  ) {
    this.unreadCount$ = this.notificationService.unreadCount$;
  }

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
          this.loadRecentNotifications();
          console.log('Menu items built:', this.menuItems);
        }
      });

    // Refresh notification dropdown when realtime notification arrives
    this.notificationService.refreshRequested$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.loadNotifications(5));

    // Activity feed: update existing notification in place when SignalR pushes an update
    this.notificationService.notificationReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => this.mergeRealtimeNotification(notification));

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

  loadRecentNotifications(): void {
    this.loadNotifications(5);
  }

  private mergeRealtimeNotification(notification: Notification): void {
    const idx = this.recentNotifications.findIndex(n => n.id === notification.id);
    if (idx >= 0) {
      this.recentNotifications = [
        ...this.recentNotifications.slice(0, idx),
        notification,
        ...this.recentNotifications.slice(idx + 1)
      ];
    } else {
      this.recentNotifications = [notification, ...this.recentNotifications].slice(0, 5);
    }
  }

  private loadNotifications(limit: number = 5): void {
    this.loadingNotifications = true;
    this.notificationService.getNotifications(false, limit)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (notifications) => {
          this.recentNotifications = Array.isArray(notifications) ? notifications : [];
          this.loadingNotifications = false;
        },
        error: () => {
          this.recentNotifications = [];
          this.loadingNotifications = false;
        }
      });
  }

  markNotificationAsRead(notification: Notification): void {
    if (notification.isRead) return;
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
      }
    });
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.recentNotifications.forEach(n => n.isRead = true);
      }
    });
  }

  hasUnreadNotifications(): boolean {
    return this.recentNotifications.some(n => !n.isRead);
  }

  onNotificationClick(notification: Notification): void {
    this.markNotificationAsRead(notification);
    this.navigateFromNotification(notification);
  }

  getNotificationIcon(notification: Notification): string {
    return getNotificationIcon(notification);
  }

  /** Navigate to the appropriate page based on notification reference type */
  private navigateFromNotification(notification: Notification): void {
    const refType = (notification.referenceType ?? 'Order').toLowerCase();
    const refId = notification.referenceId ?? notification.orderId;
    if (!refId) return;

    switch (refType) {
      case 'order':
        this.router.navigate(['/orders', refId]);
        break;
      case 'invoice':
        this.router.navigate(['/invoices', refId]);
        break;
      case 'message':
        this.router.navigate(['/orders', notification.orderId ?? refId, 'messages']);
        break;
      default:
        if (notification.orderId) {
          this.router.navigate(['/orders', notification.orderId]);
        }
    }
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

    // Base menu items - Dashboard only (other items are role-specific)
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
          label: 'Orders',
          icon: 'pi pi-shopping-cart',
          routerLink: '/orders',
          command: () => {
            this.router.navigate(['/orders']);
            if (window.innerWidth < 768) {
              this.sidebarVisible = false;
            }
          }
        },
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
        },
        {
          label: 'Files',
          icon: 'pi pi-file',
          routerLink: '/files',
          command: () => {
            this.router.navigate(['/files']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Invoices',
          icon: 'pi pi-money-bill',
          routerLink: '/invoices',
          command: () => {
            this.router.navigate(['/invoices']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Detail Analytics',
          icon: 'pi pi-chart-bar',
          routerLink: '/analytics',
          command: () => {
            this.router.navigate(['/analytics']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Financial Overview',
          icon: 'pi pi-wallet',
          routerLink: '/financial',
          command: () => {
            this.router.navigate(['/financial']);
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
          label: 'My Orders',
          icon: 'pi pi-shopping-cart',
          routerLink: '/orders',
          command: () => {
            this.router.navigate(['/orders']);
            if (window.innerWidth < 768) {
              this.sidebarVisible = false;
            }
          }
        },
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
          label: 'Detail Analytics',
          icon: 'pi pi-chart-bar',
          routerLink: '/analytics',
          command: () => {
            this.router.navigate(['/analytics']);
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
          label: 'Files',
          icon: 'pi pi-file',
          routerLink: '/files',
          command: () => {
            this.router.navigate(['/files']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Invoices',
          icon: 'pi pi-money-bill',
          routerLink: '/invoices',
          command: () => {
            this.router.navigate(['/invoices']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Financial Overview',
          icon: 'pi pi-wallet',
          routerLink: '/financial',
          command: () => {
            this.router.navigate(['/financial']);
            this.sidebarVisible = false;
          }
        }
      );
    }

    if (userRole === 'Designer') {
      this.menuItems.push(
        {
          label: 'Assigned Orders',
          icon: 'pi pi-shopping-cart',
          routerLink: '/orders',
          command: () => {
            this.router.navigate(['/orders']);
            // Only close sidebar on mobile/small screens
            if (window.innerWidth < 768) {
              this.sidebarVisible = false;
            }
          }
        },
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
          label: 'Messages',
          icon: 'pi pi-inbox',
          routerLink: '/messages',
          command: () => {
            this.router.navigate(['/messages']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Files',
          icon: 'pi pi-file',
          routerLink: '/files',
          command: () => {
            this.router.navigate(['/files']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Invoices',
          icon: 'pi pi-money-bill',
          routerLink: '/invoices',
          command: () => {
            this.router.navigate(['/invoices']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Detail Analytics',
          icon: 'pi pi-chart-bar',
          routerLink: '/analytics',
          command: () => {
            this.router.navigate(['/analytics']);
            this.sidebarVisible = false;
          }
        },
        {
          label: 'Financial Overview',
          icon: 'pi pi-wallet',
          routerLink: '/financial',
          command: () => {
            this.router.navigate(['/financial']);
            this.sidebarVisible = false;
          }
        }
      );
    }

    // Add Notifications for all authenticated users
    this.menuItems.push({
      label: 'Notifications',
      icon: 'pi pi-bell',
      routerLink: '/notifications',
      routerLinkActiveOptions: { exact: true },
      command: () => {
        this.router.navigate(['/notifications']);
        this.sidebarVisible = false;
      }
    });

    // Add Settings at the end for all authenticated users
    this.menuItems.push({
      label: 'Settings',
      icon: 'pi pi-cog',
      routerLink: '/settings',
      command: () => {
        this.router.navigate(['/settings']);
        this.sidebarVisible = false;
      }
    });
    
    console.log('Menu items built. Total:', this.menuItems.length, 'Items:', this.menuItems.map(m => m.label));
  }
}
