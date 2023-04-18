# Import Data into Redis for offline development:

## When to use

When you are not going to be at your normal workstation and thus at an IP address that has not been approved/whitelisted.

## How to use

### Prerequisites

1. Have `redis-cli` installed globally.
2. Ensure you have redis running (at the time of writing this should be in docker).

### Steps

1. Using terminal (WSL2 on Windows)
2. Navigate to the directory this readme is located in.
3. Execute all `*.redis` files within data folder using the following command:
    - `redis-cli -h localhost -p 6379 < ./data.redis`

### Extension

You can either keep adding new redis-cli commands per line directly to the `data.redis` file, or add new files and repeat step 3 for each file.

## References

| Name                                   | Link                                                                         |
| -------------------------------------- | ---------------------------------------------------------------------------- |
| Developer redis import using redis-cli | https://developer.redis.com/guides/import/#import-using-reds-cli-script      |
| Install redis-cli for windows          | https://redis.io/docs/getting-started/installation/install-redis-on-windows/ |
