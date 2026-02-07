import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProjectsRoutingModule } from './projects-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { ProjectListComponent } from './project-list/project-list.component';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { ProjectListModule } from './project-list/project-list.module';

// PrimeNG
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { TabViewModule } from 'primeng/tabview';
import { TooltipModule } from 'primeng/tooltip';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';

@NgModule({
  declarations: [ProjectDetailComponent],
  imports: [
    CommonModule,
    FormsModule,
    ProjectsRoutingModule,
    LayoutModule,
    SharedModule,
    ProjectListModule,
    CardModule,
    ButtonModule,
    TagModule,
    TabViewModule,
    TooltipModule,
    ProgressSpinnerModule,
    InputTextareaModule,
    DialogModule,
    DropdownModule
  ]
})
export class ProjectsModule { }
