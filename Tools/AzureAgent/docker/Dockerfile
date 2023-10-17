FROM ubuntu:22.04
ARG NODE_VERSION=v18.17.1
ARG DOTNET_VERSION=7.0
ARG SONARSCANNER_VERSION=5.13.1

ENV SUMMARY="Azure DevOps agent with .NET"  \
    DESCRIPTION="Azure DevOps agent base image with .NET v${DOTNET_VERSION}, nodejs ${NODE_VERSION}, OpenShift CLI"

RUN DEBIAN_FRONTEND=noninteractive apt-get update
RUN DEBIAN_FRONTEND=noninteractive apt-get upgrade -y

RUN DEBIAN_FRONTEND=noninteractive apt-get install -y -qq --no-install-recommends \
    ca-certificates lsb-release git tzdata iputils-ping netcat traceroute curl \
    apt-transport-https libpng-dev jq postgresql-client dh-autoreconf apt-utils  software-properties-common zip unzip \
    dotnet-sdk-${DOTNET_VERSION} default-jdk python3 g++ build-essential \
    redis-tools

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
RUN dotnet tool install --global dotnet-sonarscanner --version ${SONARSCANNER_VERSION}&& \
    dotnet tool install -g dotnet-reportgenerator-globaltool && \
    chmod -R 777 $HOME && \
    rm -rf /tmp/NuGetScratch/lock/* && \
    chmod 777 /tmp/.nuget

RUN mkdir -p /opt/az &&\
    chmod -R 777 /opt/az

ADD https://github.com/bcgov/healthgateway/blob/dev/Tools/AzureAgent/scripts/start.sh?raw=true /opt/az/start.sh
ADD https://github.com/bcgov/healthgateway/blob/dev/Tools/AzureAgent/scripts/stop.sh?raw=true /opt/az/stop.sh
RUN chmod +rx /opt/az/*.sh

WORKDIR /opt/az

USER 1001

CMD ["bash", "-c", "/opt/az/start.sh"]
