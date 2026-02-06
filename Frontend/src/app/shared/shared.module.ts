import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HasPermissionDirective } from './directives/has-permission.directive';
import { RoleNamePipe } from './pipes/role-name.pipe';

@NgModule({
  declarations: [
    HasPermissionDirective,
    RoleNamePipe
  ],
  imports: [
    CommonModule
  ],
  exports: [
    HasPermissionDirective,
    RoleNamePipe
  ]
})
export class SharedModule { }
