# HG Load Testing where this mimics a user logging in a fetchign the timeline.

## Running (macos or linux)

```bash
HG_ENV=test HG_KEY=apikey HG_PASSWORD=pwd docker compose up
```

### Options

HG_KEY is the Akamai Queue-IT api key to bypass the queue.
HG_ENV is a choice of dev, test, prod
HG_PASSWORD is the password for the loadtest_0x keycloak accounts, where x is from 1 to 15.