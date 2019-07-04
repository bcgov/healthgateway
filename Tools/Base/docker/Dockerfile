FROM registry.access.redhat.com/openshift3/rhel7:latest

ARG DOTNET_MAJOR_VERSION=2
ARG DOTNET_VERSION=2.2

ENV SUMMARY="Azure .NETi base image"  \
    DESCRIPTION="Azure .NET v${DOTNET_VERSION}"

LABEL summary="$SUMMARY" \
      description="$DESCRIPTION" \
      io.k8s.description="$DESCRIPTION" \
      io.k8s.display-name="dotnet${NODE_MAJOR_VERSION}" \
      io.openshift.expose-services="5000:http" \
      io.openshift.tags="dotnet,.net-v${DOTNET_VERSION}" \
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

# Update environment variables
ENV PATH=$PATH:/opt/rh/rh-dotnet22/root/bin
ENV LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/usr/local/lib
ENV HOME=${APP_HOME}

WORKDIR ${APP_HOME}

USER 1001

CMD ["bash", "-c", "dotnet", "run"]
