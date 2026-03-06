import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HasPermissionDirective } from './directives/has-permission.directive';
import { RoleNamePipe } from './pipes/role-name.pipe';
import { RelativeTimePipe } from './pipes/relative-time.pipe';

@NgModule({
  declarations: [
    HasPermissionDirective,
    RoleNamePipe,
    RelativeTimePipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    HasPermissionDirective,
    RoleNamePipe,
    RelativeTimePipe
  ]
})
export class SharedModule { }
