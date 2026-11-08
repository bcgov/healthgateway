package ca.bc.gov.healthgateway.vault

import android.app.KeyguardManager
import android.content.Context
import android.content.pm.PackageManager
import android.os.Build
import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyPermanentlyInvalidatedException
import android.security.keystore.KeyProperties
import android.security.keystore.UserNotAuthenticatedException
import android.util.Base64
import android.util.Log
import androidx.biometric.BiometricManager
import androidx.biometric.BiometricManager.Authenticators.BIOMETRIC_STRONG
import androidx.biometric.BiometricManager.Authenticators.DEVICE_CREDENTIAL
import androidx.biometric.BiometricPrompt
import androidx.core.content.ContextCompat
import androidx.fragment.app.FragmentActivity
import expo.modules.kotlin.Promise
import expo.modules.kotlin.functions.Queues
import expo.modules.kotlin.modules.Module
import expo.modules.kotlin.modules.ModuleDefinition
import java.nio.ByteBuffer
import java.security.GeneralSecurityException
import java.security.KeyFactory
import java.security.KeyPairGenerator
import java.security.KeyStore
import java.security.PrivateKey
import java.security.ProviderException
import java.security.PublicKey
import java.security.SecureRandom
import java.security.spec.MGF1ParameterSpec
import java.security.spec.X509EncodedKeySpec
import javax.crypto.Cipher
import javax.crypto.spec.GCMParameterSpec
import javax.crypto.spec.OAEPParameterSpec
import javax.crypto.spec.PSource
import javax.crypto.spec.SecretKeySpec
import kotlin.concurrent.thread

private const val KEYSTORE = "AndroidKeyStore"
private const val KEY_ALIAS = "hg_vault_record_key"
/** From the design that kept an AES key: removed with the old copies. */
private const val LEGACY_ALIAS = "hg_vault_wrapping_key"
private const val LEGACY_PREFS = "hg_secure_vault"
private const val AES_GCM = "AES/GCM/NoPadding"
private const val RSA_OAEP = "RSA/ECB/OAEPWithSHA-256AndMGF1Padding"
private const val RSA_BITS = 2048
private const val IV_BYTES = 12
private const val TAG_BITS = 128
private const val KEY_BYTES = 32

/** A prompt that has just passed covers the read that follows it, so the lock screen doesn't lead to a second one. */
private const val RECENT_PROMPT_SECONDS = 30

/**
 * The record cache's RSA key pair. The private key lives in StrongBox, or otherwise the Keystore's own hardware, and only
 * works after strong biometrics or the device PIN. Saving needs only the public key: each save makes a new AES key and
 * locks it with that, so nothing asks and no key is kept. Reading needs the private key, so the Keystore refuses it until
 * the user has just passed a prompt.
 */
class HgSecureVaultModule : Module() {
  private val context: Context
    get() = requireNotNull(appContext.reactContext)

  private val credentialsPerUse = Build.VERSION.SDK_INT >= Build.VERSION_CODES.R

  /** The Keystore wants these spelled out: its default for the mask digest differs from the provider's. */
  private val oaep =
    OAEPParameterSpec("SHA-256", "MGF1", MGF1ParameterSpec.SHA1, PSource.PSpecified.DEFAULT)

  override fun definition() = ModuleDefinition {
    Name("HgSecureVault")

    AsyncFunction("unlock") { prompt: String, subtitle: String, promise: Promise ->
      ask(prompt, subtitle, promise) { promise.resolve(null) }
    }.runOnQueue(Queues.MAIN)

    // the Keystore's time limit can't be ended early; it is short
    Function("lock") { }

    AsyncFunction("encrypt") { plaintext: String -> seal(plaintext) }

    AsyncFunction("decrypt") { ciphertext: String, prompt: String, subtitle: String, promise: Promise ->
      try {
        promise.resolve(open(ciphertext))
      } catch (error: UserNotAuthenticatedException) {
        val activity = appContext.currentActivity
        if (activity == null) {
          promise.reject("ERR_VAULT_UNAVAILABLE", "No activity to show the prompt", error)
        } else {
          activity.runOnUiThread {
            ask(prompt, subtitle, promise) { thread { openAfterPrompt(ciphertext, promise) } }
          }
        }
      } catch (error: Exception) {
        rejectOpen(error, promise)
      }
    }

    AsyncFunction("destroy") {
      deleteKeys()
    }
  }

  private fun openAfterPrompt(ciphertext: String, promise: Promise) {
    try {
      promise.resolve(open(ciphertext))
    } catch (error: Exception) {
      rejectOpen(error, promise)
    }
  }

  private fun rejectOpen(error: Exception, promise: Promise) {
    if (error is KeyPermanentlyInvalidatedException) {
      // the device's security was removed and set up again: the old key is gone for good
      deleteKeys()
      promise.reject("ERR_VAULT_INVALIDATED", "The key was invalidated", error)
    } else {
      promise.reject("ERR_VAULT_CRYPTO", error.message ?: "Could not decrypt", error)
    }
  }

  private fun keyStore() = KeyStore.getInstance(KEYSTORE).apply { load(null) }

  private fun deleteKeys() {
    context.getSharedPreferences(LEGACY_PREFS, Context.MODE_PRIVATE).edit().clear().apply()
    keyStore().apply {
      deleteEntry(LEGACY_ALIAS)
      deleteEntry(KEY_ALIAS)
    }
  }

  // Prompt

  private fun allowedAuthenticators() = BIOMETRIC_STRONG or DEVICE_CREDENTIAL

  private fun ask(prompt: String, subtitle: String, promise: Promise, passed: () -> Unit) {
    val activity = appContext.currentActivity as? FragmentActivity
    if (activity == null) {
      promise.reject("ERR_VAULT_UNAVAILABLE", "No activity to show the prompt", null)
      return
    }
    val canAuthenticate =
      if (credentialsPerUse) {
        BiometricManager.from(context).canAuthenticate(allowedAuthenticators()) ==
          BiometricManager.BIOMETRIC_SUCCESS
      } else {
        activity.getSystemService(KeyguardManager::class.java).isDeviceSecure
      }
    if (!canAuthenticate) {
      promise.reject("ERR_VAULT_UNAVAILABLE", "Device security is not set up", null)
      return
    }

    val info =
      BiometricPrompt.PromptInfo.Builder().setTitle(prompt).setSubtitle(subtitle).apply {
        if (credentialsPerUse) {
          setAllowedAuthenticators(allowedAuthenticators())
        } else {
          @Suppress("DEPRECATION")
          setDeviceCredentialAllowed(true)
        }
      }.build()

    val callback =
      object : BiometricPrompt.AuthenticationCallback() {
        override fun onAuthenticationSucceeded(result: BiometricPrompt.AuthenticationResult) {
          passed()
        }

        override fun onAuthenticationError(errorCode: Int, errString: CharSequence) {
          val cancelled =
            errorCode == BiometricPrompt.ERROR_USER_CANCELED ||
              errorCode == BiometricPrompt.ERROR_NEGATIVE_BUTTON ||
              errorCode == BiometricPrompt.ERROR_CANCELED ||
              errorCode == BiometricPrompt.ERROR_LOCKOUT ||
              errorCode == BiometricPrompt.ERROR_TIMEOUT
          promise.reject(
            if (cancelled) "ERR_VAULT_CANCELLED" else "ERR_VAULT_UNAVAILABLE",
            errString.toString(),
            null
          )
        }
      }

    BiometricPrompt(activity, ContextCompat.getMainExecutor(context), callback).authenticate(info)
  }

  // Records

  /** Key length, the locked AES key, the IV, then the records. */
  private fun seal(plaintext: String): String {
    val aesKey = ByteArray(KEY_BYTES).also { SecureRandom().nextBytes(it) }
    try {
      val rsa = Cipher.getInstance(RSA_OAEP)
      rsa.init(Cipher.ENCRYPT_MODE, publicKey(), oaep)
      val locked = rsa.doFinal(aesKey)
      val iv = ByteArray(IV_BYTES).also { SecureRandom().nextBytes(it) }
      val aes = Cipher.getInstance(AES_GCM)
      aes.init(Cipher.ENCRYPT_MODE, SecretKeySpec(aesKey, "AES"), GCMParameterSpec(TAG_BITS, iv))
      val sealed = aes.doFinal(plaintext.toByteArray())
      val out = ByteBuffer.allocate(Int.SIZE_BYTES + locked.size + iv.size + sealed.size)
      out.putInt(locked.size).put(locked).put(iv).put(sealed)
      return Base64.encodeToString(out.array(), Base64.NO_WRAP)
    } finally {
      aesKey.fill(0)
    }
  }

  /** Throws `UserNotAuthenticatedException` when the OS wants a prompt first. */
  private fun open(ciphertext: String): String {
    val input = ByteBuffer.wrap(Base64.decode(ciphertext, Base64.NO_WRAP))
    val locked = ByteArray(input.int).also { input.get(it) }
    val iv = ByteArray(IV_BYTES).also { input.get(it) }
    val sealed = ByteArray(input.remaining()).also { input.get(it) }
    val secret =
      keyStore().getKey(KEY_ALIAS, null) as? PrivateKey
        ?: throw GeneralSecurityException("There is no key")
    val rsa = Cipher.getInstance(RSA_OAEP)
    rsa.init(Cipher.DECRYPT_MODE, secret, oaep)
    val aesKey = rsa.doFinal(locked)
    try {
      val aes = Cipher.getInstance(AES_GCM)
      aes.init(Cipher.DECRYPT_MODE, SecretKeySpec(aesKey, "AES"), GCMParameterSpec(TAG_BITS, iv))
      return String(aes.doFinal(sealed))
    } finally {
      aesKey.fill(0)
    }
  }

  // Keys

  /** A plain copy of the public key: the Keystore's own object would hold it to the private key's rules. */
  private fun publicKey(): PublicKey {
    val stored = keyStore().getCertificate(KEY_ALIAS)?.publicKey ?: generateKey()
    return KeyFactory.getInstance(stored.algorithm)
      .generatePublic(X509EncodedKeySpec(stored.encoded))
  }

  private fun generateKey(): PublicKey {
    deleteKeys()
    // StrongBox is the dedicated security chip. Phones without one, or whose chip refuses, use the Keystore's own hardware.
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.P &&
      context.packageManager.hasSystemFeature(PackageManager.FEATURE_STRONGBOX_KEYSTORE)
    ) {
      try {
        return generateKeyPair(strongBox = true)
      } catch (error: GeneralSecurityException) {
        Log.i("HgSecureVault", "StrongBox unavailable: ${error.message}")
      } catch (error: ProviderException) {
        Log.i("HgSecureVault", "StrongBox unavailable: ${error.message}")
      }
    }
    return generateKeyPair(strongBox = false)
  }

  private fun generateKeyPair(strongBox: Boolean): PublicKey {
    val spec =
      KeyGenParameterSpec.Builder(KEY_ALIAS, KeyProperties.PURPOSE_DECRYPT)
        .setKeySize(RSA_BITS)
        .setDigests(KeyProperties.DIGEST_SHA256)
        .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_RSA_OAEP)
        .setUserAuthenticationRequired(true)
        .setInvalidatedByBiometricEnrollment(false)
        .apply {
          if (credentialsPerUse) {
            setUserAuthenticationParameters(
              RECENT_PROMPT_SECONDS,
              KeyProperties.AUTH_BIOMETRIC_STRONG or KeyProperties.AUTH_DEVICE_CREDENTIAL
            )
          } else {
            @Suppress("DEPRECATION")
            setUserAuthenticationValidityDurationSeconds(RECENT_PROMPT_SECONDS)
          }
          if (strongBox && Build.VERSION.SDK_INT >= Build.VERSION_CODES.P) {
            setIsStrongBoxBacked(true)
          }
        }
        .build()
    return KeyPairGenerator.getInstance(KeyProperties.KEY_ALGORITHM_RSA, KEYSTORE)
      .apply { initialize(spec) }
      .generateKeyPair()
      .public
  }
}
