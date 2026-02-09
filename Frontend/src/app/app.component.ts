import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>
    <p-toast></p-toast>
  `,
  styles: []
})
export class AppComponent {
  title = 'Hawk Merchandising Web Portal';
}
