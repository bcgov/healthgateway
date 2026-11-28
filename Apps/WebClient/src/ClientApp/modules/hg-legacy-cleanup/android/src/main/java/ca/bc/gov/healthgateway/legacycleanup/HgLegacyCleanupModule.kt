package ca.bc.gov.healthgateway.legacycleanup

import expo.modules.kotlin.Promise
import expo.modules.kotlin.modules.Module
import expo.modules.kotlin.modules.ModuleDefinition

/** JavaScript's side of the cleanup: the launch's report, and the same deletes on demand for Reset App. */
class HgLegacyCleanupModule : Module() {
  override fun definition() = ModuleDefinition {
    Name("HgLegacyCleanup")

    AsyncFunction("legacyCleanupReport") { promise: Promise ->
      LegacyCleanup.report { promise.resolve(it) }
    }

    AsyncFunction("removeLegacyData") { promise: Promise ->
      val context = appContext.reactContext?.applicationContext
      if (context == null) {
        promise.reject("ERR_LEGACY_CLEANUP", "No application context", null)
      } else {
        LegacyCleanup.removeNow(context) { promise.resolve(it) }
      }
    }
  }
}
