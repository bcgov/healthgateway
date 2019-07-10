FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest

# Install Chrome for Selenium
RUN cat << EOF > /etc/yum.repos.d/google-x86_64.repo \
[google64] \
name=Google - x86_64 \
baseurl=http://dl.google.com/linux/rpm/stable/x86_64 \
enabled=1 \
gpgcheck=1 \
gpgkey=https://dl-ssl.google.com/linux/linux_signing_key.pub \
EOF

RUN yum install ./google-chrome-stable_current_*.rpm

# Install chromedriver for Selenium
RUN mkdir -p /opt && \
  pushd /opt && \
  echo "Downloading https://chromedriver.storage.googleapis.com/2.31/chromedriver_linux64.zip" && \
  curl -sL https://chromedriver.storage.googleapis.com/2.31/chromedriver_linux64.zip -o /usr/local/bin/chromedriver && \
  chmod +x /usr/local/bin/chromedriver

# Install chromedriver for Selenium
#RUN curl https://chromedriver.storage.googleapis.com/2.31/chromedriver_linux64.zip -o /usr/local/bin/chromedriver
#RUN chmod +x /usr/local/bin/chromedriver

RUN npm install

CMD ["npm", "test"]