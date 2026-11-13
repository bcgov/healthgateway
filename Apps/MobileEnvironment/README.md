# MobileEnvironment

Tells the Health Gateway mobile app which environments a build may use. It's a small nginx container serving one
public endpoint through the API gateway (Kong):

| Environment | URL |
|---|---|
| Production | `https://hg.api.gov.bc.ca/api/mobileservice/environment` |
| Test | `https://hg.test.api.gov.bc.ca/api/mobileservice/environment` |
| Development | `https://hg.dev.api.gov.bc.ca/api/mobileservice/environment` |

```json
{ "releasedBuild": 452 }
```

`releasedBuild` is the last build released to the public on both stores. A release build of the app whose build number
is above it offers Development, Test and Production and starts on Test, so app reviewers and staff can use Test. The
released build and older ones offer Production only. `null` (`RELEASED_BUILD` unset or not a whole number) also means
Production only. The app always asks Production's endpoint; the Development and Test copies exist for testing this app
and the gateway route.

The value lives only on the live deployment, as the `RELEASED_BUILD` environment variable, so changing it needs no
build, Helm deploy or code change. Gold and GoldDR are kept in sync so a failover behaves the same.

## Contents

| Path | What |
|---|---|
| `Dockerfile`, `nginx.conf` | nginx on port 8080: `/api/mobileservice/environment` (`application/json`, cached 60 seconds) and `/health` |
| `released-build.sh` | Runs at container start and writes the response from `RELEASED_BUILD` |
| `test.sh` | Builds the image and checks it locally, run as OpenShift runs it (needs Docker) |
| `azure/build.yaml` | Builds the image and pushes it to Docker Hub (public) as `healthopenshift/mobileenvironment:1.0.0.<n>` |
| `k8s/base/` | Deployment (3 replicas), service and pod disruption budget (at least 1 pod always available) |
| `k8s/overlays/{dev,test,prod}/` | For each environment: the namespace, the name prefix (`<env>-mobileenvironment-deployment`, `<env>-mobileenvironment-svc`, the name Kong points at, and `<env>-mobileenvironment-pdb`) and the image tag (the build number) |
| `../../Tools/Kong/environment.tmpl` | The public Kong route to the service |

The pods are labelled `role: api`, so the Helm release's existing `<release>-aps-netpol` lets Kong reach them. There's
no OpenShift route: the app is reached only through Kong.

## Test locally

```bash
./test.sh
```

It runs the container as a random non-root user with a read-only root filesystem and checks the response for several
`RELEASED_BUILD` values (set, unset, empty, not a number, leading zeros, spaces, too long), the headers, `/health`
and that other paths and methods are refused.

## Deploy

Do this once per environment. For Production, do it on Gold and then on GoldDR. Run the commands from the repository
root, logged in to the cluster with `oc`.

### 1. The image

The image is built by the `azure/build.yaml` pipeline when `Apps/MobileEnvironment` changes on `dev`, and pushed to
Docker Hub as the public `healthopenshift/mobileenvironment:<build number>`. The cluster pulls it from there directly,
with no image stream in the tools namespace. Each overlay names the build it runs (`newTag` in
`k8s/overlays/<env>/kustomization.yaml`), so check the overlay names a build that exists on Docker Hub.

### 2. Apply the manifests

```bash
oc apply -k Apps/MobileEnvironment/k8s/overlays/dev    # or test, prod
```

This creates the deployment, service and pod disruption budget in that overlay's namespace. The deployment starts with
no `RELEASED_BUILD`, so the endpoint returns `null`, which means Production only. Every step after this is therefore
safe whatever order it runs in.

Check:

```bash
oc get pods -l name=mobile-environment      # 3 ready pods
oc get pdb dev-mobileenvironment-pdb         # MIN AVAILABLE 1 (test-, prod- in those namespaces)
```

### 3. Publish the Kong route

The route is in `Tools/Kong/environment.tmpl`. If the Kong configuration hasn't been published since it was added, run
the `HG - Publish Kong` release for the environment. Then:

```bash
curl -i https://hg.dev.api.gov.bc.ca/api/mobileservice/environment
```

This should return `200` and `{"releasedBuild":null}`. A `404` with a Kong message means the route isn't published.
A `502` or `503` means Kong can't reach the pods: check the pods are ready and labelled `role: api`.

### 4. Set the released build

As below. Production gets `0` the first time: no 3.x build is released yet, so every 3.x build offers Test. Development
and Test can stay unset, or be set to whatever a test needs.

## Set the released build

Do this on every release, once both stores have approved the build and before it's released in either. In Production,
set it on Gold and on GoldDR, with the same value.

- **Console:** Workloads → Deployments → `<env>-mobileenvironment-deployment` (for example `prod-mobileenvironment-deployment`) → Environment. The first time, choose Add more and name it
  `RELEASED_BUILD`. After that, edit the value. Then Save.
- **CLI:** `oc set env deployment/<env>-mobileenvironment-deployment RELEASED_BUILD=<build>`

Saving starts a rolling update that keeps at least 2 of the 3 pods serving. It takes a few seconds.

Check the active cluster through the gateway:

```bash
curl -s https://hg.api.gov.bc.ca/api/mobileservice/environment
```

Public DNS reaches only the active cluster. To check the other one:

```bash
oc port-forward deployment/prod-mobileenvironment-deployment 8080:8080
curl -s localhost:8080/api/mobileservice/environment    # in another terminal
```

**Only ever raise it.** A lower value makes the public's build look unreleased, so new installs would start on Test.
`0` and `null` are safe on day one only because no 3.x build is public yet.

## Rules

- Apply manifest changes with `oc apply -k`. Never use `oc replace`, and never add `RELEASED_BUILD` to the manifests.
  `oc apply` removes only fields that were in the last manifest it applied, so a value set in the console or with
  `oc set env` survives every apply. If it were in a manifest, every apply would put the committed value back.
- If the value is ever lost, for example because the deployment was deleted and recreated, the endpoint returns `null`
  (Production only) until it's set again.
- The Kong route lives in `Tools/Kong/environment.tmpl`. Change it there and publish. A change made only in the API
  Services Portal is overwritten by the next publish.

## Other tasks

- **New image:** run the pipeline, then set the new build number as `newTag` in the environment's overlay, commit, and
  `oc apply -k` it. Promote by doing the same in the `test` and then `prod` overlays (Gold and GoldDR for Production).
  `RELEASED_BUILD` is kept, as with any apply.
- **Image pulls:** the image comes from Docker Hub anonymously, which limits pulls per IP address. The tags are build
  numbers that never change, so the deployment uses `imagePullPolicy: IfNotPresent` and a node pulls each build at most
  once. If pulls are ever refused (`ImagePullBackOff` with `toomanyrequests`), wait and retry.
- **Logs:** `oc logs deployment/<env>-mobileenvironment-deployment`. The first lines show the value each pod started with
  (`releasedBuild is 452`).
