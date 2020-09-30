FROM ubuntu:20.04
ARG NODE_VERSION=v12.18.3
ARG DOTNET_VERSION=3.1
ARG OPENSHIFT_VERSION=v3.11.0-0cbc58b

ENV SUMMARY="Azure DevOps agent with .NET"  \
    DESCRIPTION="Azure DevOps agent base image with .NET v${DOTNET_VERSION}, nodejs ${NODE_VERSION}, OpenShift CLI ${OPENSHIFT_VERSION}"

LABEL summary="$SUMMARY" \
    description="$DESCRIPTION" \
    io.k8s.description="$DESCRIPTION" \
    io.k8s.display-name="azure-devlops-.net${DOTNET_VERSION}" \
    io.openshift.expose-services="8080:http" \
    io.openshift.tags="builder,azure,devops,agent-v${DEVOPS_AGENT_VERSION},.net-v${DOTNET_VERSION},nodejs-${NODE_VERSION},openshift-${OPENSHIFT_VERSION}" \
    release="1"

RUN  export DEBIAN_FRONTEND=noninteractive && \
    apt-get update -y && \
    apt-get install -y git default-jdk tzdata iputils-ping telnet traceroute curl apt-transport-https libpng-dev jq && \
    curl -sLO https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \ 
    rm packages-microsoft-prod.deb && \
    apt-get update -y && \ 
    apt-get install -y dotnet-sdk-${DOTNET_VERSION} && \
    apt-get install -y postgresql-client && \
    ln -fs /usr/share/zoneinfo/America/Vancouver /etc/localtime && \
    dpkg-reconfigure --frontend noninteractive tzdata

# Fetch stock nodejs and install
RUN mkdir -p /opt && \
    cd /opt && \
    curl -sL https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.gz?raw=true | tar -zx && \
    rm -f node-${NODE_VERSION}-linux-x64.tar.gz

#Fetch OpenShift client and install
RUN mkdir -p /opt && \
    cd /opt && \
    curl -sL https://github.com/openshift/origin/releases/download/v3.11.0/openshift-origin-client-tools-${OPENSHIFT_VERSION}-linux-64bit.tar.gz | tar -zx && \
    rm -f openshift-origin-client-tools-${OPENSHIFT_VERSION}-linux-64bit.tar.gz

# Update environment variables
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/lib
ENV AZP_URL=set_me_to_the_org_url
ENV AZP_TOKEN=set_me_to_a_pat
ENV HOME=/tmp
ENV NODE_HOME=/opt/node-${NODE_VERSION}-linux-x64
ENV PATH=$PATH:/opt/az/agent/bin:/opt/node-${NODE_VERSION}-linux-x64/bin:/opt/openshift-origin-client-tools-${OPENSHIFT_VERSION}-linux-64bit:$HOME/.local/bin/:$HOME/.dotnet/tools

# Update the version of `npm` that came with the packages above
# and install a few additional tools.
RUN npm i -g npm@latest yarn@latest nsp@latest nodemon@latest && \
    rm -rf ~/.npm && \
    node -v && \
    npm -v

# Install .Net tooling for SonarQube and Test Results
RUN dotnet tool install --global dotnet-sonarscanner --version 4.8.0 && \
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
