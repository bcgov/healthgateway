import { voidMethod } from "@test/stubs/util";

import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import {
    TimelineActions,
    TimelineGetters,
    TimelineModule,
    TimelineMutations,
    TimelineState,
} from "@/store/modules/timeline/types";

const timelineState: TimelineState = {
    filter: TimelineFilterBuilder.buildEmpty(),
    keyword: "",
    isLinearView: true,
    linearDate: new DateWrapper(),
    calendarDate: new DateWrapper(),
    selectedDate: null,
};

const timelineGetters: TimelineGetters = {
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

const timelineActions: TimelineActions = {
    setFilter: voidMethod,
    setKeyword: voidMethod,
    setLinearView: voidMethod,
    clearFilter: voidMethod,
    setLinearDate: voidMethod,
    setCalendarDate: voidMethod,
    setSelectedDate: voidMethod,
};

const timelineMutations: TimelineMutations = {
    setFilter: voidMethod,
    setKeyword: voidMethod,
    setLinearView: voidMethod,
    clearFilter: voidMethod,
    setLinearDate: voidMethod,
    setCalendarDate: voidMethod,
    setSelectedDate: voidMethod,
};

const timelineStub: TimelineModule = {
    namespaced: true,
    state: timelineState,
    getters: timelineGetters,
    actions: timelineActions,
    mutations: timelineMutations,
};

export default timelineStub;
