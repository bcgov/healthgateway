import { useEffect, useState } from "react";
import { Platform, View } from "react-native";

import { useAnnouncement } from "./announce";
import { AppText } from "./AppText";

/** How long the regions stay empty between two announcements, so a repeat is still a change. */
const CLEAR_MS = 50;

/**
 * Web: two visually hidden live regions, one polite and one assertive, that `announce()` writes into. Mount once, in
 * the root layout. Native: nothing; `announce()` speaks through the platform.
 */
export function Announcer() {
    const { text, politeness, seq } = useAnnouncement();
    // the regions are empty until the announcement is a little old, so a repeat of the same text is still a change
    const [shownSeq, setShownSeq] = useState(0);
    useEffect(() => {
        if (Platform.OS !== "web") return;
        const timer = setTimeout(() => setShownSeq(seq), CLEAR_MS);
        return () => clearTimeout(timer);
    }, [seq]);
    const shown = shownSeq === seq ? text : "";
    if (Platform.OS !== "web") return null;
    return (
        <>
            <View
                testID="announcer-polite"
                className="sr-only"
                {...({
                    role: "status",
                    "aria-live": "polite",
                    accessibilityAtomic: true,
                } as object)}
            >
                <AppText>{politeness === "polite" ? shown : ""}</AppText>
            </View>
            <View
                testID="announcer-assertive"
                className="sr-only"
                {...({
                    role: "alert",
                    "aria-live": "assertive",
                    accessibilityAtomic: true,
                } as object)}
            >
                <AppText>{politeness === "assertive" ? shown : ""}</AppText>
            </View>
        </>
    );
}
