import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import {
    TimelineActions,
    TimelineGetters,
    TimelineModule,
    TimelineMutations,
    TimelineState,
} from "@/store/modules/timeline/types";

import { stubbedVoid } from "../../utility/stubUtil";

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
    setFilter: stubbedVoid,
    setKeyword: stubbedVoid,
    setLinearView: stubbedVoid,
    clearFilter: stubbedVoid,
    setLinearDate: stubbedVoid,
    setCalendarDate: stubbedVoid,
    setSelectedDate: stubbedVoid,
};

const timelineMutations: TimelineMutations = {
    setFilter: stubbedVoid,
    setKeyword: stubbedVoid,
    setLinearView: stubbedVoid,
    clearFilter: stubbedVoid,
    setLinearDate: stubbedVoid,
    setCalendarDate: stubbedVoid,
    setSelectedDate: stubbedVoid,
};

const timelineStub: TimelineModule = {
    namespaced: true,
    state: timelineState,
    getters: timelineGetters,
    actions: timelineActions,
    mutations: timelineMutations,
};

export default timelineStub;
