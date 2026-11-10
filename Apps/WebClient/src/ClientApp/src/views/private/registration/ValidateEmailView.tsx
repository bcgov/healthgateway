import { useLocalSearchParams, useRouter } from "expo-router";
import { useEffect, useState } from "react";
import { View } from "react-native";

import { AppText, HgButton, Icon, InternalLink } from "@/components/common";
import { Path } from "@/constants/path";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, ResultError } from "@/models/errors";
import { logger } from "@/services/logger";
import { useUserStore } from "@/stores/user.store";

/** Port of ValidateEmailView.vue — landing for the verification link (/validateEmail/:inviteKey). */
export function ValidateEmailView() {
    usePageTitle("Email Verification");
    const colors = useThemeColors();
    const router = useRouter();
    const { inviteKey } = useLocalSearchParams<{ inviteKey: string }>();
    const validateEmail = useUserStore((s) => s.validateEmail);
    const [isLoading, setIsLoading] = useState(true);
    const [isVerified, setIsVerified] = useState<boolean>();
    const [isAlreadyVerified, setIsAlreadyVerified] = useState<boolean>();

    useEffect(() => {
        validateEmail(String(inviteKey ?? ""))
            .then(setIsVerified)
            .catch((error) => {
                if (isSessionEnded(error)) return;
                if (error instanceof ResultError && error.statusCode === 409)
                    setIsAlreadyVerified(true);
                else logger.error("Error while validating email.");
            })
            .finally(() => setIsLoading(false));
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    const isSuccess = Boolean(isAlreadyVerified || isVerified);
    const validationFailed = isVerified === false;

    return (
        <View className="items-center p-4">
            {isLoading ? (
                <AppText
                    variant="h3"
                    testID="verifyingInvite"
                    className="text-bc-black"
                >
                    We are verifying your email...
                </AppText>
            ) : (
                <>
                    <Icon
                        name={isSuccess ? "circle-check" : "circle-xmark"}
                        size={48}
                        color={isSuccess ? colors.success : colors.error}
                    />
                    <View className="my-4 p-4 items-center">
                        {isVerified ? (
                            <AppText
                                variant="title-large"
                                testID="verifiedInvite"
                                className="text-center"
                            >
                                Your email address has been verified.{"\n"}
                                Health Gateway may now send you notifications.
                                {"\n"}You can change your preferences on your{" "}
                                <InternalLink
                                    href={Path.Profile}
                                    testID="verified-profile-page-link"
                                    className="font-sans text-[22px] leading-7 no-underline"
                                >
                                    Profile Page
                                </InternalLink>{" "}
                                at any time.
                            </AppText>
                        ) : null}
                        {isAlreadyVerified ? (
                            <AppText
                                variant="title-large"
                                testID="alreadyVerifiedInvite"
                                className="text-center"
                            >
                                Your email address is already verified
                            </AppText>
                        ) : null}
                        {validationFailed ? (
                            <AppText
                                variant="title-large"
                                testID="expiredInvite"
                                className="text-center"
                            >
                                Your link is expired or incorrect. Please resend
                                verification email from your profile page
                            </AppText>
                        ) : null}
                    </View>
                    <HgButton
                        testID="continueButton"
                        text="Continue"
                        onPress={() =>
                            router.replace(isSuccess ? Path.Home : Path.Profile)
                        }
                    />
                </>
            )}
        </View>
    );
}
