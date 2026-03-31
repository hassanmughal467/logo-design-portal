import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { DashboardService } from './dashboard.service';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { SharedListDataService } from './shared-list-data.service';

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
