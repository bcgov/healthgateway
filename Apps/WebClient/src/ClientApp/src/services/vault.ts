import { Platform } from "react-native";

import { logger } from "@/services/logger";

import NativeVault from "../../modules/hg-secure-vault";

export type VaultUnlock = "unlocked" | "cancelled" | "unavailable";

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
            return isVaultCancelled(error) ? "cancelled" : "unavailable";
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
};
