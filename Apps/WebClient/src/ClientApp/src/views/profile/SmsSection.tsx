import { DateTime } from "luxon";
import { useEffect, useState } from "react";
import { ActivityIndicator, View } from "react-native";

import { FieldLabel, HgAlert, HgButton, HgTextField } from "@/components/ui";
import {
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { Action, Text, Type } from "@/constants/tracking";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, isTooManyRequestsError } from "@/models/errors";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { Loader, useLoadingStore } from "@/stores/loading.store";
import { useUserStore } from "@/stores/user.store";
import {
    maskPhone,
    PhoneNumberMaskedRegex,
    stripPhoneMask,
} from "@/utils/validation";

import { StatusLine } from "./StatusLine";
import { VerifySmsDialog } from "./VerifySmsDialog";

/** Port of UserProfileSmsComponent.vue */
export function SmsSection() {
    const colors = useThemeColors();
    const storeNumber = useUserStore((s) => s.user.sms);
    const verified = useUserStore((s) => s.user.verifiedSms);
    const email = useUserStore((s) => s.user.email);
    const hdid = useUserStore((s) => s.user.hdid);
    const retrieveProfile = useUserStore((s) => s.retrieveProfile);
    const setResend = useUserStore((s) => s.setSmsResendDateTime);
    const addError = useErrorStore((s) => s.addError);
    const setTooManyError = useErrorStore((s) => s.setTooManyRequestsError);
    const setTooManyWarning = useErrorStore((s) => s.setTooManyRequestsWarning);
    const applyLoader = useLoadingStore((s) => s.applyLoader);

    const [editable, setEditable] = useState(false);
    const [masked, setMasked] = useState(maskPhone(storeNumber));
    const [touched, setTouched] = useState(false);
    const [updating, setUpdating] = useState(false);
    const [serverCheck, setServerCheck] = useState<{
        digits: string;
        ok: boolean;
    }>();
    const [showVerify, setShowVerify] = useState(false);
    const [showVerifiedMessage, setShowVerifiedMessage] = useState(false);

    // resync with the store when the profile reloads (web client watchers)
    const [prevStoreNumber, setPrevStoreNumber] = useState(storeNumber);
    if (prevStoreNumber !== storeNumber) {
        setPrevStoreNumber(storeNumber);
        setMasked(maskPhone(storeNumber));
    }
    const [prevEmail, setPrevEmail] = useState(email);
    if (prevEmail !== email) {
        setPrevEmail(email);
        setShowVerifiedMessage(false);
    }

    const digits = stripPhoneMask(masked);
    const needsServerCheck =
        editable && Boolean(digits) && digits !== storeNumber;
    const formatOk = PhoneNumberMaskedRegex.test(digits);
    // the vuelidate withAsync validator: undefined while the server check for these digits is pending
    const serverValid: boolean | undefined = !needsServerCheck
        ? undefined
        : !formatOk
          ? false
          : serverCheck?.digits === digits
            ? serverCheck.ok
            : undefined;
    const pending = needsServerCheck && formatOk && serverValid === undefined;

    useEffect(() => {
        if (!needsServerCheck || !formatOk) return;
        let cancelled = false;
        // PromiseUtility.withMinimumDelay(…, 500)
        const t = setTimeout(() => {
            services.userProfile
                .isPhoneNumberValid(digits)
                .then((ok) => !cancelled && setServerCheck({ digits, ok }))
                .catch(
                    () => !cancelled && setServerCheck({ digits, ok: false })
                );
        }, 500);
        return () => {
            cancelled = true;
            clearTimeout(t);
        };
    }, [digits, needsServerCheck, formatOk]);

    const errors: string[] = [];
    if (editable && touched) {
        if (digits === storeNumber && digits)
            errors.push(
                "New SMS number must be different from the previous one"
            );
        if (digits && serverValid === false)
            errors.push("Valid SMS number is required");
    }
    const isValid = !digits || digits === storeNumber || serverValid === true;
    const canSave = !updating && digits !== storeNumber && isValid;

    function refreshProfile(): Promise<void> {
        return retrieveProfile().catch((error: unknown) => {
            if (isSessionEnded(error)) return;
            if (isTooManyRequestsError(error))
                setTooManyWarning(TooManyRequestsLocation.Page);
            else addError(ErrorType.Retrieve, ErrorSourceType.Profile);
        });
    }

    function openVerify() {
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.VerifyMobileNumber,
            type: Type.Profile,
        });
        setShowVerify(true);
    }

    function updateSms() {
        if (updating) return;
        setUpdating(true);
        // reset the resend timer when the user submits their SMS number
        setResend(DateTime.now().toISO());
        void applyLoader(
            Loader.UserProfile,
            "updateSms",
            services.userProfile
                .updateSmsNumber(hdid, digits)
                .then(refreshProfile)
                .then(() => {
                    setEditable(false);
                    setTouched(false);
                    if (useUserStore.getState().user.sms) openVerify();
                })
                .catch((error: unknown) => {
                    if (isSessionEnded(error)) return;
                    if (isTooManyRequestsError(error))
                        setTooManyError(TooManyRequestsLocation.Page);
                    else addError(ErrorType.Update, ErrorSourceType.Profile);
                })
                .finally(() => setUpdating(false))
        );
    }

    return (
        <View>
            <FieldLabel
                title="Cell Number (SMS notifications)"
                level={3}
                append={
                    <HgButton
                        testID="editSMSBtn"
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
                    testID="smsNumberInput"
                    label="Cell Number"
                    autoComplete="tel"
                    floatingLabel
                    value={masked}
                    onChangeText={(v) => setMasked(maskPhone(v))}
                    editable={editable}
                    clearable={editable}
                    placeholder={editable ? "Your phone number" : "Empty"}
                    keyboardType="phone-pad"
                    maxLength={14}
                    errorMessages={errors}
                    className={errors.length > 0 ? "mb-4" : "mb-5"}
                    appendInner={
                        pending ? (
                            <ActivityIndicator
                                testID="smsNumberPending"
                                size="small"
                                color={colors.info}
                            />
                        ) : null
                    }
                />
            </View>
            {editable ? (
                <View className="mb-4">
                    {!digits && storeNumber ? (
                        <HgAlert
                            type="warning"
                            live="polite"
                            variant="text"
                            testID="smsOptOutMessage"
                            text="Removing your phone number will disable future SMS communications from the Health Gateway."
                        />
                    ) : null}
                    <View className="flex-row">
                        <HgButton
                            testID="cancelSMSEditBtn"
                            text="Cancel"
                            variant="secondary"
                            className="mr-2"
                            onPress={() => {
                                setEditable(false);
                                setMasked(maskPhone(storeNumber));
                                setTouched(false);
                            }}
                        />
                        <HgButton
                            testID="saveSMSEditBtn"
                            text="Save"
                            className="mx-2"
                            disabled={!canSave}
                            onPress={() => {
                                setTouched(true);
                                if (canSave) updateSms();
                            }}
                        />
                    </View>
                </View>
            ) : (
                <>
                    <StatusLine
                        verified={verified}
                        hasValue={Boolean(storeNumber)}
                        testIDPrefix="sms"
                    />
                    {showVerifiedMessage && verified ? (
                        <HgAlert
                            type="success"
                            live="polite"
                            variant="text"
                            testID="verified-sms-message"
                            text="Health Gateway may now send you notifications. You can change your preferences at any time."
                        />
                    ) : null}
                    {!verified && storeNumber ? (
                        <View className="mb-4 flex-row">
                            <HgButton
                                testID="verifySMSBtn"
                                text="Verify"
                                variant="secondary"
                                onPress={openVerify}
                            />
                        </View>
                    ) : null}
                </>
            )}
            <VerifySmsDialog
                visible={showVerify}
                smsNumber={storeNumber}
                onClose={() => setShowVerify(false)}
                onVerified={() => {
                    setShowVerifiedMessage(true);
                    void applyLoader(
                        Loader.UserProfile,
                        "verifySms",
                        refreshProfile()
                    );
                }}
            />
        </View>
    );
}
