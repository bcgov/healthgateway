import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import {
    TimelineActions,
    TimelineGetters,
    TimelineModule,
    TimelineMutations,
    TimelineState,
} from "@/store/modules/timeline/types";

var timelineState: TimelineState = {
    filter: TimelineFilterBuilder.buildEmpty(),
    keyword: "",
    isLinearView: true,
    linearDate: new DateWrapper(),
    calendarDate: new DateWrapper(),
    selectedDate: null,
};

var timelineGetters: TimelineGetters = {
    filter(): TimelineFilter {
        return TimelineFilterBuilder.buildEmpty();
    },
    hasActiveFilter(): boolean {
        return false;
    },
    isLinearView(): boolean {
        return true;
    },
    keyword(): string {
        return "";
    },
    linearDate(): DateWrapper {
        return new DateWrapper();
    },
    calendarDate(): DateWrapper {
        return new DateWrapper();
    },
    selectedDate(): DateWrapper | null {
        return null;
    },
};

var timelineActions: TimelineActions = {
    setFilter(): void {},
    setKeyword(): void {},
    setLinearView(): void {},
    clearFilter(): void {},
    setLinearDate(): void {},
    setCalendarDate(): void {},
    setSelectedDate(): void {},
};

var timelineMutations: TimelineMutations = {
    setFilter(): void {},
    setKeyword(): void {},
    setLinearView(): void {},
    clearFilter(): void {},
    setLinearDate(): void {},
    setCalendarDate(): void {},
    setSelectedDate(): void {},
};

var timelineStub: TimelineModule = {
    namespaced: true,
    state: timelineState,
    getters: timelineGetters,
    actions: timelineActions,
    mutations: timelineMutations,
};

export default timelineStub;
