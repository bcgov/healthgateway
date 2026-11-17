import { useRouter } from "expo-router";
import { useNavigation } from "expo-router/react-navigation";
import { useEffect, useLayoutEffect, useRef, useState } from "react";
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
 *
 * The Health Records and Dependents tabs can each have one open at once, so each page keeps the entry it opened with
 * rather than following the shared selection, which the other tab's page may have taken over.
 */
export function TimelineEntryPage({ timelinePath }: { timelinePath: string }) {
    const navigation = useNavigation();
    const router = useRouter();
    const { inlineEntryDetails } = useBreakpoint();
    // this page's own entry, as the timeline left it when it pushed the page
    const [opened] = useState(() => {
        const { fullscreenEntry, entryPageFor } = useTimelineStore.getState();
        return fullscreenEntry && entryPageFor
            ? { entry: fullscreenEntry, owner: entryPageFor }
            : undefined;
    });
    const instant = useTimelineStore((s) => s.entryPageInstant);
    const updated = useTimelineStore((s) => s.updatedEntry);
    const updatedOnArrival = useRef(updated);
    const title = getEntryTypeDetails(opened?.entry.type)?.recordName ?? "";

    const leave = () => {
        if (navigation.canGoBack()) navigation.goBack();
        else router.replace(toHref(timelinePath));
    };

    useLayoutEffect(() => {
        navigation.setOptions({ title, headerBackTitle: BACK_TITLE });
    }, [navigation, title]);

    // leaving the page, however it is left, closes its entry: only its own, not one the other tab has open since
    useEffect(
        () => () => {
            const timeline = useTimelineStore.getState();
            if (opened && timeline.fullscreenEntry === opened.entry)
                timeline.closeFullscreenEntry();
        },
        [opened]
    );

    // its note edited or deleted: back to the timeline, as web's dialog closes
    useEffect(() => {
        if (
            updated !== updatedOnArrival.current &&
            updated?.id === opened?.entry.id
        )
            leave();
        // eslint-disable-next-line react-hooks/exhaustive-deps -- only when an entry changes
    }, [updated]);

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
        if (!opened) leave();
        // eslint-disable-next-line react-hooks/exhaustive-deps -- only on arrival
    }, []);

    if (!opened) return null;
    const { entry, owner } = opened;
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
