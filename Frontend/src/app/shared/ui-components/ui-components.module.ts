import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { PageHeaderComponent } from './page-header/page-header.component';
import { KpiCardComponent } from './kpi-card/kpi-card.component';
import { CardContainerComponent } from './card-container/card-container.component';
import { StatusBadgeComponent } from './status-badge/status-badge.component';
import { EmptyStateComponent } from './empty-state/empty-state.component';
import { FormSectionComponent } from './form-section/form-section.component';
import { DataTableWrapperComponent } from './data-table-wrapper/data-table-wrapper.component';
import { PrimaryButtonComponent } from './primary-button/primary-button.component';
import { SecondaryButtonComponent } from './secondary-button/secondary-button.component';
import { ModalDialogComponent } from './modal-dialog/modal-dialog.component';
import { SkeletonCardComponent } from './skeleton-card/skeleton-card.component';
import { SkeletonTableComponent } from './skeleton-table/skeleton-table.component';
import { SkeletonDashboardComponent } from './skeleton-dashboard/skeleton-dashboard.component';

@NgModule({
  declarations: [
    PageHeaderComponent,
    KpiCardComponent,
    CardContainerComponent,
    StatusBadgeComponent,
    EmptyStateComponent,
    FormSectionComponent,
    DataTableWrapperComponent,
    PrimaryButtonComponent,
    SecondaryButtonComponent,
    ModalDialogComponent,
    SkeletonCardComponent,
    SkeletonTableComponent,
    SkeletonDashboardComponent
  ],
  imports: [CommonModule, ButtonModule, DialogModule],
  exports: [
    PageHeaderComponent,
    KpiCardComponent,
    CardContainerComponent,
    StatusBadgeComponent,
    EmptyStateComponent,
    FormSectionComponent,
    DataTableWrapperComponent,
    PrimaryButtonComponent,
    SecondaryButtonComponent,
    ModalDialogComponent,
    SkeletonCardComponent,
    SkeletonTableComponent,
    SkeletonDashboardComponent
  ]
})
export class UiComponentsModule {}
