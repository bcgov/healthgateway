FROM ubuntu:25.10
ARG NODE_VERSION=v22.20.0
ARG DOTNET_VERSION=10.0
ARG SONARSCANNER_VERSION=11.0.0
ARG VAULT_VERSION=1.20.4
ARG HELM_VERSION=v3.15.3

ENV SUMMARY="Azure DevOps agent with .NET"  \
    DESCRIPTION="Azure DevOps agent base image with .NET v${DOTNET_VERSION}, nodejs ${NODE_VERSION}, OpenShift CLI"

RUN DEBIAN_FRONTEND=noninteractive apt-get update && \
    DEBIAN_FRONTEND=noninteractive apt-get upgrade -y && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y -qq --no-install-recommends \
    ca-certificates lsb-release git tzdata iputils-ping netcat-openbsd traceroute curl \
    apt-transport-https libpng-dev jq postgresql-client dh-autoreconf apt-utils zip unzip \
    dotnet-sdk-${DOTNET_VERSION} default-jdk python3 g++ build-essential redis-tools

RUN ln -fs /usr/share/zoneinfo/America/Vancouver /etc/localtime && \
    dpkg-reconfigure --frontend noninteractive tzdata

RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Fetch stock nodejs and install
RUN mkdir -p /opt && \
    cd /opt && \
    curl -sL https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.gz?raw=true | tar -zx && \
    rm -f node-${NODE_VERSION}-linux-x64.tar.gz

#Fetch OpenShift client and install
RUN mkdir -p /opt/bin && \
    cd /opt/bin && \
    curl -sL https://downloads-openshift-console.apps.gold.devops.gov.bc.ca/amd64/linux/oc.tar | tar -x && \
    rm -f oc.tar

# Install Hashicorp Vault
RUN curl -sL https://releases.hashicorp.com/vault/${VAULT_VERSION}/vault_${VAULT_VERSION}_linux_amd64.zip -o - | funzip > /opt/bin/vault && \
chmod +x /opt/bin/vault 

# Install Helm
RUN curl https://raw.githubusercontent.com/helm/helm/main/scripts/get-helm-3 | bash

# Update environment variables
ENV TARGETARCH=linux-x64
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/lib
ENV AZP_URL=set_me_to_the_org_url
ENV AZP_TOKEN=set_me_to_a_pat
ENV HOME=/tmp
ENV NODE_HOME=/opt/node-${NODE_VERSION}-linux-x64
ENV PATH=$PATH:/opt/az/agent/bin:/opt/node-${NODE_VERSION}-linux-x64/bin:/opt/bin:$HOME/.local/bin/:$HOME/.dotnet/tools

# Update the version of `npm` that came with the packages above
RUN npm i -g npm@latest && \
    rm -rf ~/.npm && \
    node -v && \
    npm -v

# Install .Net tooling for SonarQube and Test Results
RUN dotnet tool install --global dotnet-sonarscanner --version ${SONARSCANNER_VERSION} && \
    chmod -R 777 $HOME && \
    rm -rf /tmp/NuGetScratch/lock/* && \
    chmod 777 /tmp/.nuget

RUN mkdir -p /opt/az &&\
    chmod -R 777 /opt/az

COPY scripts/start.sh /opt/az/start.sh
COPY scripts/stop.sh /opt/az/stop.sh
COPY scripts/getSecrets.sh /opt/bin/getSecrets 
COPY scripts/pushSecrets.sh /opt/bin/pushSecrets
RUN chmod +rx /opt/az/*.sh && \
    chmod +rx /opt/bin/getSecrets && \
    chmod +rx /opt/bin/pushSecrets

WORKDIR /opt/az

USER 1001

CMD ["bash", "-c", "/opt/az/start.sh"]
