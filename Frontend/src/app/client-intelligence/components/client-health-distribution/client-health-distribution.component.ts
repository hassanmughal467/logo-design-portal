import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ClientRetentionStats } from '@core/services/client-churn-analytics.service';

@Component({
  selector: 'app-client-health-distribution',
  templateUrl: './client-health-distribution.component.html',
  styleUrls: ['./client-health-distribution.component.scss']
})
export class ClientHealthDistributionComponent implements OnChanges {
  @Input() retentionStats: ClientRetentionStats | null = null;
  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['retentionStats'] && this.retentionStats) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const s = this.retentionStats!;
    const total = s.healthyCount + s.warningCount + s.highRiskCount + s.churnLikelyCount;
    if (total === 0) return;

    this.chartOptions = {
      series: [s.healthyCount, s.warningCount, s.highRiskCount, s.churnLikelyCount],
      chart: {
        type: 'donut',
        height: 320
      },
      labels: ['Healthy', 'Warning', 'High Risk', 'Churn Likely'],
      colors: ['#10b981', '#f59e0b', '#ef4444', '#8b5cf6'],
      legend: { position: 'bottom' },
      plotOptions: {
        pie: {
          donut: {
            size: '65%',
            labels: {
              show: true,
              total: {
                show: true,
                label: 'Total Clients',
                formatter: () => String(total)
              }
            }
          }
        }
      },
      dataLabels: { enabled: true },
      tooltip: {
        y: {
          formatter: (v: number) => {
            const pct = total > 0 ? ((v / total) * 100).toFixed(1) : '0';
            return `${v} (${pct}%)`;
          }
        }
      }
    };
  }
}
