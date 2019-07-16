# Upgrading with Bundled Plugins

*The following "trick" is a workaround for [Missing plugins when upgrading Sonarqube Versions](https://github.com/BCDevOps/sonarqube/issues/6)*

When upgrading Sonarqube to a newer image, such as going from `bcgovimages/sonarqube:6.7.1` to `bcgovimages/sonarqube:6.7.5` which has a couple of handy plugins bundled with it, you may find the plugins don't get installed/registered.  Sonarqube does not seem to install bundled plugins when upgrading image versions.

Luckily the plugins are still included in the image.  So you can simply copy them into the plugins directory.

1. Open a terminal window to your Sonarqube instance.
1. Run the following commands to install the plugins (for example);
```
cp $SONARQUBE_HOME/lib/bundled-plugins/sonar-zap-plugin-1.1.2.jar $SONARQUBE_HOME/extensions/plugins/
cp $SONARQUBE_HOME/lib/bundled-plugins/qualinsight-sonarqube-badges-3.0.1.jar $SONARQUBE_HOME/extensions/plugins/
```
1. Login to your Sonarqube instance as `admin`
1. On the **Administration** > **System** screen click **Restart Server**
1. Once the server restarts verify the plugins were installed by heading over to the **Administration** > **Marketplace** screen and search for the plugins.  They should be marked as installed.