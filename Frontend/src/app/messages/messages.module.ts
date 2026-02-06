import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MessagesRoutingModule } from './messages-routing.module';
import { LayoutModule } from '../layout/layout.module';
import { SharedModule } from '../shared/shared.module';
import { MessageListModule } from './message-list/message-list.module';

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    MessagesRoutingModule,
    LayoutModule,
    SharedModule,
    MessageListModule
  ]
})
export class MessagesModule { }
