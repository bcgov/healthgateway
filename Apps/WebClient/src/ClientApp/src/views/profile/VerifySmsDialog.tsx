import { DateTime } from "luxon";
import { useEffect, useRef, useState } from "react";
import { Platform, View } from "react-native";

import { TooManyRequests } from "@/components/site/TooManyRequests";
import {
    AppText,
    Dialog,
    HgAlert,
    HgButton,
    HgTextField,
    Loading,
} from "@/components/ui";
import {
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { useNow } from "@/hooks/useNow";
import {
    isSessionEnded,
    isTooManyRequestsError,
    ResultError,
} from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { maskPhone } from "@/utils/validation";

interface Props {
    visible: boolean;
    smsNumber: string;
    onClose: () => void;
    onVerified: () => void;
}

/** Port of VerifySmsDialogComponent.vue */
export function VerifySmsDialog({
    visible,
    smsNumber,
    onClose,
    onVerified,
}: Props) {
    const hdid = useUserStore((s) => s.user.hdid);
    const resendIso = useUserStore((s) => s.smsResendDateTime);
    const setResend = useUserStore((s) => s.setSmsResendDateTime);
    const resendMinutes = useConfigStore(
        (s) => selectWebConfig(s)?.timeouts.resendSMS ?? 1
    );
    const addError = useErrorStore((s) => s.addError);
    const setTooManyError = useErrorStore((s) => s.setTooManyRequestsError);
    const setTooManyWarning = useErrorStore((s) => s.setTooManyRequestsWarning);

    const [code, setCode] = useState("");
    const [loading, setLoading] = useState(false);
    const [validationError, setValidationError] = useState(false);
    const [unexpectedError, setUnexpectedError] = useState(false);
    const [sent, setSent] = useState(false);
    const now = useNow(visible);
    // the "sent" guard resets after 5 s; cleared on unmount so no timer outlives the dialog
    const sentTimer = useRef<ReturnType<typeof setTimeout> | undefined>(
        undefined
    );
    useEffect(() => () => clearTimeout(sentTimer.current), []);

    const resendAt = resendIso
        ? DateTime.fromISO(resendIso).plus({ minutes: resendMinutes })
        : undefined;
    const msLeft = resendAt ? Math.max(0, resendAt.toMillis() - now) : 0;
    const allowRetry = msLeft === 0;

    // showDialog(): send the code when the dialog opens with no resend timestamp yet
    useEffect(() => {
        if (visible && resendIso === undefined)
            void Promise.resolve().then(sendUserSmsUpdate);
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [visible]);

    function sendUserSmsUpdate() {
        if (sent) return;
        setSent(true);
        services.userProfile
            .updateSmsNumber(hdid, smsNumber)
            .then(() => {
                setResend(DateTime.now().toISO());
                clearTimeout(sentTimer.current);
                sentTimer.current = setTimeout(() => setSent(false), 5000);
            })
            .catch((error: unknown) => {
                if (isSessionEnded(error)) return;
                setSent(false);
                if (isTooManyRequestsError(error))
                    setTooManyWarning(TooManyRequestsLocation.Page);
                else addError(ErrorType.Update, ErrorSourceType.Profile);
            });
    }

    function verifySms(value: string) {
        if (loading) return;
        const digits = value.replace(/\D/g, "");
        setLoading(true);
        services.userProfile
            .validateSms(hdid, digits)
            .then((ok) => {
                setValidationError(!ok);
                if (ok) {
                    onClose();
                    onVerified();
                }
            })
            .catch((error: unknown) => {
                if (isSessionEnded(error)) return;
                logger.error(describeError(error));
                if (error instanceof ResultError && error.statusCode === 429)
                    setTooManyError(TooManyRequestsLocation.VerifySmsModal);
                else setUnexpectedError(true);
            })
            .finally(() => {
                setCode("");
                setLoading(false);
            });
    }

    const minutes = Math.floor(msLeft / 60000);
    const seconds = Math.ceil((msLeft % 60000) / 1000);

    return (
        <>
            <Loading isLoading={loading} />
            <Dialog
                visible={visible}
                title="Phone Verification"
                tone="primary"
                persistent
                maxWidth={300}
                closeTestID="messageModalCloseButton"
                actionsAlign={unexpectedError ? "end" : "center"}
                onRequestClose={onClose}
                testID="verifySMSModal"
                actions={
                    unexpectedError ? (
                        <HgButton text="OK" onPress={onClose} />
                    ) : !allowRetry ? (
                        <AppText className="text-center">
                            Your code has been sent. You can resend after{" "}
                            <AppText testID="countdownText">
                                {minutes > 0 ? `${minutes}m ` : ""}
                                {seconds}s.
                            </AppText>
                        </AppText>
                    ) : (
                        <View className="flex-row items-center">
                            <AppText>Didn&apos;t receive a code?</AppText>
                            <HgButton
                                nativeID="resendSMSVerification"
                                testID="resendSMSVerification"
                                text="Resend"
                                variant="link"
                                className="ml-2"
                                disabled={sent}
                                onPress={sendUserSmsUpdate}
                            />
                        </View>
                    )
                }
            >
                <TooManyRequests
                    location={TooManyRequestsLocation.VerifySmsModal}
                />
                {unexpectedError ? (
                    <HgAlert
                        testID="verifySMSModalUnexpectedErrorText"
                        type="error"
                        live="assertive"
                        variant="text"
                        text="An unexpected error has occurred. Please try refreshing your browser or try again later."
                    />
                ) : (
                    <View testID="verifySMSModalText">
                        <AppText className="text-center">
                            Enter the verification code sent to{" "}
                            <AppText className="font-bold">
                                {maskPhone(smsNumber)}
                            </AppText>
                        </AppText>
                        <HgTextField
                            testID="verifySMSModalCodeInput"
                            label="Verification Code"
                            autoComplete={Platform.select({
                                android: "sms-otp",
                                default: "one-time-code",
                            })}
                            value={code}
                            autoFocus
                            maxLength={6}
                            keyboardType="number-pad"
                            editable={!loading}
                            errorMessages={
                                validationError
                                    ? ["Invalid verification code. Try again."]
                                    : []
                            }
                            onChangeText={(v) => {
                                setCode(v);
                                if (v.length >= 6) verifySms(v);
                            }}
                            className="w-full mt-3"
                        />
                    </View>
                )}
            </Dialog>
        </>
    );
}
