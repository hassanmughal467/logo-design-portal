import { Pipe, PipeTransform } from '@angular/core';
import { UserRole } from '../models/user.model';

@Pipe({
  name: 'roleName'
})
export class RoleNamePipe implements PipeTransform {
  transform(value: UserRole | string): string {
    const roleMap: { [key: string]: string } = {
      [UserRole.SuperAdmin]: 'Super Admin',
      [UserRole.Admin]: 'Admin',
      [UserRole.Designer]: 'Designer',
      [UserRole.Client]: 'Client'
    };

    return roleMap[value] || value;
  }
}
