import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ClientRevenueItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-client-revenue-chart',
  templateUrl: './client-revenue-chart.component.html',
  styleUrls: ['./client-revenue-chart.component.scss']
})
export class ClientRevenueChartComponent implements OnChanges {
  @Input() items: ClientRevenueItem[] = [];
  @Input() loading = false;

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const labels = this.items.map(i => (i.clientName.length > 30 ? i.clientName.substring(0, 30) + '...' : i.clientName));
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.totalRevenue) }],
      chart: { type: 'bar', height: Math.max(280, this.items.length * 36), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#3b82f6'],
      xaxis: { categories: labels, labels: { formatter: (v: number) => '$' + v } },
      yaxis: { labels: { maxWidth: 180 } },
      dataLabels: { enabled: true, formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) },
      tooltip: { y: { formatter: (v: number) => '$' + v.toLocaleString() } }
    };
  }
}
