import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { MessageService } from 'primeng/api';
import { RealtimeNotificationService } from './realtime-notification.service';
import { AuthService } from './auth.service';
import { NotificationService } from './notification.service';
import { RealtimeStatusService } from './realtime-status.service';
import { User } from '@shared/models/user.model';

describe('RealtimeNotificationService', () => {
  let configureLoggingSpy: jasmine.Spy;

  const fakeUser: User = {
    id: 'user-1',
    email: 'test@example.com',
    firstName: 'Test',
    lastName: 'User',
    isActive: true,
    createdAt: new Date()
  };

  beforeEach(() => {
    const fakeConnection = {
      on: jasmine.createSpy('on'),
      onreconnecting: jasmine.createSpy('onreconnecting'),
      onreconnected: jasmine.createSpy('onreconnected'),
      onclose: jasmine.createSpy('onclose'),
      start: jasmine.createSpy('start').and.returnValue(Promise.resolve()),
      stop: jasmine.createSpy('stop').and.returnValue(Promise.resolve()),
      invoke: jasmine.createSpy('invoke').and.returnValue(Promise.resolve()),
      state: signalR.HubConnectionState.Connected
    };

    // Spy on the real prototype methods so the fluent builder chain still works,
    // but replace `build()` so no real HubConnection/HttpConnection (and therefore
    // no real network I/O) is ever created during this unit test.
    configureLoggingSpy = spyOn(signalR.HubConnectionBuilder.prototype, 'configureLogging').and.callThrough();
    spyOn(signalR.HubConnectionBuilder.prototype, 'build')
      .and.returnValue(fakeConnection as unknown as signalR.HubConnection);

    const authMock = {
      currentUser$: of(fakeUser),
      getAccessToken: jasmine.createSpy('getAccessToken').and.returnValue('fake-test-token')
    };
    const notificationMock = jasmine.createSpyObj('NotificationService', ['onRealtimeNotificationReceived']);
    const messageServiceMock = jasmine.createSpyObj<MessageService>('MessageService', ['add']);

    TestBed.configureTestingModule({
      providers: [
        RealtimeNotificationService,
        { provide: AuthService, useValue: authMock },
        { provide: NotificationService, useValue: notificationMock },
        RealtimeStatusService,
        { provide: MessageService, useValue: messageServiceMock }
      ]
    });
  });

  it('configures SignalR logging at Warning level so token-bearing connection URLs are not logged to the console', () => {
    // Instantiating the service triggers currentUser$'s synchronous emission, which
    // calls connect() and builds the HubConnectionBuilder chain under test.
    TestBed.inject(RealtimeNotificationService);

    expect(configureLoggingSpy).toHaveBeenCalledWith(signalR.LogLevel.Warning);
  });
});
