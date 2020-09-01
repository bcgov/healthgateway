# K6 HealthGateway Tests

## Smoke Testing

The Smoke Test's role is to verify that your System can handle minimal load, without any problems. Our smoke tests uses by default 30 virtual users and executes each API call once to ensure it returns a 200 OK.

Any errors here are an indication of functionality not working under basic load.

### When to run the smoke test?

Run this test often, after each system change/release.  This ensures that functionality has not broken under basic loads.

## Load Testing

Load testing is primarily concerned with assessing the systems performance, the purpose of stress testing is to assess the availability and stability of the system under heavy load.

## Stress Testing

Stress Testing is a type of load testing used to determine the limits of the system. The purpose of this test is to verify the stability and reliability of the system under extreme conditions.

## Spike Testing

Spike testing is a type of stress testing that immediately overwhelms the system with an extreme surge of load.

## Soak Testing

While load testing is primarily concerned with performance assessment, and stress testing is concerned with system stability under extreme conditions, soak testing is concerned with reliability over a long time. The soak test uncovers performance and reliability issues stemming from a system being under pressure for an extended period.

### When to run the soak test?

Soak testing helps uncover bugs and reliability issues that surface when a system is loaded over an extended period of time. Soak testing helps reliability. Run this test only once you have successfully run other tests.
