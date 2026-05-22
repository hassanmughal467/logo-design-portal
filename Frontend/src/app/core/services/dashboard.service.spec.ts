import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { DashboardService } from './dashboard.service';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { SharedListDataService } from './shared-list-data.service';
import { OrderStatus } from '@shared/models/order.model';

describe('DashboardService', () => {
  let service: DashboardService;
  let apiService: jasmine.SpyObj<ApiService>;
  let authService: jasmine.SpyObj<AuthService>;
  let notificationService: jasmine.SpyObj<NotificationService>;
  let sharedListData: jasmine.SpyObj<SharedListDataService>;

  beforeEach(() => {
    apiService = jasmine.createSpyObj('ApiService', ['get']);
    authService = jasmine.createSpyObj('AuthService', ['getCurrentUser']);
    notificationService = jasmine.createSpyObj('NotificationService', ['getNotifications']);
    sharedListData = jasmine.createSpyObj('SharedListDataService', ['getAllUsers', 'getAllOrdersAdmin', 'clearAll']);
    sharedListData.getAllUsers.and.returnValue(of([]));
    sharedListData.getAllOrdersAdmin.and.returnValue(of([]));

    TestBed.configureTestingModule({
      providers: [
        DashboardService,
        { provide: ApiService, useValue: apiService },
        { provide: AuthService, useValue: authService },
        { provide: NotificationService, useValue: notificationService },
        { provide: SharedListDataService, useValue: sharedListData }
      ]
    });

    service = TestBed.inject(DashboardService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getDashboardData returns empty structure when no user', (done) => {
    authService.getCurrentUser.and.returnValue(null);
    service.getDashboardData().subscribe((data) => {
      expect(data.stats.totalOrders).toBe(0);
      expect(data.recentOrders).toEqual([]);
      done();
    });
  });

  it('getDashboardData loads my-orders for Client role', (done) => {
    authService.getCurrentUser.and.returnValue({
      id: '1',
      email: 'c@test.com',
      firstName: 'C',
      lastName: 'L',
      role: 'Client'
    } as any);
    apiService.get.and.returnValue(of([]));
    notificationService.getNotifications.and.returnValue(of([]));

    service.getDashboardData().subscribe(() => {
      expect(apiService.get).toHaveBeenCalledWith('orders/my-orders');
      done();
    });
  });

  it('still counts orders when gallery request fails for Client', (done) => {
    authService.getCurrentUser.and.returnValue({
      id: '1',
      email: 'c@test.com',
      firstName: 'C',
      lastName: 'L',
      role: 'Client'
    } as any);
    const sampleOrder = [
      {
        id: 'order-1',
        title: 'Test',
        status: OrderStatus.WaitingForAdminApproval,
        createdAt: new Date().toISOString()
      }
    ];
    apiService.get.withArgs('orders/my-orders').and.returnValue(of(sampleOrder));
    apiService.get.withArgs('gallery/my-gallery').and.returnValue(throwError(() => new Error('gallery down')));
    apiService.get.and.returnValue(of([]));
    notificationService.getNotifications.and.returnValue(of([]));

    service.getDashboardData().subscribe((data) => {
      expect(data.stats.totalOrders).toBe(1);
      expect(data.stats.activeOrders).toBe(1);
      done();
    });
  });

  it('counts WaitingForAdminApproval as active for Client dashboard stats', (done) => {
    authService.getCurrentUser.and.returnValue({
      id: '1',
      email: 'c@test.com',
      firstName: 'C',
      lastName: 'L',
      role: 'Client'
    } as any);
    apiService.get.withArgs('orders/my-orders').and.returnValue(
      of([
        {
          id: 'order-1',
          title: 'Test',
          status: OrderStatus.WaitingForAdminApproval,
          createdAt: new Date().toISOString()
        }
      ])
    );
    apiService.get.and.returnValue(of([]));
    notificationService.getNotifications.and.returnValue(of([]));

    service.getDashboardData().subscribe((data) => {
      expect(data.stats.totalOrders).toBe(1);
      expect(data.stats.activeOrders).toBe(1);
      expect(data.stats.pendingOrders).toBe(1);
      expect(data.recentOrders.length).toBe(1);
      done();
    });
  });

  it('getDashboardData resolves GBP revenue currency for SuperAdmin completed orders', (done) => {
    authService.getCurrentUser.and.returnValue({
      id: '1',
      email: 'a@test.com',
      firstName: 'A',
      lastName: 'D',
      role: 'SuperAdmin'
    } as any);
    sharedListData.getAllOrdersAdmin.and.returnValue(
      of([
        {
          Id: 'o1',
          Title: 'L1',
          Status: 'Completed',
          CurrencyCode: 'GBP',
          ClientChargePrice: 3,
          CreatedAt: new Date().toISOString()
        },
        {
          Id: 'o2',
          Title: 'L2',
          Status: 'Completed',
          CurrencyCode: 'GBP',
          ClientChargePrice: 6,
          CreatedAt: new Date().toISOString()
        }
      ])
    );
    apiService.get.and.returnValue(of([]));
    notificationService.getNotifications.and.returnValue(of([]));

    service.getDashboardData().subscribe((data) => {
      expect(data.revenueCurrencyCode).toBe('GBP');
      expect(data.stats.totalRevenue).toBe(9);
      done();
    });
  });

  it('getDashboardData loads assigned-orders for Designer role', (done) => {
    authService.getCurrentUser.and.returnValue({
      id: '1',
      email: 'd@test.com',
      firstName: 'D',
      lastName: 'S',
      role: 'Designer'
    } as any);
    apiService.get.and.returnValue(of([]));

    service.getDashboardData().subscribe(() => {
      expect(apiService.get).toHaveBeenCalledWith('orders/assigned-orders');
      done();
    });
  });
});
