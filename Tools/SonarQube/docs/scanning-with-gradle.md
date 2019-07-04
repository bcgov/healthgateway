Prerequisites
=============
* [SonarQube](http://www.sonarqube.org/downloads/) 6.7+
* [Gradle](http://www.gradle.org/) 2.1 or higher

Usage
=====
* Analyze the project with SonarQube using Gradle:

        ./gradlew sonarqube [-Dsonar.host.url=... -Dsonar.jdbc.url=... -Dsonar.jdbc.username=... -Dsonar.jdbc.password=...]
        
Local Install
=============
To install SonarQube locally do the following:
* Download the version for your OS from [SonarQube](http://www.sonarqube.org/downloads/)
* Install locally following the directions
* Run server: http://localhost:9000
* Review your build.gradle, you need to add the following property: ```property "sonar.host.url", "http://localhost:9000"```
* run ./gradlew sonarqube from this directory
* Go to web browser and review result

References:
===========
Logging:
* https://docs.gradle.org/current/userguide/logging.html
  (add -i or --info to gradlew execution to see stdout during execution)
* https://stackoverflow.com/questions/3963708/gradle-how-to-display-test-results-in-the-console-in-real-time

Code to dispaly test results on stdout during execution to be added to gradle.build:

```
import org.gradle.api.tasks.testing.logging.TestExceptionFormat
import org.gradle.api.tasks.testing.logging.TestLogEvent

tasks.withType(Test) {
    testLogging {
        // set options for log level LIFECYCLE
        events TestLogEvent.FAILED,
               TestLogEvent.PASSED,
               TestLogEvent.SKIPPED,
               TestLogEvent.STANDARD_OUT
        exceptionFormat TestExceptionFormat.FULL
        showExceptions true
        showCauses true
        showStackTraces true

        // set options for log level DEBUG and INFO
        debug {
            events TestLogEvent.STARTED,
                   TestLogEvent.FAILED,
                   TestLogEvent.PASSED,
                   TestLogEvent.SKIPPED,
                   TestLogEvent.STANDARD_ERROR,
                   TestLogEvent.STANDARD_OUT
            exceptionFormat TestExceptionFormat.FULL
        }
        info.events = debug.events
        info.exceptionFormat = debug.exceptionFormat

        afterSuite { desc, result ->
            if (!desc.parent) { // will match the outermost suite
                def output = "Results: ${result.resultType} (${result.testCount} tests, ${result.successfulTestCount} successes, ${result.failedTestCount} failures, ${result.skippedTestCount} skipped)"
                def startItem = '|  ', endItem = '  |'
                def repeatLength = startItem.length() + output.length() + endItem.length()
                println('\n' + ('-' * repeatLength) + '\n' + startItem + output + endItem + '\n' + ('-' * repeatLength))
            }
        }
    }
}
```
