import { Redirect } from "expo-router";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { TimelineEntryPage } from "@/views/private/timeline/TimelineEntryPage";

/** iOS and Android: a Health Records entry on a page of its own. Web opens entries in a dialog, so it never comes here. */
export default function TimelineEntry() {
    if (Platform.OS === "web") return <Redirect href={Path.Timeline} />;
    return <TimelineEntryPage timelinePath={Path.Timeline} />;
}
