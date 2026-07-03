import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { RevenueForecastItem } from '@core/services/financial-analytics.service';
import { compactCurrencyLabel, formatCurrencyAmount } from '@core/utils/currency-format';

@Component({
  selector: 'app-revenue-forecast-chart',
  templateUrl: './revenue-forecast-chart.component.html',
  styleUrls: ['./revenue-forecast-chart.component.scss']
})
export class RevenueForecastChartComponent implements OnChanges {
  @Input() items: RevenueForecastItem[] = [];
  @Input() currencyCode = 'USD';
  @Input() currencyMixed = false;

  chartOptions: any = null;

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['items'] || changes['currencyCode'] || changes['currencyMixed']) && this.items?.length) {
      this.buildChart();
    }
  }

  private buildChart(): void {
    const code = this.currencyMixed ? undefined : this.currencyCode;
    const fmt = (v: number) => (v != null ? formatCurrencyAmount(v, code) : '-');
    const axis = (v: number) => compactCurrencyLabel(v, code, this.currencyMixed);
    const actualData = this.items.map(i => i.actual ?? null);
    const predictedData = this.items.map(i => i.predicted ?? null);
    this.chartOptions = {
      series: [
        { name: 'Actual', data: actualData },
        { name: 'Predicted', data: predictedData }
      ],
      chart: { type: 'line', height: 280, toolbar: { show: false } },
      stroke: { curve: 'smooth', width: 2 },
      colors: ['#10b981', '#8b5cf6'],
      xaxis: { categories: this.items.map(i => i.month), labels: { rotate: -45 } },
      yaxis: { min: 0, labels: { formatter: axis } },
      legend: { position: 'top' },
      dataLabels: { enabled: false },
      tooltip: {
        shared: true,
        y: [{ formatter: fmt }, { formatter: fmt }]
      }
    };
  }
}
