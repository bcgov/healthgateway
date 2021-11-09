FROM ubuntu:20.04
ARG NODE_VERSION=v16.13.0
ARG DOTNET_VERSION=6.0
ARG SONARSCANNER_VERSION=5.3.2

ENV SUMMARY="Azure DevOps agent with .NET"  \
    DESCRIPTION="Azure DevOps agent base image with .NET v${DOTNET_VERSION}, nodejs ${NODE_VERSION}, OpenShift CLI"

LABEL summary="$SUMMARY" \
    description="$DESCRIPTION" \
    io.k8s.description="$DESCRIPTION" \
    io.k8s.display-name="azure-devlops-.net${DOTNET_VERSION}" \
    io.openshift.expose-services="8080:http" \
    io.openshift.tags="builder,azure,devops,agent,.net-v${DOTNET_VERSION},nodejs-${NODE_VERSION},openshift" \
    release="1"

RUN  export DEBIAN_FRONTEND=noninteractive && \
    apt-get update -y && \
    apt-get install -y g++ build-essential python git default-jdk tzdata iputils-ping telnet traceroute curl apt-transport-https libpng-dev jq && \
    curl -sLO https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \ 
    rm packages-microsoft-prod.deb && \
    apt-get update -y && \ 
    apt-get install -y dotnet-sdk-${DOTNET_VERSION} && \
    apt-get install -y postgresql-client && \
    apt-get install -y dh-autoreconf && \
    ln -fs /usr/share/zoneinfo/America/Vancouver /etc/localtime && \
    dpkg-reconfigure --frontend noninteractive tzdata

# Fetch stock nodejs and install
RUN mkdir -p /opt && \
    cd /opt && \
    curl -sL https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.gz?raw=true | tar -zx && \
    rm -f node-${NODE_VERSION}-linux-x64.tar.gz

#Fetch OpenShift client and install
RUN mkdir -p /opt/bin && \
    cd /opt/bin && \
    curl -sL https://downloads-openshift-console.apps.silver.devops.gov.bc.ca/amd64/linux/oc.tar | tar -x && \
    rm -f oc.tar

# Update environment variables
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

RUN mkdir /opt/az &&\
    chmod -R 777 /opt/az

ADD https://github.com/bcgov/healthgateway/blob/dev/Tools/AzureAgent/scripts/start.sh?raw=true /opt/az/start.sh
ADD https://github.com/bcgov/healthgateway/blob/dev/Tools/AzureAgent/scripts/stop.sh?raw=true /opt/az/stop.sh
RUN chmod +rx /opt/az/*.sh

WORKDIR /opt/az

USER 1001

CMD ["bash", "-c", "/opt/az/start.sh"]
