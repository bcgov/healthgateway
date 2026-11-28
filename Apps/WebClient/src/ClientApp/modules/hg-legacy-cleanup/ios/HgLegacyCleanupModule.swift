import ExpoModulesCore

/** JavaScript's side of the cleanup: the launch's report, and the same deletes on demand for Reset App. */
public class HgLegacyCleanupModule: Module {
  public func definition() -> ModuleDefinition {
    Name("HgLegacyCleanup")

    AsyncFunction("legacyCleanupReport") { (promise: Promise) in
      LegacyCleanup.shared.report { promise.resolve($0) }
    }

    AsyncFunction("removeLegacyData") { (promise: Promise) in
      LegacyCleanup.shared.removeNow { promise.resolve($0) }
    }
  }
}
