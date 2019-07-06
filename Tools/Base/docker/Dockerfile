FROM registry.access.redhat.com/openshift3/rhel7:latest

ARG NODE_MAJOR_VERSION=10
ARG NODE_VERSION=v10.13.0
ARG DOTNET_MAJOR_VERSION=2
ARG DOTNET_VERSION=2.2

ENV SUMMARY=".NET core ${DOTNET_VERSION} base image"  \
    DESCRIPTION=".NET core v${DOTNET_VERSION} and nodejs v${NODE_VERSION}"

LABEL summary="$SUMMARY" \
      description="$DESCRIPTION" \
      io.k8s.description="$DESCRIPTION" \
      io.k8s.display-name="dotnet${NODE_MAJOR_VERSION}" \
      io.openshift.expose-services="5000:http" \
      io.openshift.tags="dotnet,.net-v${DOTNET_VERSION},nodejs-v${NODE_VERSION}" \
      release="1"

ENV PATH=$HOME/.local/bin/:$PATH \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8 

# We need to call 2 (!) yum commands before being able to enable repositories properly
# This is a workaround for https://bugzilla.redhat.com/show_bug.cgi?id=1479388
RUN yum repolist > /dev/null && \
    yum install -y yum-utils && \
    yum-config-manager --disable \* &> /dev/null && \
    yum-config-manager --enable rhel-server-rhscl-7-rpms && \
    yum-config-manager --enable rhel-7-server-rpms && \
    yum-config-manager --enable rhel-7-server-optional-rpms && \
    yum-config-manager --enable rhel-7-server-dotnet-rpms && \
    yum-config-manager --enable rhel-7-server-ose-3.11-rpms && \
    INSTALL_PKGS="nss_wrapper libtool-ltdl \
        scl-utils rh-dotnet22 atomic-openshift-clients" && \
    yum install -y --setopt=tsflags=nodocs $INSTALL_PKGS && \
    rpm -V $INSTALL_PKGS && \
    # Remove redhat-logos (httpd dependency) to keep image size smaller.
    # rpm -e --nodeps redhat-logos && \
    yum clean all -y && \
    rm -rf /var/cache/yum

# enable the rh-dotnet22 software collection environment 
RUN scl enable rh-dotnet22 bash

# Define and Create app home
ENV APP_HOME=/opt/app
RUN mkdir -p /opt ${APP_HOME} && \
    chgrp -R 0 ${APP_HOME} && \
    chmod -R g=u ${APP_HOME} /etc/passwd && \
    chmod g+s ${APP_HOME}

# Fetch stock nodejs and install
RUN mkdir -p /opt && \
    pushd /opt && \
    echo "Downloading https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.xz" && \
    curl -sL https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-linux-x64.tar.xz | tar -Jx && \
    rm -f node-${NODE_VERSION}-linux-x64.tar.xz

# Update environment variables
ENV PATH=$PATH:/opt/rh/rh-dotnet22/root/bin:/opt/node-${NODE_VERSION}-linux-x64/bin
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/lib
ENV HOME=${APP_HOME}
ENV NODE_HOME=/opt/node-${NODE_VERSION}-linux-x64

WORKDIR ${APP_HOME}

CMD ["dotnet run"]
