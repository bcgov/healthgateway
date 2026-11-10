import { useRouter } from "expo-router";
import { useEffect, useState } from "react";
import { View } from "react-native";

import {
    AppText,
    HgButton,
    HgCheckbox,
    HgTextField,
    Icon,
    PageTitle,
} from "@/components/common";
import { Loading } from "@/components/site/Loading";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, ResultError } from "@/models/errors";
import type { TermsOfService } from "@/models/userProfile";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { openExternal } from "@/utils/openExternal";
import {
    EmailRegex,
    maskPhone,
    PhoneNumberMaskedRegex,
    stripPhoneMask,
} from "@/utils/validation";
import { PageError } from "@/views/errors/PageError";

import { TermsOfServiceBox } from "./TermsOfServiceBox";

/** Port of RegistrationView.vue */
export function RegistrationView() {
    const colors = useThemeColors();
    const router = useRouter();
    const oidcUserInfo = useUserStore((s) => s.oidcUserInfo);
    const createProfile = useUserStore((s) => s.createProfile);
    const minPatientAge = useConfigStore(
        (s) => selectWebConfig(s)?.minPatientAge
    );
    const errorStore = useErrorStore();

    const [accepted, setAccepted] = useState(false);
    const [email, setEmail] = useState(oidcUserInfo?.email ?? "");
    const [emailConfirmation, setEmailConfirmation] = useState(
        oidcUserInfo?.email ?? ""
    );
    const [smsMasked, setSmsMasked] = useState("");
    const [isEmailChecked, setIsEmailChecked] = useState(true);
    const [isSmsChecked, setIsSmsChecked] = useState(true);
    const [touched, setTouched] = useState(false);
    // no OIDC user info → nothing to validate, so not loading
    const [loadingUserData, setLoadingUserData] = useState(
        oidcUserInfo !== undefined
    );
    const [loadingTerms, setLoadingTerms] = useState(true);
    const [submitting, setSubmitting] = useState(false);
    const [clientRegistryError, setClientRegistryError] = useState(false);
    const [isValidAge, setIsValidAge] = useState<boolean | null>(null);
    const [termsOfService, setTermsOfService] = useState<TermsOfService>();
    const [smsServerValid, setSmsServerValid] = useState<{
        digits: string;
        ok: boolean;
    }>();

    const sms = stripPhoneMask(smsMasked);
    const isLoading = loadingTerms || loadingUserData || submitting;

    // loadUserData(): age validation against the Client Registry
    useEffect(() => {
        if (!oidcUserInfo) {
            errorStore.addError(ErrorType.Retrieve, ErrorSourceType.User);
            return;
        }
        services.userProfile
            .validateAge(oidcUserInfo.hdid ?? "")
            .then(setIsValidAge)
            .catch((error) => {
                if (isSessionEnded(error)) return;
                if (error instanceof ResultError && error.statusCode === 429)
                    errorStore.setTooManyRequestsError();
                else {
                    setClientRegistryError(true);
                    errorStore.addCustomError(
                        `Unable to validate ${ErrorSourceType.User.toLowerCase()}`,
                        ErrorSourceType.User
                    );
                }
            })
            .finally(() => setLoadingUserData(false));
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // loadTermsOfService()
    useEffect(() => {
        services.userProfile
            .getTermsOfService()
            .then((result) => {
                logger.debug("getTermsOfService result");
                setTermsOfService(result);
            })
            .catch((error) => {
                if (isSessionEnded(error)) return;
                logger.error(describeError(error));
                if (error instanceof ResultError && error.statusCode === 429)
                    errorStore.setTooManyRequestsWarning();
                else
                    errorStore.addError(
                        ErrorType.Retrieve,
                        ErrorSourceType.TermsOfService
                    );
            })
            .finally(() => setLoadingTerms(false));
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // async SMS validation (IsValidPhoneNumber), debounced
    useEffect(() => {
        if (!isSmsChecked || !sms || !PhoneNumberMaskedRegex.test(sms)) return;
        let cancelled = false;
        const t = setTimeout(() => {
            services.userProfile
                .isPhoneNumberValid(sms)
                .then(
                    (ok) => !cancelled && setSmsServerValid({ digits: sms, ok })
                )
                .catch(
                    () =>
                        !cancelled &&
                        setSmsServerValid({ digits: sms, ok: false })
                );
        }, 400);
        return () => {
            cancelled = true;
            clearTimeout(t);
        };
    }, [sms, isSmsChecked]);

    // validations (vuelidate rules)
    const emailErrors: string[] = [];
    const emailConfirmationErrors: string[] = [];
    const smsErrors: string[] = [];
    if (touched && isEmailChecked) {
        if (!email) emailErrors.push("Value is required");
        else if (!EmailRegex.test(email)) emailErrors.push("Invalid email");
        if (!emailConfirmation)
            emailConfirmationErrors.push("Value is required");
        else if (emailConfirmation !== email)
            emailConfirmationErrors.push("Both email addresses must match");
        else if (!EmailRegex.test(emailConfirmation))
            emailConfirmationErrors.push("Invalid email");
    }
    const smsFormatOk = !sms || PhoneNumberMaskedRegex.test(sms);
    const smsValid =
        !isSmsChecked ||
        (sms.length > 0 &&
            smsFormatOk &&
            smsServerValid?.digits === sms &&
            smsServerValid.ok);
    if (touched && isSmsChecked) {
        if (!sms) smsErrors.push("Value is required");
        else if (
            !smsFormatOk ||
            (smsServerValid?.digits === sms && !smsServerValid.ok)
        )
            smsErrors.push("Invalid phone number");
    }
    const emailValid =
        !isEmailChecked ||
        (EmailRegex.test(email) && emailConfirmation === email);

    async function onSubmit() {
        setTouched(true);
        // vuelidate's $validate waits for the phone check, which may still be debouncing
        let smsIsValid = smsValid;
        if (
            isSmsChecked &&
            sms &&
            smsFormatOk &&
            smsServerValid?.digits !== sms
        ) {
            smsIsValid = await services.userProfile
                .isPhoneNumberValid(sms)
                .catch(() => false);
            setSmsServerValid({ digits: sms, ok: smsIsValid });
        }
        if (!emailValid || !smsIsValid || !accepted || !oidcUserInfo) return;
        setSubmitting(true);
        try {
            await createProfile({
                hdid: oidcUserInfo.hdid ?? "",
                termsOfServiceId: termsOfService?.id ?? "",
                acceptedTermsOfService: accepted,
                email: isEmailChecked ? email : "",
                isEmailVerified: false,
                smsNumber: isSmsChecked ? sms : "",
                isSMSNumberVerified: false,
                preferences: {},
                lastLoginDateTimes: [],
                notificationSettings: [],
            });
            router.replace({
                pathname: Path.Home,
                params: { registration: "success" },
            });
        } catch {
            logger.error("Error while registering.");
        } finally {
            setSubmitting(false);
        }
    }

    const logoutButton = (
        <HgButton
            testID="registration-logout-button"
            variant="secondary"
            icon="right-from-bracket"
            text="Log Out"
            className="mt-4"
            onPress={() => router.push(Path.Logout)}
        />
    );

    if (isLoading) {
        return (
            <View className="min-h-[320px]">
                <Loading isLoading />
            </View>
        );
    }
    if (!termsOfService?.content) {
        return <AppText variant="h1">Unknown error</AppText>;
    }
    if (isValidAge === false) {
        return (
            <PageError title="Minimum age required for registration">
                <AppText
                    testID="minimumAgeErrorText"
                    className="mt-1 mb-4 text-center"
                >
                    You must be{" "}
                    <AppText className="font-bold">
                        {String(minPatientAge ?? "")}
                    </AppText>{" "}
                    years of age or older to use this application.
                </AppText>
                {logoutButton}
            </PageError>
        );
    }
    if (clientRegistryError) {
        return (
            <PageError title="Error retrieving user information">
                <AppText
                    testID="clientRegistryErrorText"
                    className="mt-1 mb-4 text-center"
                >
                    There may be an issue in our Client Registry. Please contact{" "}
                    <AppText
                        className="text-link font-bold"
                        onPress={() =>
                            void openExternal("mailto:HealthGateway@gov.bc.ca")
                        }
                    >
                        HealthGateway@gov.bc.ca
                    </AppText>
                </AppText>
                {logoutButton}
            </PageError>
        );
    }
    if (isValidAge !== true) {
        return <AppText variant="h1">Unknown error</AppText>;
    }

    return (
        <View>
            <PageTitle title="Registration" />
            <AppText variant="title-large" className="text-bc-grey mb-3">
                Communication Preferences (Optional)
            </AppText>
            <AppText variant="body-medium" className="mb-4 text-bc-grey">
                Choose how you would like to receive updates. Preferences can be
                changed anytime in your profile.
            </AppText>

            <HgCheckbox
                testID="emailCheckbox"
                label="Email Notifications"
                checked={isEmailChecked}
                onChange={(v) => {
                    setIsEmailChecked(v);
                    if (!v) {
                        setEmail("");
                        setEmailConfirmation("");
                    }
                }}
            />
            <AppText variant="body-medium" className="italic mb-2">
                Enter your email to receive updates when available.
            </AppText>
            <HgTextField
                testID="emailInput"
                accessibilityLabel="Email address"
                autoComplete="email"
                value={email}
                onChangeText={(v) => {
                    setEmail(v.trim());
                    setTouched(true);
                }}
                editable={isEmailChecked}
                aria-disabled={!isEmailChecked}
                placeholder="Your email address"
                keyboardType="email-address"
                autoCapitalize="none"
                autoCorrect={false}
                errorMessages={emailErrors}
            />
            <HgTextField
                testID="emailConfirmationInput"
                accessibilityLabel="Confirm email address"
                autoComplete="email"
                value={emailConfirmation}
                onChangeText={(v) => {
                    setEmailConfirmation(v.trim());
                    setTouched(true);
                }}
                editable={isEmailChecked}
                aria-disabled={!isEmailChecked}
                placeholder="Confirm your email address"
                keyboardType="email-address"
                autoCapitalize="none"
                autoCorrect={false}
                errorMessages={emailConfirmationErrors}
            />

            <View className="h-[3px] bg-border-divider mt-6" />

            <HgCheckbox
                testID="sms-checkbox"
                label="Text Notifications"
                checked={isSmsChecked}
                onChange={(v) => {
                    setIsSmsChecked(v);
                    if (!v) setSmsMasked("");
                }}
            />
            <AppText variant="body-medium" className="italic mb-2">
                Enter your phone number to receive text updates when available.
            </AppText>
            <HgTextField
                testID="smsNumberInput"
                accessibilityLabel="Phone number"
                autoComplete="tel"
                value={smsMasked}
                onChangeText={(v) => {
                    setSmsMasked(maskPhone(v));
                    setTouched(true);
                }}
                editable={isSmsChecked}
                aria-disabled={!isSmsChecked}
                placeholder="Your phone number"
                keyboardType="phone-pad"
                maxLength={14}
                errorMessages={smsErrors}
            />

            {!isEmailChecked && !isSmsChecked ? (
                <View className="flex-row items-start mt-2">
                    <Icon
                        name="triangle-exclamation"
                        size={18}
                        color={colors.primary}
                        style={{ marginRight: 8, marginTop: 3 }}
                    />
                    <AppText className="flex-1 font-bold text-primary">
                        You won&apos;t receive notifications from the Health
                        Gateway. You can update this from your Profile Page
                        later.
                    </AppText>
                </View>
            ) : null}

            <AppText variant="h3" className="mt-10 mb-2 text-bc-black">
                Terms of Service
            </AppText>
            <TermsOfServiceBox html={termsOfService.content} height={330} />
            <HgCheckbox
                testID="acceptCheckbox"
                label="I agree to the terms of service above"
                checked={accepted}
                onChange={setAccepted}
                errorMessages={
                    touched && !accepted ? ["Value must be checked"] : []
                }
            />
            <View className="items-end mb-3 mt-2">
                <HgButton
                    testID="registerButton"
                    text="Register"
                    className="self-end px-12"
                    disabled={!accepted}
                    loading={submitting}
                    onPress={() => void onSubmit()}
                />
            </View>
        </View>
    );
}
