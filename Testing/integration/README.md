# Integration tests

Run the suite with Docker running and the configured test authentication credentials available:

```sh
ASPNETCORE_ENVIRONMENT=Development dotnet test Testing/integration/integration.sln
```

The API hosts run inside the tests; do not start Gateway API, Admin API, or JobScheduler separately. External authentication and patient services still need to be reachable.

## Isolation and lifecycle

API scenario tests share the `WebAppScenario` collection fixture, PostgreSQL database, and Redis container. Each test creates and disposes its own API host. Host-specific service registrations are supplied through `ConfigureServices`; they are never retained on the fixture.

The three messaging tests stay in the same sequential collection but each owns a separate `WebAppFixture`, including its own PostgreSQL and Redis containers. Only their hosts register a Hangfire worker. No API-test outbox rows can be dispatched by these workers. The serialization test needs no host or containers.

Each messaging test generates two full GUID session IDs. A test-only Azure client factory restricts the actual Service Bus session processor to those IDs. Messages from other tests are not received or acknowledged. The production sender, outbox dispatcher, and session fault handler remain under test.

The configured `PhsaV2:ServiceBus:QueueName` must be a session-enabled queue reserved for integration tests. The current Send/Listen credentials are sufficient; no queue creation, deletion, or purge is performed. Do not run an unrestricted application consumer against that test queue, since it could take messages belonging to a test. Failed-session messages retain the existing dead-letter behavior, and every test uses new session IDs so persisted fault state cannot affect later tests.

`SECRETS_PATH`, when set, is loaded before the test overrides. Database, Redis, and messaging outbox settings take precedence over secrets so tests cannot accidentally use an external database or bypass the transactional outbox.

## Validation

Run the entire suite more than once, as well as the messaging and dependent classes separately. There is no required execution order. The session-failure test waits for both failed-session messages to be rejected. The rollback test checks database rollback and sends a committed control message before checking that rolled-back messages never arrive during its observation window.
