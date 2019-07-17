FROM selenium/node-chrome:3.6.0

ENV HOME=/home/seluser

USER root

COPY ./fix-permissions /usr/local/bin/fix-permissions

RUN chown -R 1001:0 /opt/selenium && \
  /usr/local/bin/fix-permissions /opt/selenium && \
  /usr/local/bin/fix-permissions $HOME
  
USER 1001
WORKDIR $HOME
