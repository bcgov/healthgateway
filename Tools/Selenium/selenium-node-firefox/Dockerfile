FROM selenium/node-firefox:3.6.0

ENV HOME=/home/seluser

USER root

RUN chown -R 1001:0 /opt/selenium

USER 1001
WORKDIR $HOME
