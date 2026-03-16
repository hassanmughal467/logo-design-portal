import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-client-revenue-trend-chart',
  templateUrl: './client-revenue-trend-chart.component.html',
  styleUrls: ['./client-revenue-trend-chart.component.scss']
})
export class ClientRevenueTrendChartComponent implements OnChanges {
  @Input() items: any[] = [];
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const categories = this.items.map(i => i.month);
    const revenue = this.items.map(i => i.revenue);
    this.chartOptions = {
      series: [{ name: 'Revenue', data: revenue }],
      chart: { type: 'area', height: 320, toolbar: { show: false }, zoom: { enabled: false } },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1 } },
      colors: ['#10b981'],
      xaxis: { categories },
      yaxis: { labels: { formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) } },
      tooltip: {
        y: { formatter: (v: number) => '$' + v.toLocaleString() }
      },
      dataLabels: { enabled: false }
    };
  }
}
