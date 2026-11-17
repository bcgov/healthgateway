import { useRouter } from "expo-router";
import { useNavigation } from "expo-router/react-navigation";
import { useEffect, useLayoutEffect } from "react";
import { ScrollView, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { ErrorAlert } from "@/components/site/ErrorAlert";
import { getEntryTypeDetails } from "@/constants/entryType";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { toHref } from "@/navigation/href";
import { useTimelineStore } from "@/stores/timeline.store";

import { TimelineEntryView } from "./entries";

/** The page's back button, to the timeline beneath (the platform's own, named for it). */
const BACK_TITLE = "Health Records";

/**
 * iOS and Android: a timeline entry on a page of its own, pushed on the tab's stack over the timeline that opened it
 * (`FullscreenTimelineEntry`), titled with the record type. Leaving it (Back, the back swipe, Android's back) closes the
 * entry. Widening the window (a Duo unfolding) goes back to the timeline at once, where the entry's card carries on
 * expanded. With no entry to show (a link opened straight to it, or the app restarted there) it goes to `timelinePath`.
 */
export function TimelineEntryPage({ timelinePath }: { timelinePath: string }) {
    const navigation = useNavigation();
    const router = useRouter();
    const { inlineEntryDetails } = useBreakpoint();
    const entry = useTimelineStore((s) => s.fullscreenEntry);
    const owner = useTimelineStore((s) => s.entryPageFor);
    const instant = useTimelineStore((s) => s.entryPageInstant);
    const title = getEntryTypeDetails(entry?.type)?.recordName ?? "";

    useLayoutEffect(() => {
        navigation.setOptions({ title, headerBackTitle: BACK_TITLE });
    }, [navigation, title]);

    // leaving the page, however it is left, closes the entry
    useEffect(
        () => () => useTimelineStore.getState().closeFullscreenEntry(),
        []
    );

    // Unfolding: back to the timeline, without the animation (the stack reads `entryPageInstant` for this page). The
    // entry stays open until the page has gone, so its card sees it and opens expanded.
    useEffect(() => {
        if (inlineEntryDetails)
            useTimelineStore.setState({ entryPageInstant: true });
    }, [inlineEntryDetails]);
    useEffect(() => {
        if (inlineEntryDetails && instant && navigation.canGoBack())
            navigation.goBack();
    }, [inlineEntryDetails, instant, navigation]);

    // nothing to show: to the timeline
    useEffect(() => {
        if (entry && owner) return;
        if (navigation.canGoBack()) navigation.goBack();
        else router.replace(toHref(timelinePath));
        // eslint-disable-next-line react-hooks/exhaustive-deps -- only on arrival
    }, []);

    if (!entry || !owner) return null;
    return (
        <ScrollView
            testID="entryDetailsPage"
            className="flex-1 bg-surface"
            contentContainerClassName="flex-grow"
            contentInsetAdjustmentBehavior="automatic"
            keyboardShouldPersistTaps="handled"
        >
            {/* landscape: the content keeps clear of the notch and the home indicator's sides */}
            <SafeAreaView edges={["left", "right"]} style={{ flexGrow: 1 }}>
                <View className="mx-2">
                    <ErrorAlert />
                </View>
                <TimelineEntryView
                    testID="entryDetailsCard"
                    entry={entry}
                    hdid={owner.hdid}
                    isMobileDetails
                    commentsAreEnabled={owner.commentsAreEnabled}
                />
            </SafeAreaView>
        </ScrollView>
    );
}
