import { type ReactNode, useState } from "react";
import {
    Alert,
    Image,
    type ImageSourcePropType,
    Linking,
    Platform,
    Pressable,
    ScrollView,
    View,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { AppText, HgButton, Icon, LogoPlate } from "@/components/ui";
import { useLocalAuthMethod } from "@/hooks/useAppLock";
import { useThemeColors } from "@/hooks/useTheme";
import { describeError, logger } from "@/services/logger";
import type { LocalAuthMethod } from "@/utils/localAuth";

export const methodLabel: Record<LocalAuthMethod, string> = {
    faceId: "Use Face ID",
    touchId: "Use Touch ID",
    biometrics: "Use biometrics",
    passcode: "Use passcode",
    none: "Set up authentication",
};

const securityTips = [
    {
        image: require("@/assets/images/onboarding/data-tip-touch.png") as ImageSourcePropType,
        text: "Do not turn off your fingerprint ID or face ID",
    },
    {
        image: require("@/assets/images/onboarding/data-tip-passcode.png") as ImageSourcePropType,
        text: "Do not share or turn off your passcode",
    },
    {
        image: require("@/assets/images/onboarding/data-tip-unlock.png") as ImageSourcePropType,
        text: "Do not leave your device unlocked",
    },
];

/** Old apps' messages for a device with no passcode or biometrics (AllowSecurityAccess / error_biometric_enrollment). */
function promptDeviceSetup() {
    const [title, message] =
        Platform.OS === "ios"
            ? [
                  "Allow Security Access",
                  "Health Gateway needs permission to use Passcode for the device.\n\nGo to Settings > Passcode and change the settings to on.",
              ]
            : [
                  "Enable Biometric",
                  "Health Gateway requires to authenticate using Biometric or passcode. Go to Settings > To enable Biometric or Passcode.",
              ];
    Alert.alert(title, message, [
        { text: "Not now", style: "cancel" },
        {
            text: "Settings",
            onPress: () => {
                Linking.openSettings().catch((error: unknown) =>
                    logger.warn(
                        `Unable to open Settings: ${describeError(error)}`
                    )
                );
            },
        },
    ]);
}

/** Port of LocalAuthPrivacyView.swift + DataSecurityTipsTableViewCell.swift */
function SecurityTips({ onBack }: { onBack: () => void }) {
    const colors = useThemeColors();
    return (
        <ScrollView contentContainerClassName="px-6 pb-6">
            <View className="flex-row items-center min-h-[48px] mb-2">
                <Pressable
                    testID="securityTipsBackBtn"
                    accessibilityRole="button"
                    accessibilityLabel="Back"
                    onPress={onBack}
                    className="w-11 h-11 items-center justify-center -ml-3"
                >
                    <Icon
                        name="chevron-left"
                        size={18}
                        color={colors.primary}
                    />
                </Pressable>
                <AppText variant="label" accessibilityRole="header">
                    Keeping your data secure
                </AppText>
            </View>
            <AppText className="mb-6">
                Health information for you and your family can be stored on this
                device. To keep this data private, you must have your device
                security features turned on.
            </AppText>
            {securityTips.map((tip) => (
                <View key={tip.text} className="flex-row items-center mb-6">
                    <Image
                        source={tip.image}
                        className="w-12 h-12 mr-4"
                        resizeMode="contain"
                    />
                    <AppText className="flex-1">{tip.text}</AppText>
                </View>
            ))}
        </ScrollView>
    );
}

interface Props {
    /** Turns the lock on through the system prompt. Not called while no method is set up. */
    onAuthenticate: () => void;
    busy?: boolean;
    /** "Not now" */
    secondary?: ReactNode;
}

/** Port of LocalAuthView.swift, the full-screen "Protect your personal information" page: the first-launch offer. */
export function LocalAuthPanel({ onAuthenticate, busy, secondary }: Props) {
    const insets = useSafeAreaInsets();
    const method = useLocalAuthMethod();
    const [showTips, setShowTips] = useState(false);

    return (
        <View
            className="flex-1 bg-background"
            // the top inset is OnboardingView's, above the environment banner
            style={{ paddingBottom: insets.bottom }}
        >
            {showTips ? (
                <SecurityTips onBack={() => setShowTips(false)} />
            ) : (
                <ScrollView contentContainerClassName="grow justify-center px-6 py-6">
                    <AppText variant="h3" accessibilityRole="header">
                        Protect your personal information
                    </AppText>
                    <AppText className="mt-4">
                        Device security must be enabled to support a quicker
                        sign-on and to save a copy of your data on this device
                        for when you are disconnected.
                    </AppText>
                    {/* opaque white artwork: on a plate in dark mode */}
                    <View className="self-center my-10">
                        <LogoPlate>
                            <Image
                                source={
                                    require("@/assets/images/onboarding/local-auth-icon.png") as ImageSourcePropType
                                }
                                className="w-[200px] h-[180px]"
                                resizeMode="contain"
                                accessibilityIgnoresInvertColors
                            />
                        </LogoPlate>
                    </View>
                    {method ? (
                        <HgButton
                            testID="localAuthBtn"
                            text={methodLabel[method]}
                            uppercase={false}
                            block
                            loading={busy}
                            onPress={
                                method === "none"
                                    ? promptDeviceSetup
                                    : onAuthenticate
                            }
                        />
                    ) : null}
                    {secondary}
                    <HgButton
                        testID="localAuthLearnMoreBtn"
                        text="Learn more about keeping your data secure"
                        variant="link"
                        uppercase={false}
                        className="self-center mt-4"
                        onPress={() => setShowTips(true)}
                    />
                </ScrollView>
            )}
        </View>
    );
}
