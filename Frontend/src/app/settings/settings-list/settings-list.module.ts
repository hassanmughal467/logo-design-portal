import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SettingsListComponent } from './settings-list.component';

// PrimeNG
import { TabViewModule } from 'primeng/tabview';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { TagModule } from 'primeng/tag';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TooltipModule } from 'primeng/tooltip';
import { PasswordModule } from 'primeng/password';

@NgModule({
  declarations: [SettingsListComponent],
  imports: [
    CommonModule,
    FormsModule,
    TabViewModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputTextareaModule,
    CheckboxModule,
    DropdownModule,
    DialogModule,
    TagModule,
    InputSwitchModule,
    TooltipModule,
    PasswordModule
  ],
  exports: [SettingsListComponent]
})
export class SettingsListModule { }
