import { MutationTree } from "vuex";

import { DateWrapper } from "@/models/dateWrapper";
import { TimelineState } from "@/models/storeState";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

export const mutations: MutationTree<TimelineState> = {
    setFilter(state: TimelineState, filter: TimelineFilter) {
        logger.verbose(`TimelineState:setFilter`);
        state.filter = filter;
    },
    setKeyword(state: TimelineState, keyword: string) {
        logger.verbose(`TimelineState:setKeyword`);
        state.keyword = keyword;
    },
    setLinearView(state: TimelineState, isLinearView: boolean) {
        logger.verbose(`TimelineState:setLinearView`);
        state.isLinearView = isLinearView;
    },
    clearFilter(state: TimelineState) {
        logger.verbose(`TimelineState:clearFilter`);
        state.filter = TimelineFilterBuilder.buildEmpty();
    },

    setLinearDate(state: TimelineState, linearDate: DateWrapper) {
        logger.verbose(`TimelineState:setLinearDate`);
        state.linearDate = linearDate;
    },

    setCalendarDate(state: TimelineState, calendarDate: DateWrapper) {
        logger.verbose(`TimelineState:setCalendarDate`);
        state.calendarDate = calendarDate;
    },

    setSelectedDate(state: TimelineState, selectedDate: DateWrapper | null) {
        logger.verbose(`TimelineState:setSelectedDate`);
        state.selectedDate = selectedDate;
    },
};
