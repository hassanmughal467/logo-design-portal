import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HasPermissionDirective } from './directives/has-permission.directive';
import { RoleNamePipe } from './pipes/role-name.pipe';
import { RelativeTimePipe } from './pipes/relative-time.pipe';
import { UiComponentsModule } from './ui-components/ui-components.module';

@NgModule({
  declarations: [
    HasPermissionDirective,
    RoleNamePipe,
    RelativeTimePipe
  ],
  imports: [
    CommonModule,
    UiComponentsModule
  ],
  exports: [
    HasPermissionDirective,
    RoleNamePipe,
    RelativeTimePipe,
    UiComponentsModule
  ]
})
export class SharedModule { }
