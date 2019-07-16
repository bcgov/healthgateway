FROM selenium/node-firefox:3.6.0

ENV HOME=/home/seluser

USER root

RUN chown -R 1001:0 /opt/selenium

RUN chgrp 0 /opt/selenium && \
	chmod g+rw,g+x /opt/selenium

RUN chgrp 0 $HOME && \
	chmod g+rw,g+x $HOME

USER 1001
WORKDIR $HOME
