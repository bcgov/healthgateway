import { useRouter } from "expo-router";
import { useState } from "react";
import { ActionSheetIOS, Platform, Pressable, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { AppText, Icon } from "@/components/common";
import { AppModal } from "@/components/common/AppModal";
import { Backdrop } from "@/components/common/Backdrop";
import { type EnvironmentId, environmentsOf } from "@/config/environments";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import {
    selectEnvironment,
    useEnvironmentStore,
} from "@/stores/environment.store";
import { switchEnvironment } from "@/stores/switchEnvironment";
import { runReset } from "@/utils/resetApp";

const TITLE = "Environment";

/**
 * The environment picker for the start-up error page, which can't reach the Security page's Environment section: iOS's
 * system action sheet, an Android bottom sheet. `open()` shows it; render `sheet` once near the caller. Native only.
 */
export function useEnvironmentSelector() {
    const router = useRouter();
    const colors = useThemeColors();
    const insets = useSafeAreaInsets();
    const current = useEnvironmentStore(selectEnvironment);
    const [visible, setVisible] = useState(false);
    // what this build offers (the error page shows the button only when that is more than one)
    const offered = useEnvironmentStore((s) => s.offered);
    const options = environmentsOf(offered);

    const choose = (id: EnvironmentId) => {
        runReset(() => switchEnvironment(id, () => router.replace(Path.Root)));
    };

    const open = () => {
        if (Platform.OS === "ios") {
            ActionSheetIOS.showActionSheetWithOptions(
                {
                    title: TITLE,
                    options: [
                        ...options.map((e) =>
                            e.id === current.id ? `${e.name} ✓` : e.name
                        ),
                        "Cancel",
                    ],
                    cancelButtonIndex: options.length,
                },
                (index) => {
                    const environment = options[index];
                    if (environment) choose(environment.id);
                }
            );
            return;
        }
        setVisible(true);
    };

    const sheet =
        Platform.OS === "ios" ? null : (
            <AppModal
                supportedOrientations={["portrait", "landscape"]}
                transparent
                visible={visible}
                animationType="slide"
                onRequestClose={() => setVisible(false)}
            >
                <Backdrop
                    className="flex-1 justify-end bg-black/40"
                    onPress={() => setVisible(false)}
                >
                    <View
                        testID="environment-sheet"
                        onStartShouldSetResponder={() => true}
                        className="bg-surface rounded-t-2xl pt-2"
                        style={{ paddingBottom: insets.bottom + 8 }}
                    >
                        <View className="self-center w-8 h-1 rounded-full bg-border-divider mb-2" />
                        <AppText variant="label" className="px-4 py-2">
                            {TITLE}
                        </AppText>
                        {options.map((e) => (
                            <Pressable
                                key={e.id}
                                testID={`environment-${e.id}`}
                                accessibilityRole="button"
                                aria-selected={e.id === current.id}
                                onPress={() => {
                                    setVisible(false);
                                    choose(e.id);
                                }}
                                className="flex-row items-center px-4 py-3 active:bg-surface-hover"
                            >
                                <AppText className="flex-1">{e.name}</AppText>
                                {e.id === current.id ? (
                                    <Icon
                                        name="check"
                                        size={16}
                                        color={colors.primary}
                                    />
                                ) : null}
                            </Pressable>
                        ))}
                    </View>
                </Backdrop>
            </AppModal>
        );

    return { open, sheet };
}
