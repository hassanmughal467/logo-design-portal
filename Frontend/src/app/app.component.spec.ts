import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { NO_ERRORS_SCHEMA } from '@angular/core';
import { AppComponent } from './app.component';
import { RealtimeNotificationService } from '@core/services/realtime-notification.service';
import { MessageService } from 'primeng/api';

describe('AppComponent', () => {
  let router: jasmine.SpyObj<Router>;
  let messageService: jasmine.SpyObj<MessageService>;

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigateByUrl']);
    messageService = jasmine.createSpyObj<MessageService>('MessageService', ['clear']);

    TestBed.configureTestingModule({
      declarations: [AppComponent],
      providers: [
        { provide: RealtimeNotificationService, useValue: {} },
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
