import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-top-clients-chart',
  templateUrl: './top-clients-chart.component.html',
  styleUrls: ['./top-clients-chart.component.scss']
})
export class TopClientsChartComponent implements OnChanges {
  @Input() items: any[] = [];
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const labels = this.items.map(i => (i.clientName?.length > 35 ? i.clientName.substring(0, 35) + '...' : i.clientName) || 'Unknown');
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.totalRevenue) }],
      chart: { type: 'bar', height: Math.max(320, this.items.length * 40), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '75%' } },
      colors: ['#3b82f6'],
      xaxis: { categories: labels, labels: { formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) } },
      yaxis: { labels: { maxWidth: 200 } },
      dataLabels: { enabled: true, formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) },
      tooltip: {
        y: { formatter: (v: number) => '$' + v.toLocaleString() }
      }
    };
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 0 }).format(value);
  }
}
