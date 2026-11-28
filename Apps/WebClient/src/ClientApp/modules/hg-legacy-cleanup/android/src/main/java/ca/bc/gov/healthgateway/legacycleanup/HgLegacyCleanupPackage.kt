package ca.bc.gov.healthgateway.legacycleanup

import android.app.Application
import android.content.Context
import expo.modules.core.interfaces.ApplicationLifecycleListener
import expo.modules.core.interfaces.Package

/** Found by Expo's autolinking through its name and the `Package` import: runs the cleanup as the process starts. */
class HgLegacyCleanupPackage : Package {
  override fun createApplicationLifecycleListeners(context: Context): List<ApplicationLifecycleListener> =
    listOf(
      object : ApplicationLifecycleListener {
        override fun onCreate(application: Application) {
          LegacyCleanup.runAtLaunch(application)
        }
      }
    )
}
