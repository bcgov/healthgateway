import { requireOptionalNativeModule } from "expo-modules-core";

/** The native vault: see `ios/HgSecureVaultModule.swift` and the Android module. Null where it is not built in. */
export interface HgSecureVaultNative {
    /** Asks for biometrics or the device PIN / passcode. A pass also covers decrypting for the next 30 seconds. */
    unlock(prompt: string, subtitle: string): Promise<void>;
    /** Ends that 30 seconds where the OS allows it (iOS). Android's Keystore lets it run out. */
    lock(): void;
    /** A new AES-256-GCM key for each call, locked with the public key: never asks, and no key is kept. */
    encrypt(plaintext: string): Promise<string>;
    /** Needs the private key: the OS asks unless a prompt has just passed. */
    decrypt(
        ciphertext: string,
        prompt: string,
        subtitle: string
    ): Promise<string>;
    /** Deletes the key pair for good, which makes everything encrypted with it unreadable. */
    destroy(): Promise<void>;
}

export default requireOptionalNativeModule<HgSecureVaultNative>(
    "HgSecureVault"
);
