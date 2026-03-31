import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { DesignerRevenueItem } from '@core/services/financial-analytics.service';

@Component({
  selector: 'app-designer-revenue-chart',
  templateUrl: './designer-revenue-chart.component.html',
  styleUrls: ['./designer-revenue-chart.component.scss']
})
export class DesignerRevenueChartComponent implements OnChanges {
  @Input() items: DesignerRevenueItem[] = [];

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['items'] && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const labels = this.items.map(i => (i.designerName.length > 25 ? i.designerName.substring(0, 25) + '...' : i.designerName));
    this.chartOptions = {
      series: [{ name: 'Revenue', data: this.items.map(i => i.totalRevenue) }],
      chart: { type: 'bar', height: Math.max(280, this.items.length * 36), toolbar: { show: false } },
      plotOptions: { bar: { borderRadius: 6, horizontal: true, barHeight: '70%' } },
      colors: ['#10b981'],
      xaxis: { categories: labels, labels: { formatter: (v: number) => '$' + v } },
      yaxis: { labels: { maxWidth: 150 } },
      dataLabels: { enabled: true, formatter: (v: number) => '$' + (v >= 1000 ? (v / 1000) + 'k' : v) },
      tooltip: { y: { formatter: (v: number) => '$' + v.toLocaleString() } }
    };
  }
}
