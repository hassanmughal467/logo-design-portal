import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { PermissionsService } from '@core/services/permissions.service';
import { AuthService } from '@core/services/auth.service';

@Directive({
  selector: '[appHasPermission]'
})
export class HasPermissionDirective {
  private hasView = false;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private permissionsService: PermissionsService,
    private authService: AuthService
  ) {}

  @Input() set appHasPermission(permission: string | string[]) {
    const permissions = Array.isArray(permission) ? permission : [permission];
    const hasPermission = this.permissionsService.hasAnyPermission(permissions);

    if (hasPermission && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!hasPermission && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}
