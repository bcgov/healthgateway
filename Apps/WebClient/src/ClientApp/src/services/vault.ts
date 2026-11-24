import { Platform } from "react-native";

import { logger } from "@/services/logger";

import NativeVault from "../../modules/hg-secure-vault";

/**
 * "cancelled": the prompt was turned down or failed. "keyGone": the device has no security set up any more, and the key
 * went with it. "unavailable": anything else, which may pass at the next try.
 */
export type VaultUnlock = "unlocked" | "cancelled" | "unavailable" | "keyGone";

const errorCode = (error: unknown) =>
    (error as { code?: string } | undefined)?.code;

const PROMPT =
    Platform.OS === "ios"
        ? "To keep your data safe, you must enter your device passcode."
        : "Health Gateway";
const PROMPT_SUBTITLE = "Log in using your biometric credential";

/** The user cancelled or failed the prompt, as opposed to the key or file being no good. */
export const isVaultCancelled = (error: unknown) =>
    errorCode(error) === "ERR_VAULT_CANCELLED";

/**
 * The key is gone for good, and nothing it encrypted can be read again: the device's security was removed, or removed and
 * set up again. Any other failure may pass at the next try.
 */
export const isVaultKeyGone = (error: unknown) => {
    const code = errorCode(error);
    return code === "ERR_VAULT_NO_KEY" || code === "ERR_VAULT_INVALIDATED";
};

/**
 * The device-protected key pair (`modules/hg-secure-vault`), in native code: JavaScript only hands over text. Encrypting
 * never asks and no key is kept in memory; decrypting makes the OS ask for biometrics or the device PIN / passcode, unless
 * `unlock` has just passed. A dev client built before the module was added has none, which reads as "unavailable".
 */
export const vault = {
    async unlock(): Promise<VaultUnlock> {
        if (!NativeVault) return "unavailable";
        try {
            await NativeVault.unlock(PROMPT, PROMPT_SUBTITLE);
            return "unlocked";
        } catch (error) {
            logger.warn(`Vault did not unlock: ${errorCode(error)}`);
            if (isVaultCancelled(error)) return "cancelled";
            return isVaultKeyGone(error) ? "keyGone" : "unavailable";
        }
    },
    lock: () => NativeVault?.lock(),
    encrypt: (text: string) => {
        if (!NativeVault) throw new Error("No vault");
        return NativeVault.encrypt(text);
    },
    decrypt: (text: string) => {
        if (!NativeVault) throw new Error("No vault");
        return NativeVault.decrypt(text, PROMPT, PROMPT_SUBTITLE);
    },
    destroy: async () => {
        await NativeVault?.destroy();
    },
    /** iOS: keeps the record cache's folder out of backups. A dev client built before this was added skips it. */
    excludeFromBackup: async (uri: string) => {
        await NativeVault?.excludeFromBackup?.(uri);
    },
};
