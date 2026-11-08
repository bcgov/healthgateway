import type { TimelineEntry } from "@/models/timeline/timelineEntry";

/** Props every dataset's timeline entry component takes (the web client's entry component props). */
export interface TimelineEntryProps {
    entry: TimelineEntry;
    hdid: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
    testID?: string;
}
