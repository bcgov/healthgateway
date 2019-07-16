Selenium OpenShift Templates
===

> OpenShift Templates used for a Scalable Selenium infrastructure

Usage
===

Creates the selenium-hub DeploymentConfig, Route and Service.

```bash
$ oc process -f selenium-hub.yaml | oc apply -f -
```

Creates the chrome-node BuildConfig, DeploymentConfig and Service.

```bash
$ oc process -f selenium-node-chrome.yaml | oc apply -f -
```

Creates the firefox-node BuildConfig, DeploymentConfig and Service.

```bash
$ oc process -f selenium-node-firefox.yaml | oc apply -f -
```
