import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { NotificationService } from '@core/services/notification.service';
import { User } from '@shared/models/user.model';
import { Notification } from '@shared/models/notification.model';
import { getNotificationIcon } from '@shared/utils/notification-helpers';
import { Observable, Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';
import { MenuItem } from 'primeng/api';
import { OverlayPanel } from 'primeng/overlaypanel';

@Component({
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.scss']
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  @ViewChild('notificationPanel') notificationPanel!: OverlayPanel;
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
        }
      });

    // Refresh notification dropdown only when it is already open (never auto-open on realtime notification)
    this.notificationService.refreshRequested$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.notificationPanel?.overlayVisible) {
          this.loadNotifications(5);
        }
      });

    // Activity feed: update existing notification in place when SignalR pushes an update (only if panel is open)
    this.notificationService.notificationReceived$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notification => {
        if (this.notificationPanel?.overlayVisible) {
          this.mergeRealtimeNotification(notification);
        }
      });

    // Update breadcrumbs and close sidebar on mobile when route changes
    this.router.events
      .pipe(
        filter(event => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.updateBreadcrumbs();
        // Close sidebar on mobile after navigation (prevents blink, avoids double-close)
        if (window.innerWidth < 768) {
          this.sidebarVisible = false;
        }
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
          this.router.navigate(['/profile']);
        }
      },
      {
        label: 'Settings',
        icon: 'pi pi-cog',
        command: () => {
          this.router.navigate(['/settings']);
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
    // Defer navigation until after overlay panel closes to avoid interference
    setTimeout(() => this.navigateFromNotification(notification), 150);
  }

  getNotificationIcon(notification: Notification): string {
    return getNotificationIcon(notification);
  }

  /** Navigate to the appropriate page based on notification (RedirectUrl preferred, else referenceType/referenceId). */
  private navigateFromNotification(notification: Notification): void {
    const url = notification.redirectUrl?.trim();
    if (url) {
      this.router.navigateByUrl(url.startsWith('/') ? url : `/${url}`);
      return;
    }
    const refType = (notification.referenceType ?? 'Order').toLowerCase();
    const refId = notification.referenceId ?? notification.orderId;
    const orderId = notification.orderId ?? (refType === 'order' ? refId : null);
    switch (refType) {
      case 'order':
        if (refId) this.router.navigate(['/orders', refId]);
        break;
      case 'invoice':
        if (refId) this.router.navigate(['/invoices', refId]);
        break;
      case 'message':
        if (orderId) this.router.navigate(['/orders', orderId]);
        else if (refId) this.router.navigate(['/messages']);
        break;
      case 'system':
        this.router.navigate(['/users']);
        break;
      case 'quote':
        if (refId) this.router.navigate(['/quotes', refId]);
        else this.router.navigate(['/quotes']);
        break;
      default:
        if (orderId || refId) this.router.navigate(['/orders', orderId ?? refId]);
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

    // Base menu items - Dashboard
    this.menuItems = [
      { label: 'Dashboard', icon: 'pi pi-home', routerLink: '/dashboard', routerLinkActiveOptions: { exact: false } }
    ];

    if (userRole === 'SuperAdmin' || userRole === 'Admin') {
      this.menuItems.push(
        { label: 'Operations', styleClass: 'menu-section-header' },
        { label: 'Orders', icon: 'pi pi-shopping-cart', routerLink: '/orders' },
        { label: 'Quotes', icon: 'pi pi-file', routerLink: '/quotes' },
        { label: 'Users', icon: 'pi pi-users', routerLink: '/users', badge: userRole === 'SuperAdmin' ? 'Admin' : undefined },
        { label: 'Clients', icon: 'pi pi-user', routerLink: '/clients' },
        { label: 'Projects', icon: 'pi pi-palette', routerLink: '/projects' },
        { label: 'Designers', icon: 'pi pi-user-edit', routerLink: '/designers' },
        { label: 'Messages', icon: 'pi pi-inbox', routerLink: '/messages' },
        { label: 'Reviews', icon: 'pi pi-star', routerLink: '/reviews' },
        { label: 'Files', icon: 'pi pi-file', routerLink: '/files' },
        { label: 'Analytics', styleClass: 'menu-section-header' },
        { label: 'Detail Analytics', icon: 'pi pi-chart-bar', routerLink: '/analytics' },
        { label: 'Client Intelligence', icon: 'pi pi-chart-pie', routerLink: '/client-intelligence' },
        { label: 'Financial', styleClass: 'menu-section-header' },
        { label: 'Invoices', icon: 'pi pi-money-bill', routerLink: '/invoices' },
        { label: 'Financial Overview', icon: 'pi pi-wallet', routerLink: '/financial', routerLinkActiveOptions: { exact: true } },
        { label: 'Designer Payout', icon: 'pi pi-money-bill', routerLink: '/financial/designer-payout' },
        { label: 'Client Pricing', icon: 'pi pi-tag', routerLink: '/client-pricing' },
        { label: 'Designer Pricing', icon: 'pi pi-tag', routerLink: '/designer-pricing' }
      );
    }

    if (userRole === 'SuperAdmin') {
      this.menuItems.push(
        { label: 'Administration', styleClass: 'menu-section-header' },
        { label: 'Permissions', icon: 'pi pi-key', routerLink: '/permissions', badge: 'Admin' }
      );
    }

    if (userRole === 'Client') {
      this.menuItems.push(
        { label: 'Operations', styleClass: 'menu-section-header' },
        { label: 'My Orders', icon: 'pi pi-shopping-cart', routerLink: '/orders' },
        { label: 'My Quotes', icon: 'pi pi-file', routerLink: '/quotes' },
        { label: 'My Projects', icon: 'pi pi-palette', routerLink: '/projects' },
        { label: 'Messages', icon: 'pi pi-inbox', routerLink: '/messages' },
        { label: 'Files', icon: 'pi pi-file', routerLink: '/files' },
        { label: 'Analytics', styleClass: 'menu-section-header' },
        { label: 'Detail Analytics', icon: 'pi pi-chart-bar', routerLink: '/analytics' },
        { label: 'Financial', styleClass: 'menu-section-header' },
        { label: 'Invoices', icon: 'pi pi-money-bill', routerLink: '/invoices' },
        { label: 'Financial Overview', icon: 'pi pi-wallet', routerLink: '/financial', routerLinkActiveOptions: { exact: true } }
      );
    }

    if (userRole === 'Designer') {
      this.menuItems.push(
        { label: 'Operations', styleClass: 'menu-section-header' },
        { label: 'Orders', icon: 'pi pi-shopping-cart', routerLink: '/orders' },
        { label: 'My Projects', icon: 'pi pi-palette', routerLink: '/projects' },
        { label: 'Messages', icon: 'pi pi-inbox', routerLink: '/messages' },
        { label: 'Files', icon: 'pi pi-file', routerLink: '/files' },
        { label: 'Financial', styleClass: 'menu-section-header' },
        { label: 'Designer Payout', icon: 'pi pi-money-bill', routerLink: '/financial/designer-payout' }
      );
    }

    this.menuItems.push(
      { label: 'Notifications', icon: 'pi pi-bell', routerLink: '/notifications', routerLinkActiveOptions: { exact: true } },
      { label: 'Settings', icon: 'pi pi-cog', routerLink: '/settings' }
    );
    
  }
}
