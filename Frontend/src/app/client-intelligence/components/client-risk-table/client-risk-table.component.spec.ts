import { ClientRiskTableComponent } from './client-risk-table.component';

describe('ClientRiskTableComponent', () => {
  let component: ClientRiskTableComponent;

  beforeEach(() => {
    component = new ClientRiskTableComponent();
  });

  it('maps risk levels to labels and severities', () => {
    expect(component.getRiskLevelLabel('Healthy')).toBe('Healthy');
    expect(component.getRiskSeverity('Healthy')).toBe('success');
    expect(component.getRiskSeverity('Warning')).toBe('warning');
    expect(component.getRiskSeverity('HighRisk')).toBe('danger');
    expect(component.getRiskSeverity('Unknown')).toBe('info');
  });

  it('formats currency and dates', () => {
    expect(component.formatCurrency(10, 'USD')).toContain('10');
    expect(component.formatDate(undefined)).toBe('—');
  });
});
