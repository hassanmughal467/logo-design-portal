import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ClientRetentionStats } from '@core/services/client-churn-analytics.service';

@Component({
  selector: 'app-client-retention-chart',
  templateUrl: './client-retention-chart.component.html',
  styleUrls: ['./client-retention-chart.component.scss']
})
export class ClientRetentionChartComponent implements OnChanges {
  @Input() retentionStats: ClientRetentionStats | null = null;
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['retentionStats'] && this.retentionStats?.retentionTrend?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const items = this.retentionStats!.retentionTrend;
    const categories = items.map(i => i.month);
    const retentionData = items.map(i => i.retentionRate);

    this.chartOptions = {
      series: [{ name: 'Retention Rate %', data: retentionData }],
      chart: {
        type: 'area',
        height: 320,
        toolbar: { show: false },
        zoom: { enabled: false }
      },
      stroke: { curve: 'smooth', width: 2 },
      fill: { type: 'gradient', gradient: { shadeIntensity: 1, opacityFrom: 0.4, opacityTo: 0.1 } },
      colors: ['#6366f1'],
      xaxis: { categories },
      yaxis: {
        labels: { formatter: (v: number) => v + '%' },
        min: 0,
        max: 100,
        tickAmount: 5
      },
      tooltip: {
        y: { formatter: (v: number) => v + '%' }
      },
      dataLabels: { enabled: false }
    };
  }
}
