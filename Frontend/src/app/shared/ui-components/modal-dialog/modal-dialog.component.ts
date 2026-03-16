import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-modal-dialog',
  templateUrl: './modal-dialog.component.html',
  styleUrls: ['./modal-dialog.component.scss']
})
export class ModalDialogComponent {
  @Input() visible = false;
  @Input() header = '';
  @Input() width = '500px';
  @Input() maxWidth = '90vw';
  @Output() visibleChange = new EventEmitter<boolean>();
}
