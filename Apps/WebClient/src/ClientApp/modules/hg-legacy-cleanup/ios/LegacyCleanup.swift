import Foundation
import Security
import UIKit
import os

/**
 * The store app has the ID of the old Health Gateway app (myhealthBC-iOS), so an update keeps everything the old app
 * saved, and its Keychain items outlive even an uninstall. Two jobs, both before JavaScript starts (legacy.md):
 *
 * 1. The fresh-install guard. Uninstalling removes UserDefaults but not the Keychain, so a missing `hg.installed` means a
 *    new install: every Keychain item the app can reach is deleted, the old app's and a stored login or app lock from an
 *    earlier install of this one.
 * 2. The sweep of the old app's files and settings, by name, until one run removes them all.
 */
// its state is only touched on `queue`, or on the main thread at launch
final class LegacyCleanup: @unchecked Sendable {
  static let shared = LegacyCleanup()

  /** Only the guard reads this. Reset App must leave it: clearing it would delete the next login at the next launch. */
  private let installedKey = "hg.installed"
  /** Its own flag, so a file that can't be deleted retries the sweep without the guard deleting the Keychain again. */
  private let sweptKey = "hg.legacyCleanup.done"
  private let legacyService = "ca.bc.gov.myhealth"
  private let legacyDefaults: Set<String> = [
    "initialOnboardingScreensSeen", "rememberGatewayDetails", "enabledTypes", "hasAppLaunchedBefore",
    "loginProcessStatus", "hasSeenFirstLogin", "quickLinksPreferences", "analyticsEnabled",
    // Snowplow 2.x: the collector's address
    "url", "protocol", "method",
    // Snowplow 2.x: its ID for this install's user, and its install tracking
    "SPInstallationUserId", "SPInstalledBefore", "SPInstallTimestamp", "SPInstallVersion", "SPInstallBuild",
  ]
  // tvOS and watchOS only in Snowplow 2.x (on iOS the session is a file, removed with the snowplow folders): kept in case
  // an older version of the app wrote it
  private let snowplowSessionPrefix = "SPSessionDictionary_"
  private let log = os.Logger(subsystem: "ca.bc.gov.healthgateway", category: "legacyCleanup")
  private let queue = DispatchQueue(label: "ca.bc.gov.healthgateway.legacyCleanup", qos: .utility)
  /** What this launch's sweep did. Read and written on `queue` only. */
  private var launchReport: [Group]?
  private var activeObserver: NSObjectProtocol?

  struct Group {
    let name: String
    var found = 0
    var removed = 0
    var failed = 0

    var asDictionary: [String: Any] {
      ["group": name, "found": found, "removed": removed, "failed": failed]
    }
  }

  /** Main thread, in `didFinishLaunchingWithOptions`. */
  @MainActor
  func runAtLaunch(_ application: UIApplication) {
    // Locked before its first unlock, UserDefaults reads as empty: both flags would look missing on an install that has
    // them, and the guard would sign the user out. Nothing can read the Keychain until the unlock either.
    guard application.isProtectedDataAvailable else {
      activeObserver = NotificationCenter.default.addObserver(
        forName: UIApplication.didBecomeActiveNotification, object: nil, queue: .main
      ) { [weak self] _ in
        guard let self, let observer = self.activeObserver else { return }
        NotificationCenter.default.removeObserver(observer)
        self.activeObserver = nil
        self.run()
      }
      return
    }
    run()
  }

  private func run() {
    let defaults = UserDefaults.standard
    var keychain: [Group] = []
    if !defaults.bool(forKey: installedKey) {
      // counted first: the guard's deletes take the old app's items with them
      keychain = [removeLegacyKeychain(), removeEveryKeychainItem()]
      // Set even after a failed delete, as the old app did: a delete that never passes would otherwise sign the user out
      // at every launch.
      defaults.set(true, forKey: installedKey)
    }
    guard !defaults.bool(forKey: sweptKey) else {
      if !keychain.isEmpty { queue.async { self.finish(keychain) } }
      return
    }
    // none of the old names are anything this app reads, so start-up doesn't wait for it
    queue.async {
      let groups = (keychain.isEmpty ? [self.removeLegacyKeychain()] : keychain) + self.sweep()
      self.finish(groups)
    }
  }

  /** The launch's report, once the sweep has finished. Nil: nothing ran, as it was all done at an earlier launch. */
  func report(_ completion: @escaping ([[String: Any]]?) -> Void) {
    queue.async { completion(self.launchReport?.map(\.asDictionary)) }
  }

  /** Reset App: the same deletes, whatever the flags say. */
  func removeNow(_ completion: @escaping ([[String: Any]]) -> Void) {
    queue.async {
      let groups = [self.removeLegacyKeychain()] + self.sweep()
      self.markSwept(groups)
      completion(groups.map(\.asDictionary))
    }
  }

  private func finish(_ groups: [Group]) {
    launchReport = groups
    markSwept(groups)
    for group in groups where group.found > 0 {
      // names and counts only
      log.notice(
        "Legacy cleanup \(group.name, privacy: .public): found \(group.found), removed \(group.removed), failed \(group.failed)"
      )
    }
  }

  private func markSwept(_ groups: [Group]) {
    if groups.allSatisfy({ $0.failed == 0 }) {
      UserDefaults.standard.set(true, forKey: sweptKey)
    }
  }

  // MARK: Keychain

  /** The old app's items: generic passwords under its service, in the default access group (the same team's). */
  private func removeLegacyKeychain() -> Group {
    var group = Group(name: "legacyKeychain")
    let query: [CFString: Any] = [
      kSecClass: kSecClassGenericPassword,
      kSecAttrService: legacyService,
      kSecAttrSynchronizable: kSecAttrSynchronizableAny,
    ]
    var items: CFTypeRef?
    let count = query.merging([kSecMatchLimit: kSecMatchLimitAll, kSecReturnAttributes: true]) { $1 }
    if SecItemCopyMatching(count as CFDictionary, &items) == errSecSuccess {
      group.found = (items as? [Any])?.count ?? 0
    }
    let status = SecItemDelete(query as CFDictionary)
    if status == errSecSuccess {
      group.found = max(group.found, 1)
      group.removed = group.found
    } else if status != errSecItemNotFound {
      group.found = max(group.found, 1)
      group.failed = group.found
      log.error("Legacy cleanup: Keychain delete failed, status \(status)")
    }
    return group
  }

  /**
   * Every item in the app's own access group, the only one it has (no keychain-access-groups, no app groups): SecureStore's
   * login and app lock, the vault's key pair, and anything a library adds later. One delete per class, counted by class.
   */
  private func removeEveryKeychainItem() -> Group {
    var group = Group(name: "keychain")
    let classes = [
      kSecClassGenericPassword, kSecClassInternetPassword, kSecClassCertificate, kSecClassKey, kSecClassIdentity,
    ]
    for itemClass in classes {
      let query: [CFString: Any] = [kSecClass: itemClass, kSecAttrSynchronizable: kSecAttrSynchronizableAny]
      let status = SecItemDelete(query as CFDictionary)
      if status == errSecSuccess {
        group.found += 1
        group.removed += 1
      } else if status != errSecItemNotFound {
        group.found += 1
        group.failed += 1
        log.error("Legacy cleanup: Keychain delete failed, status \(status)")
      }
    }
    return group
  }

  // MARK: Files and settings

  private func sweep() -> [Group] {
    let files = FileManager.default
    var database = Group(name: "database")
    var pdf = Group(name: "pdf")
    var analytics = Group(name: "analytics")

    if let documents = files.urls(for: .documentDirectory, in: .userDomainMask).first {
      // The old app's own rule (MigrationService.removeExistingDBIfNeeded): its encrypted Core Data store and the files
      // beside it. This app's copies are in the hg-record-cache folder, and AsyncStorage is in Application Support.
      for url in topLevelFiles(of: documents) where url.lastPathComponent.contains(".sqlite") {
        remove(url, into: &database)
      }
      remove(documents.appendingPathComponent("MyHealth Document.pdf"), into: &pdf)
    }
    // Snowplow 2.x's event queue and session. This app's tracker is JavaScript only and keeps its data in AsyncStorage.
    if let library = files.urls(for: .libraryDirectory, in: .userDomainMask).first {
      remove(library.appendingPathComponent("snowplow"), into: &analytics)
      remove(library.appendingPathComponent("Application Support/snowplow"), into: &analytics)
      for url in topLevelFiles(of: library) where url.lastPathComponent.hasPrefix("snowplowEvents.sqlite") {
        remove(url, into: &analytics)
      }
    }
    return [database, pdf, analytics, removeLegacyDefaults()]
  }

  private func topLevelFiles(of folder: URL) -> [URL] {
    let entries = try? FileManager.default.contentsOfDirectory(
      at: folder, includingPropertiesForKeys: [.isRegularFileKey])
    return (entries ?? []).filter { (try? $0.resourceValues(forKeys: [.isRegularFileKey]))?.isRegularFile == true }
  }

  private func remove(_ url: URL, into group: inout Group) {
    guard FileManager.default.fileExists(atPath: url.path) else { return }
    group.found += 1
    let name = group.name
    do {
      if Self.forcedFailure { throw CocoaError(.fileWriteNoPermission) }
      try FileManager.default.removeItem(at: url)
      group.removed += 1
    } catch {
      group.failed += 1
      log.error("Legacy cleanup: could not delete from \(name, privacy: .public): \((error as NSError).code)")
    }
  }

  /** By key: React Native and Expo keep their own settings in the same domain, so the domain stays. */
  private func removeLegacyDefaults() -> Group {
    var group = Group(name: "preferences")
    let defaults = UserDefaults.standard
    guard let domain = Bundle.main.bundleIdentifier, let saved = defaults.persistentDomain(forName: domain) else {
      return group
    }
    for key in saved.keys where legacyDefaults.contains(key) || key.hasPrefix(snowplowSessionPrefix) {
      group.found += 1
      defaults.removeObject(forKey: key)
      group.removed += 1
    }
    return group
  }

  /** Debug builds, for the test plan: `-hg.legacyCleanup.forceFailure YES` as a launch argument fails every file delete. */
  private static var forcedFailure: Bool {
    #if DEBUG
    return UserDefaults.standard.bool(forKey: "hg.legacyCleanup.forceFailure")
    #else
    return false
    #endif
  }
}
