import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-client-lifetime-value-chart',
  templateUrl: './client-lifetime-value-chart.component.html',
  styleUrls: ['./client-lifetime-value-chart.component.scss']
})
export class ClientLifetimeValueChartComponent implements OnChanges {
  @Input() items: any[] = [];
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const displayItems = this.items.slice(0, 8);
    const labels = displayItems.map(i => (i.clientName?.length > 25 ? i.clientName.substring(0, 25) + '...' : i.clientName) || 'Unknown');
    const revenue = displayItems.map(i => i.lifetimeRevenue);
    this.chartOptions = {
      series: revenue,
      chart: { type: 'donut', height: 280 },
      labels,
      colors: ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4', '#84cc16'],
      legend: { position: 'bottom', horizontalAlign: 'center' },
      dataLabels: { enabled: true, formatter: (v: number) => v.toFixed(1) + '%' },
      tooltip: {
        y: { formatter: (v: number) => '$' + v.toLocaleString() }
      }
    };
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(value);
  }
}
