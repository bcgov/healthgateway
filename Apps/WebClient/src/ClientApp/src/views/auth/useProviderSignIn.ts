import { useRouter } from "expo-router";
import { useState } from "react";
import { Alert } from "react-native";

import type { IdentityProviderConfiguration } from "@/models/configData";
import { isSessionEnded } from "@/models/errors";
import { toHref } from "@/navigation/href";
import { describeError, logger } from "@/services/logger";
import { useAuthStore } from "@/stores/auth.store";

import { completeLogin } from "./completeLogin";

/**
 * How a login with one provider ended, when it did not move on to the signed-in app: the person closed the sign-in
 * sheet, it failed, or the session was ended (signed out or reset) while it waited.
 */
export type SignInOutcome = "done" | "cancelled" | "failed" | "ended";

export const LOGIN_FAILED_MESSAGE =
    "An unexpected error occurred while processing the request, please try again.";

/**
 * Logs in with one provider and moves on to where the login leads. Web never comes back from `signIn` (a full-page
 * redirect); native carries on inline. Shared by the login page and the native login sheet.
 */
export function useProviderSignIn(redirectPath: string) {
    const router = useRouter();
    const signIn = useAuthStore((s) => s.signIn);
    const clearStorage = useAuthStore((s) => s.clearStorage);
    const [pending, setPending] = useState<string>();

    async function start(
        provider: IdentityProviderConfiguration
    ): Promise<SignInOutcome> {
        setPending(provider.id);
        try {
            await signIn(redirectPath, provider.hint || undefined);
            const target = await completeLogin(redirectPath);
            router.replace(toHref(target));
            return "done";
        } catch (error) {
            setPending(undefined);
            if (isSessionEnded(error)) return "ended";
            const cancelled =
                error instanceof Error && error.name === "AuthCancelledError";
            if (cancelled) {
                logger.debug("Login cancelled by the user");
            } else {
                logger.error(`Login failed: ${describeError(error)}`);
            }
            clearStorage();
            return cancelled ? "cancelled" : "failed";
        }
    }

    /** The id of the provider being logged in with, while it is under way. */
    return { start, pending };
}

/** Native: a failed login is the platform alert, offering to try the same provider again. */
export function alertLoginFailed(onRetry: () => void, onCancel: () => void) {
    Alert.alert(
        "Unable to log in",
        LOGIN_FAILED_MESSAGE,
        [
            { text: "Cancel", style: "cancel", onPress: onCancel },
            { text: "Try again", onPress: onRetry },
        ],
        // Android: back or a tap outside closes it, as Cancel does
        { cancelable: true, onDismiss: onCancel }
    );
}
