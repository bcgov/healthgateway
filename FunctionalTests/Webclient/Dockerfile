FROM registry.access.redhat.com/openshift3/rhel7:latest

RUN apt-get update
RUN apt-get install -y npm

RUN npm install selenium-side-runner@latest -g
RUN npm install chromedriver -g

# Install Chrome for Selenium
RUN curl https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb -o /chrome.deb
RUN dpkg -i /chrome.deb || apt-get install -yf
RUN rm /chrome.deb

# Install chromedriver for Selenium
RUN curl https://chromedriver.storage.googleapis.com/2.31/chromedriver_linux64.zip -o /usr/local/bin/chromedriver
RUN chmod +x /usr/local/bin/chromedriver

CMD ["npm", "test"]