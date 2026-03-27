import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { NotificationService } from './notification.service';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { RealtimeStatusService } from './realtime-status.service';

describe('NotificationService', () => {
  let service: NotificationService;
  let api: jasmine.SpyObj<ApiService>;

  beforeEach(() => {
    api = jasmine.createSpyObj('ApiService', ['get', 'patch']);
    const authMock = {
      currentUser$: of(null),
      getCurrentUser: jasmine.createSpy('getCurrentUser').and.returnValue(null)
    };
    TestBed.configureTestingModule({
      providers: [
        NotificationService,
        { provide: ApiService, useValue: api },
        { provide: AuthService, useValue: authMock },
        RealtimeStatusService
      ]
    });
    service = TestBed.inject(NotificationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getNotifications maps array response', (done) => {
    api.get.and.returnValue(of([{ id: '1', title: 't', message: 'm', isRead: false, createdAt: new Date() }]));
    service.getNotifications(false, 5).subscribe((list) => {
      expect(list.length).toBe(1);
      done();
    });
  });

  it('getNotifications swallows API errors with empty list', (done) => {
    api.get.and.returnValue(throwError(() => new Error('network')));
    service.getNotifications(true).subscribe((list) => {
      expect(list).toEqual([]);
      done();
    });
  });
});
