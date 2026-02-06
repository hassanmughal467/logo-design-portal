import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DesignersRoutingModule } from './designers-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    DesignersRoutingModule,
    LayoutModule,
    SharedModule
  ]
})
export class DesignersModule { }
