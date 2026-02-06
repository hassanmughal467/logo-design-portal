import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProjectsRoutingModule } from './projects-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { ProjectListComponent } from './project-list/project-list.component';
import { ProjectDetailComponent } from './project-detail/project-detail.component';
import { ProjectListModule } from './project-list/project-list.module';

@NgModule({
  declarations: [ProjectDetailComponent],
  imports: [
    CommonModule,
    ProjectsRoutingModule,
    LayoutModule,
    SharedModule,
    ProjectListModule
  ]
})
export class ProjectsModule { }
