FROM jboss/base-jdk:8

MAINTAINER Erik Jacobs <erikmjacobs@gmail.com>
MAINTAINER Siamak Sadeghianfar <siamaksade@gmail.com>
MAINTAINER Roland Stens (roland.stens@gmail.com)
MAINTAINER Wade Barnes (wade.barnes@shaw.ca)

# Define Plug-in Versions
ARG SONAR_ZAP_PLUGIN_VERSION=1.1.2
ARG QUALINSIGHT_SONARQUBE_BADGES_VERSION=3.0.1

ENV SONAR_VERSION=7.8 \
    SONARQUBE_HOME=/opt/sonarqube \
    SONARQUBE_JDBC_USERNAME=sonar \
    SONARQUBE_JDBC_PASSWORD=sonar \
    SONARQUBE_JDBC_URL=

ENV SONARQUBE_PLUGIN_DIR=$SONARQUBE_HOME/lib/bundled-plugins

ENV SUMMARY="SonarQube for bcgov OpenShift" \
    DESCRIPTION="This image creates the SonarQube image for use at bcgov/OpenShift"

LABEL summary="$SUMMARY" \
      description="$DESCRIPTION" \
      io.k8s.description="$DESCRIPTION" \
      io.k8s.display-name="sonarqube" \
      io.openshift.expose-services="9000:http" \
      io.openshift.tags="sonarqube" \
      release="$SONAR_VERSION"

USER root
EXPOSE 9000
ADD root /

RUN set -x \
    && cd /opt \
    && curl -o sonarqube.zip -fSL https://binaries.sonarsource.com/Distribution/sonarqube/sonarqube-$SONAR_VERSION.zip \
    && unzip sonarqube.zip \
    && mv sonarqube-$SONAR_VERSION sonarqube \
    && rm sonarqube.zip* \
    && rm -rf $SONARQUBE_HOME/bin/*

# ================================================================================================================================================================================
# Bundle Plug-in(s)
# --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# sonar-zap-plugin
# https://github.com/Coveros/zap-sonar-plugin
# Version 1.1.2 of the plug-in requires LTS version 6.7.5 of SonarQube, and is not compatible with version 7.x yet.
# - https://github.com/Coveros/zap-sonar-plugin/issues/40
# - https://github.com/Coveros/zap-sonar-plugin/pull/41
ADD https://github.com/Coveros/zap-sonar-plugin/releases/download/sonar-zap-plugin-$SONAR_ZAP_PLUGIN_VERSION/sonar-zap-plugin-$SONAR_ZAP_PLUGIN_VERSION.jar $SONARQUBE_PLUGIN_DIR

# qualinsight-plugins-sonarqube-badges
# https://github.com/QualInsight/qualinsight-plugins-sonarqube-badges
# This plug-in is for use with SonarQube versions <7.1.
# From SonarQube 7.1 badges are available from the platform without a plugin.
ADD https://github.com/QualInsight/qualinsight-plugins-sonarqube-badges/releases/download/qualinsight-plugins-sonarqube-badges-$QUALINSIGHT_SONARQUBE_BADGES_VERSION/qualinsight-sonarqube-badges-$QUALINSIGHT_SONARQUBE_BADGES_VERSION.jar $SONARQUBE_PLUGIN_DIR
# ================================================================================================================================================================================

WORKDIR $SONARQUBE_HOME
COPY run.sh $SONARQUBE_HOME/bin/

RUN useradd -r sonar
RUN /usr/bin/fix-permissions $SONARQUBE_HOME \
    && chmod 775 $SONARQUBE_HOME/bin/run.sh

USER sonar
ENTRYPOINT ["./bin/run.sh"]
