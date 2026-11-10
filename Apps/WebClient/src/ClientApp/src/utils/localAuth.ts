import { Platform } from "react-native";

import { describeError, logger } from "@/services/logger";

/** What the device can unlock with right now. "none": no passcode or biometrics are set up. */
export type LocalAuthMethod =
    "faceId" | "touchId" | "biometrics" | "passcode" | "none";

/**
 * Which method the system prompt will offer. Android does not say which biometric its prompt will use (fingerprint,
 * face, iris), so it reads "biometrics", as the old Android app did. Native only; lazy so a dev client built before
 * the module was added fails softly (AGENTS.md).
 */
export async function getLocalAuthMethod(): Promise<LocalAuthMethod> {
    if (Platform.OS === "web") return "none";
    try {
        const localAuth = await import("expo-local-authentication");
        const level = await localAuth.getEnrolledLevelAsync();
        if (level === localAuth.SecurityLevel.NONE) return "none";
        if (level === localAuth.SecurityLevel.SECRET) return "passcode";
        if (Platform.OS !== "ios") return "biometrics";
        const types = await localAuth.supportedAuthenticationTypesAsync();
        return types.includes(localAuth.AuthenticationType.FACIAL_RECOGNITION)
            ? "faceId"
            : "touchId";
    } catch (error) {
        logger.warn(
            `Local authentication unavailable: ${describeError(error)}`
        );
        return "none";
    }
}
