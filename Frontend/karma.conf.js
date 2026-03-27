module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma')
    ],
    client: {
      jasmine: {
        random: true
      },
      clearContext: false
    },
    jasmineHtmlReporter: {
      suppressAll: true
    },
    coverageReporter: {
      dir: require('path').join(__dirname, './coverage/logo-design-portal-frontend'),
      subdir: '.',
      reporters: [
        { type: 'html' },
        { type: 'text-summary' },
        { type: 'lcovonly' }
      ],
      // Watermarks: raise toward 60%+ line coverage as specs grow (CI enforces these minimums).
      check: {
        global: {
          statements: 65,
          branches: 55,
          functions: 65,
          lines: 65
        }
      }
    },
    reporters: ['progress', 'kjhtml', 'coverage'],
    port: 9876,
    colors: true,
    logLevel: config.LOG_INFO,
    autoWatch: false,
    browsers: ['ChromeHeadlessCI'],
    customLaunchers: {
      // ChromeHeadlessCI: stable defaults for GitHub Actions / Docker (no sandbox).
      ChromeHeadlessCI: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox', '--disable-gpu', '--window-size=1280,720']
      }
    },
    singleRun: true,
    restartOnFileChange: false
  });
};
