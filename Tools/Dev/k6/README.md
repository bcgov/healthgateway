# k6 Load Testing Tool

k6 is a modern load testing tool, building on Load Impact's years of experience in the load and performance testing industry. It provides a clean, approachable scripting API, local and cloud execution, and flexible configuration.

## Install

```bash
docker pull grafana/k6`
```

### See also

[k6 Getting Started](https://k6.io/docs/getting-started/installation)

## Usage

Once installed in docker, you can run an ES6 JavaScript load test by executing this way:

```code
docker run -v <local_path>:<in_docker_path> -a STDOUT -a STDERR -i grafana/k6 run -e <env_variable> <k6_script>.js
```

### See also

[Running k6](https://k6.io/docs/getting-started/running-k6)
