import ExpoModulesCore
import Foundation
import LocalAuthentication
import Security

/**
 * The record cache's P-256 key pair. The private key is generated inside the Secure Enclave and never leaves it; the
 * Enclave only uses it after biometrics or the device passcode, on this device only, and iOS deletes it if the passcode is
 * removed. Saving needs only the public key: ECIES makes a new AES key for each save, so nothing asks and no key is kept.
 * Reading needs the private key, so the OS asks then. Simulators have no Enclave and keep the private key in the Keychain
 * under the same access rule.
 */
public class HgSecureVaultModule: Module {
  private let service = "ca.bc.gov.healthgateway.vault"
  private let publicAccount = "recordCachePublicKey"
  /** From the design that kept an AES key: removed with the old copies. */
  private let legacyAccounts = ["recordCacheKeyWrapped", "recordCacheKey"]
  private let keyTag = Data("ca.bc.gov.healthgateway.vault.enclave".utf8)
  private let algorithm = SecKeyAlgorithm.eciesEncryptionCofactorX963SHA256AESGCM
  private let queue = DispatchQueue(label: "ca.bc.gov.healthgateway.vault")
  /** A prompt that has just passed covers the read that follows it, so the lock screen doesn't lead to a second one. */
  private let recentPromptSeconds: TimeInterval = 30
  private var recentContext: LAContext?
  private var recentUntil = Date.distantPast

  public func definition() -> ModuleDefinition {
    Name("HgSecureVault")

    AsyncFunction("unlock") { (prompt: String, _: String, promise: Promise) in
      self.queue.async { self.unlock(prompt: prompt, promise: promise) }
    }

    Function("lock") {
      self.queue.sync { self.forgetPrompt() }
    }

    AsyncFunction("encrypt") { (plaintext: String, promise: Promise) in
      self.queue.async { self.encrypt(plaintext, promise: promise) }
    }

    AsyncFunction("decrypt") { (ciphertext: String, prompt: String, _: String, promise: Promise) in
      self.queue.async { self.decrypt(ciphertext, prompt: prompt, promise: promise) }
    }

    AsyncFunction("destroy") { (promise: Promise) in
      self.queue.async {
        self.forgetPrompt()
        SecItemDelete(self.itemQuery(self.publicAccount) as CFDictionary)
        self.legacyAccounts.forEach { SecItemDelete(self.itemQuery($0) as CFDictionary) }
        SecItemDelete(self.privateKeyQuery() as CFDictionary)
        promise.resolve(nil)
      }
    }
  }

  private func forgetPrompt() {
    recentContext?.invalidate()
    recentContext = nil
    recentUntil = Date.distantPast
  }

  private func itemQuery(_ account: String) -> [String: Any] {
    return [
      kSecClass as String: kSecClassGenericPassword,
      kSecAttrService as String: service,
      kSecAttrAccount as String: account,
    ]
  }

  private func privateKeyQuery() -> [String: Any] {
    return [
      kSecClass as String: kSecClassKey,
      kSecAttrApplicationTag as String: keyTag,
      kSecAttrKeyType as String: kSecAttrKeyTypeECSECPrimeRandom,
    ]
  }

  private func rejectStatus(_ status: Int, promise: Promise) {
    // errSecUserCanceled, errSecAuthFailed, and LAError's authenticationFailed, userCancel, systemCancel, appCancel
    let cancelled: Set<Int> = [Int(errSecUserCanceled), Int(errSecAuthFailed), -1, -2, -4, -9]
    if cancelled.contains(status) {
      promise.reject("ERR_VAULT_CANCELLED", "Authentication did not complete")
    } else {
      promise.reject("ERR_VAULT_UNAVAILABLE", "Could not use the key (\(status))")
    }
  }

  // MARK: Prompt

  private func unlock(prompt: String, promise: Promise) {
    let context = LAContext()
    var canError: NSError?
    guard context.canEvaluatePolicy(.deviceOwnerAuthentication, error: &canError) else {
      promise.reject("ERR_VAULT_UNAVAILABLE", "Device security is not set up")
      return
    }
    context.evaluatePolicy(.deviceOwnerAuthentication, localizedReason: prompt) { passed, error in
      self.queue.async {
        guard passed else {
          self.rejectStatus((error as NSError?)?.code ?? 0, promise: promise)
          return
        }
        self.forgetPrompt()
        self.recentContext = context
        self.recentUntil = Date().addingTimeInterval(self.recentPromptSeconds)
        promise.resolve(nil)
      }
    }
  }

  // MARK: Keys

  private func createPrivateKey() -> SecKey? {
    #if targetEnvironment(simulator)
      let flags: SecAccessControlCreateFlags = .userPresence
    #else
      let flags: SecAccessControlCreateFlags = [.privateKeyUsage, .userPresence]
    #endif
    guard
      let access = SecAccessControlCreateWithFlags(
        nil, kSecAttrAccessibleWhenPasscodeSetThisDeviceOnly, flags, nil)
    else { return nil }
    var attributes: [String: Any] = [
      kSecAttrKeyType as String: kSecAttrKeyTypeECSECPrimeRandom,
      kSecAttrKeySizeInBits as String: 256,
      kSecPrivateKeyAttrs as String: [
        kSecAttrIsPermanent as String: true,
        kSecAttrApplicationTag as String: keyTag,
        kSecAttrAccessControl as String: access,
      ],
    ]
    #if !targetEnvironment(simulator)
      attributes[kSecAttrTokenID as String] = kSecAttrTokenIDSecureEnclave
    #endif
    return SecKeyCreateRandomKey(attributes as CFDictionary, nil)
  }

  private func privateKey(context: LAContext) -> SecKey? {
    var query = privateKeyQuery()
    query[kSecReturnRef as String] = true
    query[kSecUseAuthenticationContext as String] = context
    var item: CFTypeRef?
    guard SecItemCopyMatching(query as CFDictionary, &item) == errSecSuccess else { return nil }
    return (item as! SecKey)
  }

  private func storedPublicKey() -> SecKey? {
    var query = itemQuery(publicAccount)
    query[kSecReturnData as String] = true
    query[kSecMatchLimit as String] = kSecMatchLimitOne
    var item: CFTypeRef?
    guard SecItemCopyMatching(query as CFDictionary, &item) == errSecSuccess,
      let data = item as? Data
    else { return nil }
    let attributes: [String: Any] = [
      kSecAttrKeyType as String: kSecAttrKeyTypeECSECPrimeRandom,
      kSecAttrKeyClass as String: kSecAttrKeyClassPublic,
    ]
    return SecKeyCreateWithData(data as CFData, attributes as CFDictionary, nil)
  }

  /** Kept apart from the private key, so that saving never touches anything the OS would ask about. */
  private func publicKey() -> SecKey? {
    if let key = storedPublicKey() { return key }
    let context = LAContext()
    context.interactionNotAllowed = true
    guard let secret = privateKey(context: context) ?? createPrivateKey(),
      let key = SecKeyCopyPublicKey(secret),
      let data = SecKeyCopyExternalRepresentation(key, nil) as Data?
    else { return nil }
    legacyAccounts.forEach { SecItemDelete(itemQuery($0) as CFDictionary) }
    var add = itemQuery(publicAccount)
    add[kSecValueData as String] = data
    add[kSecAttrAccessible as String] = kSecAttrAccessibleWhenPasscodeSetThisDeviceOnly
    SecItemDelete(itemQuery(publicAccount) as CFDictionary)
    SecItemAdd(add as CFDictionary, nil)
    return key
  }

  // MARK: Records

  private func encrypt(_ plaintext: String, promise: Promise) {
    guard let key = publicKey() else {
      promise.reject("ERR_VAULT_UNAVAILABLE", "Device security is not set up")
      return
    }
    var error: Unmanaged<CFError>?
    guard
      let sealed = SecKeyCreateEncryptedData(key, algorithm, Data(plaintext.utf8) as CFData, &error)
        as Data?
    else {
      error?.release()
      promise.reject("ERR_VAULT_CRYPTO", "Could not encrypt")
      return
    }
    promise.resolve(sealed.base64EncodedString())
  }

  private func decrypt(_ ciphertext: String, prompt: String, promise: Promise) {
    guard let sealed = Data(base64Encoded: ciphertext) else {
      promise.reject("ERR_VAULT_CRYPTO", "Could not decrypt")
      return
    }
    let context: LAContext
    if let recent = recentContext, Date() < recentUntil {
      context = recent
    } else {
      forgetPrompt()
      context = LAContext()
      context.localizedReason = prompt
    }
    guard let secret = privateKey(context: context) else {
      promise.reject("ERR_VAULT_UNAVAILABLE", "There is no key")
      return
    }
    // This is the step the OS guards: it asks for biometrics or the passcode here, unless a prompt has just passed.
    var error: Unmanaged<CFError>?
    guard let opened = SecKeyCreateDecryptedData(secret, algorithm, sealed as CFData, &error) as Data?
    else {
      rejectStatus(error.map { CFErrorGetCode($0.takeRetainedValue()) } ?? 0, promise: promise)
      return
    }
    promise.resolve(String(decoding: opened, as: UTF8.self))
  }
}
