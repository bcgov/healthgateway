FROM selenium/node-firefox:3.6.0

ENV HOME=/home/seluser

USER root

RUN chown -R 1001:0 /opt/selenium 

RUN find /opt/selenium -exec chgrp 0 {} \; && \
    find /opt/selenium -exec chmod g+rw {} \; && \
    find /opt/selenium -type d -exec chmod g+x {} +

RUN find $HOME -exec chgrp 0 {} \; && \
    find $HOME -exec chmod g+rw {} \; && \
    find $HOME -type d -exec chmod g+x {} +

USER 1001
WORKDIR $HOME
