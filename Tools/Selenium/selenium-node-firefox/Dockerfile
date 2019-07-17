FROM selenium/node-firefox:3.6.0

ENV HOME=/home/seluser

USER root


RUN chown -R 1001:0 /opt/selenium 

RUN find $1 -exec chgrp 0 /opt/selenium \; && \
    find $1 -exec chmod g+rw /opt/selenium \; && \
    find $1 -type d -exec chmod g+x /opt/selenium \;

RUN find $1 -exec chgrp 0 $HOME \; && \
    find $1 -exec chmod g+rw $HOME \; && \
    find $1 -type d -exec chmod g+x $HOME \;

USER 1001
WORKDIR $HOME
