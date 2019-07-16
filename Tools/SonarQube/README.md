---
title: SonarQube on OpenShift
description: Documentation and resources (complete with examples) required to deploy a SonarQube server instance into a BCGov OpenShift pathfinder environment, and integrate SonarQube and ZAP scanning into your Jenkins pipeline.  With SonarQube you can perform and report on code quality, and code coverage, and scan for known vulnerabilities and security issues.
author: WadeBarnes
resourceType: Components
personas: 
  - Developer
  - Product Owner
  - Designer
labels:
  - sonarqube
  - zap
  - scan
  - test
  - code
  - quality
  - vulnerability
  - coverage
---
# SonarQube on OpenShift
The [sonarqube](https://github.com/BCDevOps/sonarqube) repository contains all of the resources required to deploy a SonarQube server instance into a BCGov OpenShift pathfinder environment, and integrate SonarQube scanning into your Jenkins pipeline.

This work was inspired by the OpenShift Demos SonarQube for OpenShift:
https://github.com/OpenShiftDemos/sonarqube-openshift-docker

There are two parts to SonarQube, the server, and the scanner.  You deploy the server once and it analyses and hosts the scanning results posted by the scanner.  You integrate the scanner into your builds/pipelines to perform code analysis and then post the results to the server.  The server then provides summaries and live drill down reports of the results.

These instructions assume:
* You have Git and the OpenShift CLI installed on your system, and they are functioning correctly.  The recommended approach is to use either [Homebrew](https://brew.sh/) (MAC) or [Chocolatey](https://chocolatey.org/) (Windows) to install the required packages.
* You have forked and cloned a local working copy of the project source code.
* You are using a reasonable shell.  A "reasonable shell" is obvious on Linux and Mac, and is assumed to be the git-bash shell on Windows.

# SonarQube Server
The following instructions describe how to build and deploy a SonarQube server instance for your project.  The build step is optional since images are already available.

## SonarQube Server Images

SonarQube server images are now available on DockerHub:
- [bcgovimages/sonarqube](https://hub.docker.com/r/bcgovimages/sonarqube/)

## Building the SonarQube Server Image

Logon to your `tools` project and run the following command:

    oc new-build https://github.com/HealthGateway/Tools/SonarQube --name=sonarqube --to=sonarqube:7.8

## Deploy on OpenShift
The [sonarqube-postgresql-template](./sonarqube-postgresql-template.yaml) has been provided to allow you to quickly and easily deploy a fully functional instance of the SonarQube server, complete with persistent storage, into your `tools` project.  The template will create all of the necessary resources for you.

Logon to your `tools` project and run the following command:

    oc new-app -f sonarqube-postgresql-template.yaml --param=SONARQUBE_VERSION=7.8
 
## Change the Default Admin Password
When the SonarQube server is first deployed it is using a default `admin` password.  For security, it is **highly** recommended you change it.  The [UpdateSqAdminPw](./provisioning/updatesqadminpw.sh) script has been provided to make this easy.  The script will generate a random password, store it in an OpenShift secret named `sonarqube-admin-password`, and update the admin password of the SonarQube server instance.

Logon to your `tools` project and run the following command from the [provisioning](./provisioning) directory:

    updatesqadminpw.sh 

To login to your SonarQube server as admin, browse to the **sonarqube-admin-password** secret in your OpenShift `tools` project, reveal the password and use it to login.

## Congratulations - You now have a running SonarQube server instance
You can now browse your SonarQube server site.  To find the link, browse to the overview of your `tools` project using the OpenShift console and click on the URL for the **SonarQube Application**.

## Optional GitHub Authentication

The GitHub authentication plug-in requirements are:

* SonarQube must be publicly accessible through HTTPS only
* The property 'sonar.core.serverBaseURL' must be set to this public HTTPS URL
* In the GitHub profile for the org, you need to create a Developer Application for which the 'Authorization callback URL' must be set to '/oauth2/callback'.

*The settings can be found under Administration -> Configuration -> General Settings -> GitHub)*

# SonarQube Scanner
The following instructions describe how to quickly integrate static SonarQube scanning into your Jenkins pipeline.

## Add the scanner scripts to your project
Gradle in combination with the `sonarqube` gradle plug-in are used to perform the scanning.  A complete gradle environment and a generic [build.gradle](./sonar-runner/build.gradle) file are provided in the [sonar-runner](./sonar-runner) directory of this project.  These scripts will be used by the Jenkins pipeline script to run SonarQube scans of your code.

Add the [sonar-runner](./sonar-runner) directory to your project.  The defaults in [build.gradle](./sonar-runner/build.gradle) assume the [sonar-runner](./sonar-runner) directory is a top level directory within your project, but it does not have to be.  You can easily override the defaults by setting the appropriate properties within the [Jenkinsfile](./jenkins/SonarQube-StaticScan-Jenkinsfile) described in the next section.

For more information about scanning with Gradle, refer to the [Scanning with Gradle](./docs/scanning-with-gradle.md) document.

## Add the Jenkins pipeline script to your project
An example Jenkins file [SonarQube-StaticScan-Jenkinsfile](./jenkins/SonarQube-StaticScan-Jenkinsfile) is provided in this project.  It performs static SonarQube scanning of your project's code.  It uses a purpose built Jenkins slave image, which is already available in the BCGov OpenShift pathfinder environment, along with the [sonar-runner](./sonar-runner) scripts described in the previous section, to run the scans.

- Add the [SonarQube-StaticScan-Jenkinsfile](./jenkins/SonarQube-StaticScan-Jenkinsfile) to your project.
- In the Jenkins file, update the variables in the **SonarQube Scanner Settings** section as needed for your project.
- Create a pipeline in your OpenShift `tools` project that references it.
- Run and test the pipeline
  - Start the pipeline manually and ensure it runs through to completion successfully.
  - Browse the project report on the SonarQube server.
- Wire the pipeline up to a GitHub Webhook.

## Congratulations - You have integrated static code scanning into your project
You can now browse your project report on the SonarQube server site.  To find the link, browse to the overview of your `tools` project using the OpenShift console and click on the URL for the **SonarQube Application**.

## Next Steps:

### Code Coverage Results
Now that you have static scanning, you'll probably notice your code coverage results are at 0% since no unit tests are being executed during the scan.  You'll likely what to integrate unit tests into the scans so you get code coverage metrics to help you determine how well you are testing your code.  **As you journey down this road, please contribute your experience back to this project to make it better for the whole community.**

### Integrate OWASP ZAP Security Vulnerability Scanning into SonarQube
To make the results of your ZAP security vulnerability scanning accessible and therefore more actionable, you can integrate the scan results into a SonarQube project report.  To accomplish this you can use the [ZAP Plugin for SonarQube](https://github.com/Coveros/zap-sonar-plugin), which is bundled in the `bcgovimages/sonarqube:6.7.5` image.

The [SonarQube-Integrated-ZapScan-Jenkinsfile](./jenkins/SonarQube-Integrated-ZapScan-Jenkinsfile) example shows you how to utilize ZAP and the plug-in together to perform a ZAP security vulnerability scan on your application, and then publish the report with SonarQube.

The example can be used as a starting point for your project.
- Add the [SonarQube-Integrated-ZapScan-Jenkinsfile](./jenkins/SonarQube-Integrated-ZapScan-Jenkinsfile) to your project.
- In the Jenkins file, update the variables in the **SonarQube Scanner Settings** and **ZAP Scanner Settings** sections as needed for your project.
- Create a pipeline in your OpenShift `tools` project that references it.
- Run and test the pipeline
  - Start the pipeline manually and ensure it runs through to completion successfully.
  - Browse the project report on the SonarQube server.

__Please Note:__ the first time running the ZAP pipeline, the execution will likely fail due to missing script execution permissions on Jenkins. The ZAP pipeline logs will provide a message with a link to the Jenkins admin dashboard page where the script permissions can be added (something like `https://your-jenkins-instance.pathfinder.gov.bc.ca/scriptApproval/`). Just click the "approve" button on the pending signature to add it to the approved list and repeat the process (run the pipeline, wait for it to fail and approve the next required script) .
The script signatures that will require approval before the pipeline can run successfully are:
```
method hudson.plugins.git.GitSCM getBranches
method hudson.plugins.git.GitSCM getUserRemoteConfigs
method hudson.plugins.git.GitSCMBackwardCompatibility getExtensions
```

Now that you have a dedicated pipeline for running ZAP scans on your deployed application, you'll want to integrate it into your build and deployment pipeline.  The [ZapScan-Integration-Example-Jenkinsfile](./jenkins/ZapScan-Integration-Example-Jenkinsfile) provides an example of how to accomplish this while keeping things clean and separated.  The example uses the [OpenShift Jenkins Pipeline DSL](https://github.com/openshift/jenkins-client-plugin) syntax.  There are two parts of the example you'll want to focus on; the deployment section, and the ZAP Scanning section.  The deployment section waits for the application deployment to complete before allowing the execution to continue.  This ensures you are running your scan following a successful deployment.  The ZAP Scanning section triggers the independent `zap-pipeline` (created above) which scans your application and publishes the report.  The example does not wait for the pipeline to complete, but the example(s) could easily be modified to wait for feedback and perform some additional operations based on the results of the scan.

Additional details regarding the `openshift/jenkins-slave-zap` image can be found here; [OWASP ZAP Security Vulnerability Scanning](https://github.com/BCDevOps/openshift-components/tree/master/cicd/jenkins-slave-zap)

### Quality Badges
Now that you are scanning your code you can publish the summary of the results using badges in your project's top-level ReadMe file.

For SonarQube versions <7.1 you will need to use the [SVG Badges](https://github.com/QualInsight/qualinsight-plugins-sonarqube-badges) plug-in, which is bundled in the `bcgovimages/sonarqube:6.7.5` image.  Starting with SonarQube 7.1, badges are available from the platform without a plugin.

# Examples

[TheOrgBook](https://github.com/bcgov/TheOrgBook) provides an example of the concepts outlined here, and demonstrates the use of static code scanning, ZAP report integration, and quality badges.

# References
- [SonarQube](https://www.sonarqube.org/)

# Tips and Tricks

- [Troubleshooting Jenkins Slave Startup Issues](./docs/troubleshooting-jenkins-slave-startup-issues.md)
- [Upgrading with Bundled Plugins](./docs/upgrading-with-bundled-plugins.md)

# Getting Help or Reporting an Issue
To report bugs/issues/feature requests, please file an [issue](../../issues).

# How to Contribute
If you have found this project helpful, please contribute back to the project as you find new and better ways to use SonarQube in your projects.

If you would like to contribute, please see our [CONTRIBUTING](./CONTRIBUTING.md) guidelines.

Please note that this project is released with a [Contributor Code of Conduct](./CODE_OF_CONDUCT.md). 
By participating in this project you agree to abide by its terms.