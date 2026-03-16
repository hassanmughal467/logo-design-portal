import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-secondary-button',
  templateUrl: './secondary-button.component.html',
  styleUrls: ['./secondary-button.component.scss']
})
export class SecondaryButtonComponent {
  @Input() label = '';
  @Input() icon = '';
  @Input() iconPos: 'left' | 'right' = 'left';
  @Input() loading = false;
  @Input() disabled = false;
  @Output() clicked = new EventEmitter<void>();
}
