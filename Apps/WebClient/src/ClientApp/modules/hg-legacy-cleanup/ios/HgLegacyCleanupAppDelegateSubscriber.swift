import ExpoModulesCore
import UIKit

/** Runs the cleanup while the app launches: React Native starts later, from the scene, so JavaScript never sees old data. */
public class HgLegacyCleanupAppDelegateSubscriber: ExpoAppDelegateSubscriber {
  public func application(
    _ application: UIApplication,
    didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]? = nil
  ) -> Bool {
    LegacyCleanup.shared.runAtLaunch(application)
    return true
  }
}
