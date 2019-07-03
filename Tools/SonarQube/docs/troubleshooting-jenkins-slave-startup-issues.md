# Troubleshooting Jenkins Slave Startup Issues

## curl: Failed connect - when downloading remoting.jar
### Symptom - 
The Jenkins slave pod gets stuck in a crash loop with the following error:
```
Downloading http://172.50.180.127:80//jnlpJars/remoting.jar  ...
curl: (7) Failed connect to 172.50.180.127:80; Connection timed out
```

When there are no services or pods in your project with that IP address.  *The IP address may differ.*

### Description -
This can happen with older Jenkins instances.

The the master Jenkins instance is using an outdated IP address which is being given to the slave pod in order for it to connect back to the master instance.

The master Jenkins instance should not be configured with an IP address, it should be configured with the Jenkins service name.

### Solution -
Logon to your master Jenkins instance.
In `Jenkins` > `Configuration` under `Cloud` > `Kubernetes`
- Update **Jenkins** URL from `http://172.50.180.127:80` to `http://jenkins:80`
- Update **Jenkins tunnel** from `172.50.150.96:50000` to `jenkins-jnlp:50000`
- While your at it, update **Container Cap** from `10` to `100`