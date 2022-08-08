# Service Testing of various test types.

## Running (macos or linux)

```bash
HG_ENV=test HG_KEY=apikey HG_PASSWORD=pwd HG_TYPE=load docker compose up
```

### Options

HG_KEY is the Akamai Queue-IT api key to bypass the queue.
HG_ENV is a choice of dev, test, prod
HG_TYPE is a choice of load, smoke, soak, spike, and stress
HG_PASSWORD is the password for the loadtest_0x keycloak accounts, where x is from 1 to 15.
HG_URL overrides the default url that is build from the environment setting. HG_ENV has no effect when this is used.