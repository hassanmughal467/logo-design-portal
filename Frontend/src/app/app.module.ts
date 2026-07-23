import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// PrimeNG
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

// Core
import { AuthService } from './core/services/auth.service';
import { TokenInterceptor } from './core/interceptors/token.interceptor';
import { HttpRequestTrackerInterceptor } from './core/interceptors/http-request-tracker.interceptor';
import { HttpLoadingErrorInterceptor } from './core/interceptors/http-loading-error.interceptor';

// Components
import { AppComponent } from './app.component';
import { AppRoutingModule } from './app-routing.module';
import { LayoutModule } from './layout/layout.module';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,
    LayoutModule,
    ToastModule
  ],
  providers: [
    MessageService,
    // Proactively restores a session (in-memory access token) that survived a reload before the
    // app renders, so route guards/resolvers/components don't fan out token-less requests that
    // each 401 and rely on TokenInterceptor's reactive refresh (see AuthService.initializeSession).
    {
      provide: APP_INITIALIZER,
      useFactory: (authService: AuthService) => () => authService.initializeSession(),
      deps: [AuthService],
      multi: true
    },
    // HTTP_INTERCEPTORS run request-side in registration order, but response/error
    // propagation unwinds in REVERSE order — the last-registered interceptor sees a
    // response error first. TokenInterceptor must be registered last so it gets the
    // chance to silently refresh on a 401 before HttpLoadingErrorInterceptor logs the
    // user out (see fix/silent-refresh-interceptor-order).
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpRequestTrackerInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: HttpLoadingErrorInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: TokenInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
