# HG Load Testing where this mimics a CORS pre-flight options call.

## Running (macos or linux)

```bash
HG_ENV=test HG_KEY=apikey docker compose up
```

### Options

HG_KEY is the Akamai Queue-IT api key to bypass the queue.
HG_ENV is a choice of dev, test   (for production use the URL setting)
HG_URL overrides the default url that is build from the environment setting. Use this for testin

### Example CLI call:
```bash
HG_URL="https://dev.healthgateway.app.nomostech.co/" HG_TYPE=smoke docker compose up
```