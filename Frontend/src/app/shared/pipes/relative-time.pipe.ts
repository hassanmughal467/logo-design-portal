import { Pipe, PipeTransform } from '@angular/core';
import { formatRelativeTime } from '../utils/notification-helpers';

@Pipe({
  name: 'relativeTime'
})
export class RelativeTimePipe implements PipeTransform {
  transform(value: Date | string | null | undefined): string {
    if (!value) return '';
    return formatRelativeTime(value);
  }
}
