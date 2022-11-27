module.exports = {
  defaultCommandTimeout: 30000,
  blockHosts: ['spt.apps.gov.bc.ca'],
  retries: {
    runMode: 1,
    openMode: 0,
  },
  viewportWidth: 1920,
  viewportHeight: 1080,
  reporter: 'mocha-junit-reporter',
  reporterOptions: {
    mochaFile: 'reports/junit/test-results.[hash].xml',
    testsuitesTitle: false,
  },
  env: {
    'bcsc.username': 'hthgtwy11',
    'bcsc.password': '',
    'keycloak.username': 'healthgateway',
    'keycloak.password': '',
    'keycloak.accept.tos.username': 'hthgtwy04',
    'keycloak.accountclosure.username': 'AccountClosure',
    'keycloak.deceased.username': 'hthgtwy19',
    'keycloak.healthgateway12.username': 'healthgateway12',
    'keycloak.hlthgw401.username': 'hlthgw401',
    'keycloak.hthgtwy20.username': 'hthgtwy20',
    'keycloak.laboratory.queued.username': 'hthgtwy09',
    'keycloak.notfound.username': 'hthgtwy03',
    'keycloak.protected.username': 'protected',
    'keycloak.unregistered.username': 'hthgtwy02',
    'idir.username': 'hgateway',
    'idir.password': '',
    phoneNumber: '',
    emailAddress: 'fakeemail@healthgateway.gov.bc.ca',
    phn: '9735353315',
  },
  projectId: 'ofnepc',
  trashAssetsBeforeRuns: true,
  e2e: {
    // We've imported your old cypress plugins here.
    // You may want to clean this up later by importing these.
    setupNodeEvents(on, config) {
      return require('./cypress/plugins/index.js')(on, config)
    },
    baseUrl: 'https://dev.healthgateway.gov.bc.ca',
    specPattern: 'cypress/e2e/**/*.{js,jsx,ts,tsx}',
  },
}
