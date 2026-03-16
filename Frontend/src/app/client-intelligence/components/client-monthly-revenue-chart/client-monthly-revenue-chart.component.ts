import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-client-monthly-revenue-chart',
  templateUrl: './client-monthly-revenue-chart.component.html',
  styleUrls: ['./client-monthly-revenue-chart.component.scss']
})
export class ClientMonthlyRevenueChartComponent implements OnChanges {
  @Input() items: any[] = [];
  @Input() loading = false;
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    } else if (changes['items'] && !this.items?.length) {
      this.chartOptions = null;
    }
  }

  private buildChart(): void {
    const categories = this.items.map(i => i.month);
    const revenue = this.items.map(i => i.revenue);
    this.chartOptions = {
      series: [{ name: 'Revenue', data: revenue }],
      chart: { type: 'bar', height: 280, toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 4, columnWidth: '70%' } },
      colors: ['#8b5cf6'],
      xaxis: { categories },
      yaxis: { labels: { formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) } },
      tooltip: { y: { formatter: (v: number) => '$' + v.toLocaleString() } },
      dataLabels: { enabled: false }
    };
  }
}
