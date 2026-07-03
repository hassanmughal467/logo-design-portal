import { ClientActivityTableComponent } from './client-activity-table.component';

describe('ClientActivityTableComponent', () => {
  let component: ClientActivityTableComponent;

  beforeEach(() => {
    component = new ClientActivityTableComponent();
  });

  it('maps activity statuses to labels and severities', () => {
    expect(component.getStatusLabel('Active')).toBe('Active');
    expect(component.getStatusSeverity('Active')).toBe('success');
    expect(component.getStatusSeverity('LowActivity')).toBe('warning');
    expect(component.getStatusSeverity('Inactive')).toBe('danger');
    expect(component.getStatusSeverity('Unknown')).toBe('info');
  });

  it('formats dates and handles missing values', () => {
    expect(component.formatDate(undefined)).toBe('-');
    expect(component.formatDate('2024-06-01T00:00:00Z')).toContain('2024');
  });
});
