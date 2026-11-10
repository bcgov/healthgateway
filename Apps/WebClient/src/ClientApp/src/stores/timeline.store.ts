import type { DateTime } from "luxon";
import { create } from "zustand";

import type { TimelineEntry } from "@/models/timeline/timelineEntry";
import {
    type TimelineFilter,
    TimelineFilterBuilder,
} from "@/models/timeline/timelineFilter";
import { logger } from "@/services/logger";
import { now } from "@/utils/dateUtil";

interface TimelineState {
    keyword: string;
    filter: TimelineFilter;
    linearDate: DateTime;
    selectedDate?: DateTime;
    /** the entry the full-screen details dialog shows on mobile (EventName.OpenFullscreenTimelineEntry) */
    fullscreenEntry?: TimelineEntry;
    /** native: the entries expanded in place on the focused screen, oldest first; folding moves the newest to full screen */
    inlineEntries: TimelineEntry[];
    setFilter: (filterBuilder: TimelineFilterBuilder) => void;
    clearFilter: () => void;
    setLinearDate: (linearDate: DateTime) => void;
    setSelectedDate: (selectedDate?: DateTime) => void;
    openFullscreenEntry: (entry: TimelineEntry) => void;
    closeFullscreenEntry: () => void;
    addInlineEntry: (entry: TimelineEntry) => void;
    removeInlineEntry: (entry: TimelineEntry) => void;
}

/** Port of stores/timeline.ts, plus the full-screen entry the web client passes through its event bus. */
export const useTimelineStore = create<TimelineState>((set) => ({
    keyword: "",
    filter: TimelineFilterBuilder.buildEmpty(),
    linearDate: now(),
    selectedDate: undefined,
    fullscreenEntry: undefined,
    inlineEntries: [],
    setFilter: (filterBuilder) => {
        logger.verbose("Setting filter in timeline store.");
        set({ filter: filterBuilder.build() });
    },
    clearFilter: () => {
        logger.verbose("Clearing filter in timeline store.");
        set({ filter: TimelineFilterBuilder.buildEmpty() });
    },
    setLinearDate: (linearDate) => {
        logger.verbose("Setting linear date in timeline store.");
        set({ linearDate });
    },
    setSelectedDate: (selectedDate) => {
        logger.verbose("Setting selected date in timeline store.");
        set({ selectedDate });
    },
    openFullscreenEntry: (entry) => set({ fullscreenEntry: entry }),
    closeFullscreenEntry: () => set({ fullscreenEntry: undefined }),
    addInlineEntry: (entry) =>
        set((s) => ({ inlineEntries: [...s.inlineEntries, entry] })),
    removeInlineEntry: (entry) =>
        set((s) => ({
            inlineEntries: s.inlineEntries.filter((e) => e !== entry),
        })),
}));

export const selectHasActiveFilter = (s: TimelineState) =>
    s.filter.hasActiveFilter() || s.keyword !== "";
export const selectSelectedEntryTypes = (s: TimelineState) =>
    s.filter.entryTypes;
