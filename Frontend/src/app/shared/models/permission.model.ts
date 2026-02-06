export interface Permission {
  id: string;
  name: string;
  description: string;
  resource: string;
  action: string;
}

export interface RolePermission {
  roleId: string;
  permissionId: string;
  permission: Permission;
}

export interface PermissionMatrix {
  roleId: string;
  roleName: string;
  permissions: Permission[];
}
