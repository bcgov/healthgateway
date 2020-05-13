FROM ubuntu:18.04
ARG DEVOPS_AGENT_VERSION=2.168.2
ARG NODE_VERSION=v12.13.1
ARG DOTNET_VERSION=3.1
ARG OPENSHIFT_VERSION=v3.11.0-0cbc58b

ENV SUMMARY="Azure DevOps agent with .NET"  \
    DESCRIPTION="Azure DevOps agent v${DEVOPS_AGENT_VERSION} with .NET v${DOTNET_VERSION}, nodejs ${NODE_VERSION}, OpenShift CLI ${OPENSHIFT_VERSION}"

LABEL summary="$SUMMARY" \
      description="$DESCRIPTION" \
      io.k8s.description="$DESCRIPTION" \
      io.k8s.display-name="azure-devlops-.net${DOTNET_VERSION}" \
      io.openshift.expose-services="8080:http" \
      io.openshift.tags="builder,azure,devops,agent-v${DEVOPS_AGENT_VERSION},.net-v${DOTNET_VERSION},nodejs-${NODE_VERSION},openshift-${OPENSHIFT_VERSION}" \
      release="1"

RUN  export DEBIAN_FRONTEND=noninteractive && \
     apt-get update -y -qq && \
     apt-get install -y -qq git default-jdk tzdata iputils-ping telnet traceroute curl apt-transport-https && \
     curl -sLO https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb && \
     dpkg -i packages-microsoft-prod.deb && \ 
     rm packages-microsoft-prod.deb && \
     apt-get update -y -qq  && \ 
     apt-get install -y -qq dotnet-sdk-${DOTNET_VERSION} && \
     ln -fs /usr/share/zoneinfo/America/Vancouver /etc/localtime && \
     dpkg-reconfigure --frontend noninteractive tzdata

# Fetch stock azure client and install
RUN mkdir -p /opt/az/agent && \
    mkdir -p /opt/az/_work && \
    cd /opt/az/agent && \
    curl -sL https://vstsagentpackage.azureedge.net/agent/${DEVOPS_AGENT_VERSION}/vsts-agent-linux-x64-${DEVOPS_AGENT_VERSION}.tar.gz | tar -zx && \
    rm -f vsts-agent-linux-x64-${DEVOPS_AGENT_VERSION}.tar.gz && \
    cd .. && \
    find /opt/az -exec chmod 777 {} \;

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

ADD https://github.com/bcgov/healthgateway/blob/dev/Tools/AzureAgent/scripts/start.sh?raw=true /opt/az/start.sh 
RUN chmod +rx /opt/az/start.sh

# Update environment variables
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/lib
ENV AZ_DEVOPS_ORG_URL=set_me_to_the_org_url
ENV AZ_DEVOPS_TOKEN=set_me_to_a_pat
ENV HOME=/tmp
ENV NODE_HOME=/opt/node-${NODE_VERSION}-linux-x64
ENV PATH=$PATH:/opt/az/agent/bin:/opt/node-${NODE_VERSION}-linux-x64/bin:/opt/openshift-origin-client-tools-${OPENSHIFT_VERSION}-linux-64bit:$HOME/.local/bin/:$HOME/.dotnet/tools

# Update the version of `npm` that came with the packages above
# and install a few additional tools.
RUN npm i -g npm@latest yarn@latest nsp@latest nodemon@latest && \
    rm -rf ~/.npm && \
    node -v && \
    npm -v

# Install the SonarQube scanner for dotnet
RUN dotnet tool install --global dotnet-sonarscanner --version 4.8.0
# Allow other users to execute the tools
RUN chmod -R 777 $HOME
# Fix nugget permissions
RUN rm -rf /tmp/NuGetScratch/lock/*
RUN chmod 777 /tmp/.nuget

WORKDIR /opt/az/_work

USER 1001

CMD ["bash", "-c", "/opt/az/start.sh", "-u", "$AZ_DEVOPS_ORG_URL", "-t", "$AZ_DEVOPS_TOKEN"]
