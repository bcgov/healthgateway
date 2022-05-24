# K6 HealthGateway Tests

k6 is a free and open-source load testing tool written in Go language with tests scripted in JavaScript.

## Virtual Users (vus)

k6 uses the concept of virtual users. When specifying the number of virtual users, they run concurrently over the script. Each iteration of the script represents a single user operating that script. So, if 10 vus ran for 1 minute and each iteration of the script took 10 seconds, then the total number of iterations run would be 10 x 60/10 = 60 iterations completed.

For more information on virtual users, see the k6 documentation: [what are virtual users](https://k6.io/docs/cloud/cloud-faq/what-are-vus-virtual-users).

### Calculation the number of VUS max

```code
VUs = (hourly sessions * average session duration in seconds)/3600
```

> For example, if we expect up to 10,000 users hitting the Healthgateway web client within a given hour, and we expect that the average session duration is 1 minute, then the VUS would be 167.

See [k6 calculation guidance](https://k6.io/docs/cloud/cloud-faq/what-are-vus-virtual-users)

## Smoke Testing

The Smoke Test's role is to verify that your System can handle minimal load, without any problems. Our smoke tests uses by default 30 virtual users and executes each API call once to ensure it returns a 200 OK.

Any errors here are an indication of functionality not working under basic load.

```bash
cd application/smoke
HG_PASSWORD=<loadtestusers_password> smoke.sh
```

### Smoke Testing Individual Services

You can smoke and load test the individual services. To smoke test an individual service use the HG_TEST environment variable set to 'smoke' and use the bash script run.sh that takes the argument of the test JavaScript file.

```bash
cd services/<service>
HG_PASSWORD==<loadtestusers_password> HG_TEST=smoke docker compose up
```

### When to run the smoke test

Run this test often, after each system change/release. This ensures that functionality has not broken under basic loads.

## Load Testing

Load testing is primarily concerned with assessing the systems performance, the purpose of stress testing is to assess the availability and stability of the system under heavy load.

```bash
cd application/load
HG_ENV=test HG_KEY=apikey HG_PASSWORD=pwd docker compose up
```

### Load Testing Individual Services

Load testing each of the individual backend services. Use 

```bash
cd services/<servicname>
HG_ENV=test HG_KEY=apikey HG_TYPE=type HG_PASSWORD=pwd docker compose up
```

## Stress Testing

Stress Testing is a type of load testing used to determine the limits of the system. The purpose of this test is to verify the stability and reliability of the system under extreme conditions.

```bash
cd application/stress
HG_PASSWORD=<loadtestusers_password> docker compose up
```

## Spike Testing

Spike testing is a type of stress testing that immediately overwhelms the system with an extreme surge of load.

```bash
cd application/spike
HG_PASSWORD=<loadtestusers_password> docker compose up
```

## Soak Testing

While load testing is primarily concerned with performance assessment, and stress testing is concerned with system stability under extreme conditions, soak testing is concerned with reliability over a long time. The soak test uncovers performance and reliability issues stemming from a system being under pressure for an extended period.

```bash
cd application/soak
HG_PASSWORD=<loadtestusers_password> docker compose up
```

### When to run the soak test

Soak testing helps uncover bugs and reliability issues that surface when a system is loaded over an extended period of time. Soak testing helps reliability. Run this test only once you have successfully run other tests.
