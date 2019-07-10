FROM docker-registry.default.svc:5000/q6qfzk-tools/dotnet22-base:latest

# Install Chrome for Selenium
#RUN curl https://dl.google.com/linux/direct/google-chrome-stable_current_x86_64.rpm -o /chrome.deb
#RUN dpkg -i /chrome.deb || apt-get install -yf
#RUN rm /chrome.deb

# Install Chrome for Selenium
RUN mkdir -p /opt && \
  pushd /opt && \
  echo "Downloading https://dl.google.com/linux/direct/google-chrome-stable_current_x86_64.rpm" && \
  curl -sL https://dl.google.com/linux/direct/google-chrome-stable_current_x86_64.rpm && \
  rpm -i google-chrome-stable_current_x86_64.rpm

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