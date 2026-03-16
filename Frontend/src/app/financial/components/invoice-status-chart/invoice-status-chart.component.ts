import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { InvoiceStatusItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-invoice-status-chart',
  templateUrl: './invoice-status-chart.component.html',
  styleUrls: ['./invoice-status-chart.component.scss']
})
export class InvoiceStatusChartComponent implements OnChanges {
  @Input() items: InvoiceStatusItem[] = [];
  @Input() loading = false;

  chartOptions: any = null;
  readonly colors: Record<string, string> = {
    Paid: '#10b981',
    Pending: '#f59e0b',
    Overdue: '#ef4444',
    Due: '#64748b',
    Cancelled: '#94a3b8'
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const labels = this.items.map(i => i.status);
    const series = this.items.map(i => i.count);
    const colors = this.items.map(i => this.colors[i.status] || '#94a3b8');
    this.chartOptions = {
      series: series,
      chart: { type: 'donut', height: 280 },
      labels,
      colors,
      legend: { position: 'bottom' },
      dataLabels: { enabled: true },
      plotOptions: { pie: { donut: { size: '65%' } } },
      tooltip: { y: { formatter: (v: number) => v + ' invoices' } }
    };
  }
}
