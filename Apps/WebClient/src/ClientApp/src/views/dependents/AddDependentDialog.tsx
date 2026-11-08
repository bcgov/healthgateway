import { useState } from "react";
import { Platform, View } from "react-native";

import { TooManyRequests } from "@/components/site/TooManyRequests";
import {
    AppText,
    Dialog,
    ExternalLink,
    HgAlert,
    HgButton,
    HgCheckbox,
    HgDatePicker,
    HgTextField,
    Loading,
} from "@/components/ui";
import { ActionType } from "@/constants/actionType";
import { TooManyRequestsLocation } from "@/constants/errorType";
import { ExternalUrl } from "@/constants/externalUrls";
import { Action, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import type { AddDependentRequest } from "@/models/dependent";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { fromIsoDate, today } from "@/utils/dateUtil";
import { maskPhn, validatePhn } from "@/utils/validation";

const BC_SERVICES_CARD_URL =
    "https://www2.gov.bc.ca/gov/content/governments/government-id/bc-services-card";

const emptyDependent: AddDependentRequest = {
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    PHN: "",
};

type Field = keyof AddDependentRequest;

/** The vuelidate rules of AddDependentComponent.vue; a field shows its first failed rule, as v-text-field does. */
export function dependentFieldError(
    field: Field,
    dependent: AddDependentRequest,
    maxDependentAge: number,
    existingPhns: string[]
): string | undefined {
    const value = dependent[field].trim();
    switch (field) {
        case "firstName":
            return value ? undefined : "Given names are required.";
        case "lastName":
            return value ? undefined : "Last name is required.";
        case "dateOfBirth": {
            if (value.length < 10) return "Invalid date.";
            const date = fromIsoDate(value);
            if (date < today().minus({ years: maxDependentAge }))
                return `Dependent must be under the age of ${maxDependentAge}.`;
            if (date > today()) return "Invalid Date.";
            return undefined;
        }
        case "PHN":
            if (value.length < 12) return "Valid PHN is required.";
            if (!validatePhn(value))
                return "Personal Health Number must be valid.";
            if (existingPhns.includes(value.replaceAll(/\s/g, "")))
                return "This dependent has already been added.";
            return undefined;
    }
}

interface Props {
    disabled?: boolean;
    onSubmit: () => void;
}

/** Port of AddDependentComponent.vue: the "Add dependent" button and its registration dialog. */
export function AddDependentDialog({ disabled = false, onSubmit }: Props) {
    const { xs } = useBreakpoint();
    const maxDependentAge = useConfigStore(
        (s) => selectWebConfig(s)?.maxDependentAge ?? 0
    );
    const dependents = useDependentStore((s) => s.dependents);
    const hdid = useUserStore((s) => s.user.hdid);

    const [isVisible, setIsVisible] = useState(false);
    const [dependent, setDependent] = useState(emptyDependent);
    const [accepted, setAccepted] = useState(false);
    const [touched, setTouched] = useState<Partial<Record<Field, boolean>>>({});
    const [errorMessage, setErrorMessage] = useState("");
    const [errorType, setErrorType] = useState<ActionType | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const existingPhns = dependents.map((d) => d.dependentInformation.PHN);
    const errorOf = (field: Field) =>
        dependentFieldError(field, dependent, maxDependentAge, existingPhns);
    const messages = (field: Field) => {
        const message = touched[field] ? errorOf(field) : undefined;
        return message ? [message] : [];
    };
    const touch = (field: Field) =>
        setTouched((t) => ({ ...t, [field]: true }));
    const change = (field: Field, value: string) =>
        setDependent((d) => ({ ...d, [field]: value }));

    const hasValidationErrors =
        !accepted ||
        (["firstName", "lastName", "dateOfBirth", "PHN"] as Field[]).some(
            (field) => errorOf(field) !== undefined
        );
    const isError = errorType !== null || errorMessage.length > 0;
    const isErrorProtected = errorType === ActionType.Protected;

    function hideDialog() {
        setDependent(emptyDependent);
        setAccepted(false);
        setErrorMessage("");
        setErrorType(null);
        setTouched({});
        setIsVisible(false);
    }

    async function handleSubmit() {
        setTouched({});
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.RegisterDependent,
            type: Type.Dependents,
        });
        setIsLoading(true);
        try {
            await services.dependent.addDependent(hdid, {
                ...dependent,
                firstName: dependent.firstName.trim().replaceAll(/\s+/g, " "),
                lastName: dependent.lastName.trim().replaceAll(/\s+/g, " "),
                PHN: dependent.PHN.replaceAll(/\D/g, ""),
            });
            setErrorType(null);
            hideDialog();
            onSubmit();
        } catch (error) {
            if (isSessionEnded(error)) return;
            const resultError =
                error instanceof ResultError ? error : undefined;
            if (resultError?.statusCode === 429) {
                useErrorStore
                    .getState()
                    .setTooManyRequestsError(
                        TooManyRequestsLocation.AddDependentDialog
                    );
            } else {
                setErrorMessage(
                    resultError?.message ??
                        (error instanceof Error ? error.message : String(error))
                );
                setErrorType(resultError?.actionCode ?? null);
            }
        } finally {
            setIsLoading(false);
        }
    }

    const fieldWidth = xs ? "w-full" : "w-1/2 px-2";

    return (
        <>
            <HgButton
                nativeID="add-dependent-button"
                testID="add-dependent-button"
                variant="secondary"
                icon="user-plus"
                text="Add dependent"
                requiresConnection
                disabled={disabled}
                onPress={() => {
                    services.tracking.trackEvent({
                        action: Action.ButtonClick,
                        text: Text.AddDependent,
                        type: Type.Dependents,
                    });
                    setIsVisible(true);
                }}
            />
            <Dialog
                visible={isVisible}
                title="Dependent Registration"
                tone="primary"
                persistent
                maxWidth={700}
                testID="add-dependent-dialog"
                closeTestID="add-dependent-dialog-close-button"
                closeNativeID="add-dependent-dialog-close-button"
                onRequestClose={hideDialog}
                actions={
                    <>
                        <HgButton
                            testID="cancel-dependent-registration-btn"
                            variant="secondary"
                            text="Cancel"
                            onPress={hideDialog}
                        />
                        <HgButton
                            testID="register-dependent-btn"
                            text="Register Dependent"
                            loading={isLoading}
                            disabled={hasValidationErrors}
                            onPress={() => void handleSubmit()}
                        />
                    </>
                }
            >
                <View testID="new-dependent-modal-form">
                    <TooManyRequests
                        location={TooManyRequestsLocation.AddDependentDialog}
                    />
                    {isError ? (
                        <HgAlert
                            type="error"
                            live="assertive"
                            testID="dependent-error-banner"
                        >
                            <AppText testID="dependent-error-text">
                                {errorType === ActionType.DataMismatch ? (
                                    "The information you entered does not match our records. Please try again with the exact given and last names on the BC Services Card."
                                ) : errorType === ActionType.NoHdId ? (
                                    <>
                                        Please ensure you are using a current{" "}
                                        <ExternalLink
                                            href={BC_SERVICES_CARD_URL}
                                        >
                                            BC Services Card
                                        </ExternalLink>
                                        .
                                    </>
                                ) : isErrorProtected ? (
                                    "Unable to add dependent."
                                ) : (
                                    errorMessage
                                )}
                            </AppText>
                            <AppText
                                testID={
                                    isErrorProtected
                                        ? "condensed-error-contact-message"
                                        : "not-condensed-error-contact-message"
                                }
                            >
                                {isErrorProtected
                                    ? "Please contact "
                                    : "If you continue to have issues, please contact "}
                                <ExternalLink
                                    href={ExternalUrl.HealthGatewayEmail}
                                >
                                    HealthGateway@gov.bc.ca
                                </ExternalLink>
                                .
                            </AppText>
                        </HgAlert>
                    ) : null}
                    <AppText className="font-bold mb-4">
                        Include all given names as shown on their BC Services
                        Card.
                    </AppText>
                    <View className={xs ? "" : "flex-row flex-wrap -mx-2"}>
                        <View className={fieldWidth}>
                            <HgTextField
                                nativeID="firstName"
                                testID="dependent-first-name-input"
                                label="First and Middle Names"
                                autoComplete="given-name"
                                autoFocus
                                clearable
                                autoCorrect={false}
                                value={dependent.firstName}
                                onChangeText={(v) => change("firstName", v)}
                                onBlur={() => touch("firstName")}
                                errorMessages={messages("firstName")}
                            />
                        </View>
                        <View className={fieldWidth}>
                            <HgTextField
                                nativeID="lastName"
                                testID="dependent-last-name-input"
                                label="Last Name"
                                autoComplete="family-name"
                                clearable
                                autoCorrect={false}
                                value={dependent.lastName}
                                onChangeText={(v) => change("lastName", v)}
                                onBlur={() => touch("lastName")}
                                errorMessages={messages("lastName")}
                            />
                        </View>
                        <View className={fieldWidth}>
                            <HgDatePicker
                                nativeID="dateOfBirth"
                                testID="dependent-date-of-birth-input"
                                label="Date of Birth"
                                // the HTML token would be "bday"; react-native-web passes this through, so off there
                                autoComplete={
                                    Platform.OS === "web"
                                        ? "off"
                                        : "birthdate-full"
                                }
                                maxDate={today()}
                                value={dependent.dateOfBirth}
                                onChange={(v) => {
                                    change("dateOfBirth", v);
                                    touch("dateOfBirth");
                                }}
                                onBlur={() => touch("dateOfBirth")}
                                errorMessages={messages("dateOfBirth")}
                            />
                        </View>
                        <View className={fieldWidth}>
                            <HgTextField
                                nativeID="phn"
                                testID="dependent-phn-input"
                                label="PHN"
                                clearable
                                keyboardType="number-pad"
                                value={dependent.PHN}
                                onChangeText={(v) => change("PHN", maskPhn(v))}
                                onBlur={() => touch("PHN")}
                                errorMessages={messages("PHN")}
                            />
                        </View>
                    </View>
                    <HgCheckbox
                        testID="dependent-terms-checkbox"
                        className="mt-2"
                        alignTop
                        checked={accepted}
                        onChange={setAccepted}
                        label="I declare that I am the child’s guardian"
                        labelContent={
                            <View className="gap-2">
                                <AppText>
                                    By providing the child’s name, date of
                                    birth, and personal health number, I declare
                                    that I am the child’s guardian and that I
                                    have the authority to request and receive
                                    health information respecting the child from
                                    third parties.
                                </AppText>
                                <AppText>
                                    If I either: (a) cease to be guardian of
                                    this child; (b) or lose the right to request
                                    or receive health information from third
                                    parties respecting this child, I will remove
                                    them as a dependent under my Health Gateway
                                    account immediately.
                                </AppText>
                                <AppText>
                                    I understand that I will no longer be able
                                    to access my child’s health records once
                                    they are 12 years of age.
                                </AppText>
                            </View>
                        }
                    />
                    <Loading isLoading={isLoading} />
                </View>
            </Dialog>
        </>
    );
}
