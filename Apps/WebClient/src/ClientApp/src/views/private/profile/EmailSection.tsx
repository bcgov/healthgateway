import { useState } from "react";
import { View } from "react-native";

import {
    FieldLabel,
    HgAlert,
    HgButton,
    HgTextField,
} from "@/components/common";
import { TooManyRequestsLocation } from "@/constants/errorType";
import { Action, Text, Type } from "@/constants/tracking";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { Loader, useLoadingStore } from "@/stores/loading.store";
import { useUserStore } from "@/stores/user.store";
import { isValidEmail } from "@/utils/validation";

import { StatusLine } from "./StatusLine";

/** Port of UserProfileEmailComponent.vue */
export function EmailSection({
    onEmailUpdated,
}: {
    onEmailUpdated: (email: string) => void;
}) {
    const email = useUserStore((s) => s.user.email);
    const verified = useUserStore((s) => s.user.verifiedEmail);
    const updateUserEmail = useUserStore((s) => s.updateUserEmail);
    const setTooMany = useErrorStore((s) => s.setTooManyRequestsError);
    const applyLoader = useLoadingStore((s) => s.applyLoader);

    const [editable, setEditable] = useState(false);
    const [input, setInput] = useState(email);
    const [touched, setTouched] = useState(false);
    const [updating, setUpdating] = useState(false);
    const [prevEmail, setPrevEmail] = useState(email);
    if (prevEmail !== email) {
        // store value changed (profile reloaded): resync the field, as the web client's watcher does
        setPrevEmail(email);
        setInput(email);
    }

    const errors: string[] = [];
    if (editable && touched) {
        if (input === email && input)
            errors.push("New email must be different from the previous one");
        if (!isValidEmail(input)) errors.push("Valid email is required");
    }
    const canSave = !updating && input !== email && isValidEmail(input);

    function send(value: string) {
        if (updating) return;
        setUpdating(true);
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.VerifyEmailAddress,
            type: Type.Profile,
        });
        void applyLoader(
            Loader.UserProfile,
            "updateUserEmail",
            updateUserEmail(value)
                .then(() => {
                    setEditable(false);
                    setTouched(false);
                    onEmailUpdated(useUserStore.getState().user.email);
                })
                .catch((error: unknown) => {
                    if (isSessionEnded(error)) return;
                    logger.error(describeError(error));
                    if (
                        error instanceof ResultError &&
                        error.statusCode === 429
                    )
                        setTooMany(TooManyRequestsLocation.Page);
                })
                .finally(() => setUpdating(false))
        );
    }

    return (
        <View>
            <FieldLabel
                title="Email Address"
                level={3}
                append={
                    // `invisible` while editing: keeps its space so the label does not shift
                    <HgButton
                        testID="editEmailBtn"
                        nativeID="editEmail"
                        text="Edit"
                        variant="link"
                        className={`ml-2 py-0 min-h-0 ${editable ? "opacity-0" : ""}`}
                        disabled={editable}
                        onPress={() => {
                            setEditable(true);
                            setTouched(true);
                        }}
                    />
                }
            />
            <View className="max-w-[400px]">
                <HgTextField
                    testID="email-input"
                    autoComplete="email"
                    value={input}
                    onChangeText={(v) => setInput(v.trim())}
                    onBlur={() => setTouched(true)}
                    editable={editable}
                    clearable={editable}
                    placeholder={editable ? "Your email address" : "Empty"}
                    label="Email Address"
                    floatingLabel
                    keyboardType="email-address"
                    autoCapitalize="none"
                    autoCorrect={false}
                    errorMessages={errors}
                    className={errors.length > 0 ? "mb-4" : "mb-5"}
                />
            </View>
            {editable ? (
                <View className="mb-4">
                    {!input && email ? (
                        <HgAlert
                            type="warning"
                            live="polite"
                            variant="text"
                            testID="emailOptOutMessage"
                            text="Removing your email address will disable future email communications from the Health Gateway."
                        />
                    ) : null}
                    <View className="flex-row">
                        <HgButton
                            testID="editEmailCancelBtn"
                            nativeID="editEmailCancelBtn"
                            text="Cancel"
                            variant="secondary"
                            className="mr-2"
                            onPress={() => {
                                setEditable(false);
                                setInput(email);
                                setTouched(false);
                            }}
                        />
                        <HgButton
                            testID="editEmailSaveBtn"
                            nativeID="editEmailSaveBtn"
                            text="Save"
                            className="mx-2"
                            disabled={!canSave}
                            onPress={() => {
                                setTouched(true);
                                if (canSave) send(input);
                            }}
                        />
                    </View>
                </View>
            ) : (
                <>
                    <StatusLine
                        verified={verified}
                        hasValue={Boolean(email)}
                        testIDPrefix="email"
                        nativeID="emailStatus"
                    />
                    {!verified && email ? (
                        <HgButton
                            testID="resendEmailBtn"
                            nativeID="resendEmail"
                            text="Resend Verification"
                            variant="secondary"
                            className="mb-4"
                            disabled={updating}
                            onPress={() => send(email)}
                        />
                    ) : null}
                </>
            )}
        </View>
    );
}
