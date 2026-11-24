import { useEffect } from "react";
import { Pressable, View } from "react-native";

import { Icon, ModalSafeArea } from "@/components/common";
import { AppModal } from "@/components/common/AppModal";
import { ErrorAlert } from "@/components/site/ErrorAlert";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useThemeColors } from "@/hooks/useTheme";
import { useTimelineStore } from "@/stores/timeline.store";

import { TimelineEntryView } from "./entries";
import { EntryDetailsLayout } from "./EntryDetailsLayout";

/**
 * Port of FullscreenTimelineEntryComponent.vue: on mobile an entry opens full screen with a back button. The dialog
 * pushes a `?details` history entry so the browser's back button closes it, as the web client does. iOS and Android:
 * `FullscreenTimelineEntry.tsx`, which opens the entry as a page of its own instead.
 */
export function FullscreenTimelineEntry({
    hdid,
    commentsAreEnabled = false,
}: {
    hdid: string;
    commentsAreEnabled?: boolean;
}) {
    const colors = useThemeColors();
    const { inlineEntryDetails } = useBreakpoint();
    const entry = useTimelineStore((s) => s.fullscreenEntry);
    const close = useTimelineStore((s) => s.closeFullscreenEntry);
    const visible = entry !== undefined;

    // simulate a history push so Back closes the dialog
    useEffect(() => {
        if (!visible) return;
        globalThis.history.pushState({}, "Entry Details", "?details");
        const onPop = () => close();
        globalThis.addEventListener("popstate", onPop);
        return () => globalThis.removeEventListener("popstate", onPop);
    }, [visible, close]);

    function closeDialog() {
        if (visible) globalThis.history.back();
    }

    // leaving the mobile layout closes it, as the web client does
    useEffect(() => {
        if (inlineEntryDetails && visible) closeDialog();
        // eslint-disable-next-line react-hooks/exhaustive-deps -- closeDialog only reads current values
    }, [inlineEntryDetails]);

    return (
        <AppModal
            supportedOrientations={["portrait", "landscape"]}
            visible={visible}
            animationType="slide"
            onRequestClose={closeDialog}
        >
            {/* the surface also fills the safe areas around the padded content */}
            <View style={{ flex: 1, backgroundColor: colors.surface }}>
                <ModalSafeArea>
                    <View
                        testID="entryDetailsModal"
                        nativeID="entry-details-modal"
                        className="flex-1 bg-surface"
                    >
                        {/* v-toolbar density="compact" with the back button */}
                        <View className="min-h-12 flex-row items-center px-1 border-b border-border-divider">
                            <Pressable
                                testID="backBtn"
                                accessibilityRole="button"
                                accessibilityLabel="Back"
                                onPress={closeDialog}
                                className="w-11 h-11 items-center justify-center"
                            >
                                <Icon
                                    name="arrow-left"
                                    size={20}
                                    color={colors["bc-black"]}
                                />
                            </Pressable>
                        </View>
                        {entry ? (
                            <EntryDetailsLayout>
                                <View className="mx-2">
                                    <ErrorAlert />
                                </View>
                                <TimelineEntryView
                                    testID="entryDetailsCard"
                                    entry={entry}
                                    hdid={hdid}
                                    isMobileDetails
                                    commentsAreEnabled={commentsAreEnabled}
                                />
                            </EntryDetailsLayout>
                        ) : null}
                    </View>
                </ModalSafeArea>
            </View>
        </AppModal>
    );
}
