/**
 * Frontend mutation testing baseline.
 * Start focused and expand mutate patterns as score stabilizes.
 */
module.exports = {
  testRunner: 'jasmine',
  mutate: [
    'src/app/**/*.ts',
    '!src/**/*.spec.ts',
    '!src/main.ts',
    '!src/test.ts',
    '!src/environments/**/*.ts',
    '!src/app/**/financial/**/*.ts',
    '!src/app/**/analytics/**/*.ts'
  ],
  reporters: ['html', 'progress', 'clear-text'],
  htmlReporter: {
    fileName: 'reports/mutation/frontend-mutation-report.html'
  },
  coverageAnalysis: 'perTest',
  karma: {
    configFile: 'karma.conf.js'
  },
  thresholds: {
    high: 80,
    low: 65,
    break: 60
  }
};
