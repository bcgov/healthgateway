import { useState } from "react";
import { View } from "react-native";

import {
    CardActions,
    HgAlert,
    HgButton,
    SectionHeading,
} from "@/components/common";
import {
    ErrorSourceType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { Action, Text, Type } from "@/constants/tracking";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { Loader, useLoadingStore } from "@/stores/loading.store";
import { useUserStore } from "@/stores/user.store";

/** Port of UserProfileManageAccountComponent.vue */
export function ManageAccountSection() {
    const closeUserAccount = useUserStore((s) => s.closeUserAccount);
    const setTooMany = useErrorStore((s) => s.setTooManyRequestsError);
    const addCustomError = useErrorStore((s) => s.addCustomError);
    const applyLoader = useLoadingStore((s) => s.applyLoader);
    const [showCloseWarning, setShowCloseWarning] = useState(false);
    const [closing, setClosing] = useState(false);

    function closeAccount() {
        if (closing) return;
        setClosing(true);
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.DeleteAccount,
            type: Type.Profile,
        });
        void applyLoader(
            Loader.UserProfile,
            "closeAccount",
            closeUserAccount()
                .then(() => setShowCloseWarning(false))
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
                            `Unable to close ${ErrorSourceType.Profile}`,
                            ErrorSourceType.Profile
                        );
                })
                .finally(() => setClosing(false))
        );
    }

    return (
        <View className="mb-6">
            <SectionHeading title="Manage Account" includeDivider level={2} />
            {!showCloseWarning ? (
                <View className="flex-row">
                    <HgButton
                        testID="recoverAccountShowCloseWarningBtn"
                        nativeID="recoverAccountShowCloseWarningBtn"
                        text="Delete My Account"
                        variant="error-secondary"
                        onPress={() => setShowCloseWarning(true)}
                    />
                </View>
            ) : (
                // v-card variant="text" class="mt-n4": text (pa-4) then actions (pa-2, spacer, buttons)
                <View className="-mt-4">
                    <View className="p-4">
                        <HgAlert
                            type="error"
                            variant="text"
                            text="Your account will be marked for removal, preventing you from accessing your information on the Health Gateway. After a set period of time it will be removed permanently."
                        />
                    </View>
                    <View className="flex-row justify-end p-2">
                        <CardActions>
                            <HgButton
                                testID="closeAccountCancelBtn"
                                nativeID="closeAccountCancelBtn"
                                text="Cancel"
                                variant="secondary"
                                className="mr-2"
                                onPress={() => setShowCloseWarning(false)}
                            />
                            <HgButton
                                testID="closeAccountBtn"
                                nativeID="closeAccountBtn"
                                text="Delete Account"
                                variant="error"
                                disabled={closing}
                                onPress={closeAccount}
                            />
                        </CardActions>
                    </View>
                </View>
            )}
        </View>
    );
}
