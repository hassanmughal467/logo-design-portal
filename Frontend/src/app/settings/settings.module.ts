import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SettingsRoutingModule } from './settings-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { SettingsListModule } from './settings-list/settings-list.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    SettingsRoutingModule,
    LayoutModule,
    SharedModule,
    SettingsListModule
  ]
})
export class SettingsModule { }
