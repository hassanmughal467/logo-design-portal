import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { of } from 'rxjs';
import { AppComponent } from './app.component';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';
import { NotificationService } from '@core/services/notification.service';
import { MessageService } from 'primeng/api';

describe('AppComponent', () => {
  let router: jasmine.SpyObj<Router>;
  let messageService: jasmine.SpyObj<MessageService>;
  let notificationService: jasmine.SpyObj<NotificationService>;

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigateByUrl']);
    messageService = jasmine.createSpyObj<MessageService>('MessageService', ['clear']);
    notificationService = jasmine.createSpyObj<NotificationService>('NotificationService', [
      'markAsRead',
      'refreshNotifications'
    ]);
    notificationService.markAsRead.and.returnValue(of({}));

    TestBed.configureTestingModule({
      declarations: [AppComponent],
      providers: [
        { provide: RealtimeNotificationService, useValue: {} },
        { provide: NotificationService, useValue: notificationService },
        { provide: Router, useValue: router },
        { provide: MessageService, useValue: messageService }
      ],
      schemas: [NO_ERRORS_SCHEMA]
    }).compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('onToastClick navigates when redirectUrl set', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.componentInstance.onToastClick({ data: { redirectUrl: '/orders' } });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/orders');
    expect(messageService.clear).toHaveBeenCalled();
  });

  it('onToastClick marks notification as read when notificationId set', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.componentInstance.onToastClick({
      data: { redirectUrl: '/orders/abc', notificationId: 'notif-1' }
    });
    expect(notificationService.markAsRead).toHaveBeenCalledWith('notif-1');
    expect(notificationService.refreshNotifications).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/orders/abc');
  });

  it('onToastClick prefixes relative path', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.componentInstance.onToastClick({ data: { redirectUrl: 'invoices' } });
    expect(router.navigateByUrl).toHaveBeenCalledWith('/invoices');
  });

  it('onToastClick skips when no url', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.componentInstance.onToastClick({});
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });
});
