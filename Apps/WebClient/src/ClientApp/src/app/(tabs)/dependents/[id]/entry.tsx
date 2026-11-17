import { Redirect, useLocalSearchParams } from "expo-router";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { toHref } from "@/navigation/href";
import { TimelineEntryPage } from "@/views/private/timeline/TimelineEntryPage";

/** iOS and Android: an entry from a dependent's Health Records on a page of its own. Web opens it in a dialog instead. */
export default function DependentTimelineEntry() {
    const { id } = useLocalSearchParams<{ id: string }>();
    const timelinePath = `${Path.Dependents}/${id}/timeline`;
    if (Platform.OS === "web") return <Redirect href={toHref(timelinePath)} />;
    return <TimelineEntryPage timelinePath={timelinePath} />;
}
