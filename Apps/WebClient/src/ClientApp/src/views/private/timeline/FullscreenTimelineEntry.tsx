import { useIsFocused, usePathname, useRouter } from "expo-router";
import { useEffect, useRef } from "react";

import { useBreakpoint } from "@/hooks/useBreakpoint";
import { toHref } from "@/navigation/href";
import { useTimelineStore } from "@/stores/timeline.store";

/**
 * The entry page beside a timeline: `/timeline` → `/timeline/entry`, a dependent's `/dependents/<id>/timeline` →
 * `/dependents/<id>/entry`.
 */
export const entryPagePath = (timelinePath: string) =>
    timelinePath.replace(/\/timeline\/?$/i, "") +
    (/^\/timeline\/?$/i.test(timelinePath) ? "/timeline/entry" : "/entry");

/**
 * iOS and Android: on a narrow window an entry opens as a page of its own, pushed on the tab's stack, so it gets the
 * platform's navigation bar, back button and back swipe (`app/(tabs)/timeline/entry.tsx`, `TimelineEntryPage`). Only
 * the timeline on screen pushes it: the Health Records and Dependents tabs can both have one mounted.
 *
 * Native, stated deviation: when the window narrows (a Duo folding) the entry expanded last carries on as that page,
 * appearing without the push animation; widening takes it back to its card, expanded (`TimelineEntryPage`). Web:
 * `FullscreenTimelineEntry.web.tsx`, the web client's full-screen dialog.
 */
export function FullscreenTimelineEntry({
    hdid,
    commentsAreEnabled = false,
}: {
    hdid: string;
    commentsAreEnabled?: boolean;
}) {
    const router = useRouter();
    const pathname = usePathname();
    const focused = useIsFocused();
    const { inlineEntryDetails } = useBreakpoint();
    const entry = useTimelineStore((s) => s.fullscreenEntry);
    const open = useTimelineStore((s) => s.openFullscreenEntry);
    const newestInlineEntry = useTimelineStore((s) => s.inlineEntries.at(-1));
    // read before the cards remove themselves from the list, which happens as the window narrows
    const newestInlineEntryRef = useRef(newestInlineEntry);

    // narrowing carries the entry expanded last to its page, without the push animation
    useEffect(() => {
        if (!inlineEntryDetails && newestInlineEntryRef.current)
            open(newestInlineEntryRef.current, { instant: true });
        // eslint-disable-next-line react-hooks/exhaustive-deps -- only when the layout changes
    }, [inlineEntryDetails]);
    useEffect(() => {
        newestInlineEntryRef.current = newestInlineEntry;
    }, [newestInlineEntry]);

    // An entry opened while this timeline is on screen, on a narrow window: its page. Only when an entry opens: coming
    // back from the page (the entry is cleared once the page has gone) or to this tab must not push it again.
    useEffect(() => {
        if (!entry || !focused || inlineEntryDetails) return;
        useTimelineStore.setState({
            entryPageFor: { hdid, commentsAreEnabled },
        });
        router.push(toHref(entryPagePath(pathname)));
        // eslint-disable-next-line react-hooks/exhaustive-deps -- only when an entry opens
    }, [entry]);

    return null;
}
