import { DateTime, Duration } from "luxon";
import { useState } from "react";
import { View } from "react-native";

import { AppText, HgAlert, HgButton } from "@/components/common";
import {
    ErrorSourceType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { Action, Text, Type } from "@/constants/tracking";
import { useNow } from "@/hooks/useNow";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import { Loader, useLoadingStore } from "@/stores/loading.store";
import { useUserStore } from "@/stores/user.store";

/** timeForDeletionString: largest whole unit above 1, else "imminently" */
export function describeTimeForDeletion(ms: number): string {
    const duration = Duration.fromMillis(ms);
    for (const unit of ["day", "hour", "minute", "second"] as const) {
        const amount = Math.floor(duration.as(unit));
        if (amount > 1) return `${amount} ${unit}s`;
    }
    return "Your account will be closed imminently";
}

/** Port of InactiveUserProfileComponent.vue */
export function InactiveProfile() {
    const closedDateTime = useUserStore((s) => s.user.closedDateTime);
    const recoverUserAccount = useUserStore((s) => s.recoverUserAccount);
    const hours = useConfigStore(
        (s) => selectWebConfig(s)?.hoursForDeletion ?? 720
    );
    const setTooMany = useErrorStore((s) => s.setTooManyRequestsError);
    const addCustomError = useErrorStore((s) => s.addCustomError);
    const applyLoader = useLoadingStore((s) => s.applyLoader);
    // the web client recalculates every second
    const now = useNow();
    const [submitting, setSubmitting] = useState(false);

    // timeForDeletion starts at -1 and is only recalculated when there is a closed date
    const timeForDeletion = closedDateTime
        ? DateTime.fromISO(closedDateTime).plus({ hours }).toMillis() - now
        : -1;

    function recoverAccount() {
        if (submitting) return;
        setSubmitting(true);
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.RecoverAccount,
            type: Type.Profile,
        });
        void applyLoader(
            Loader.UserProfile,
            "recoverAccount",
            recoverUserAccount()
                .catch((error: unknown) => {
                    if (isSessionEnded(error)) return;
                    logger.error(describeError(error));
                    if (
                        error instanceof ResultError &&
                        error.statusCode === 429
                    )
                        setTooMany(TooManyRequestsLocation.Page);
                    else
                        addCustomError(
                            `Unable to recover ${ErrorSourceType.Profile}`,
                            ErrorSourceType.Profile
                        );
                })
                .finally(() => setSubmitting(false))
        );
    }

    return (
        <View>
            <HgAlert
                testID="emailOptOutMessage"
                type="error"
                variant="text"
                title="Account marked for removal"
                text="Your account has been deactivated. If you wish to recover your account, click on the “Recover Account” button before the time expires."
            />
            <AppText testID="timeForDeletion">
                Time remaining for deletion:{" "}
                {describeTimeForDeletion(timeForDeletion)}
            </AppText>
            <View className="flex-row">
                <HgButton
                    testID="recoverAccountCancelBtn"
                    nativeID="recoverAccountCancelBtn"
                    text="Recover Account"
                    disabled={submitting}
                    onPress={recoverAccount}
                />
            </View>
        </View>
    );
}
