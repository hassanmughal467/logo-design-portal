import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DesignerListComponent } from './designer-list.component';

const routes: Routes = [
  {
    path: '',
    component: DesignerListComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DesignerListRoutingModule { }
