import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

import { TimelineMutations, TimelineState } from "./types";

export const mutations: TimelineMutations = {
    setFilter(state: TimelineState, filter: TimelineFilter) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setFilter`);
        state.filter = filter;
    },
    setKeyword(state: TimelineState, keyword: string) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setKeyword`);
        state.keyword = keyword;
    },
    setLinearView(state: TimelineState, isLinearView: boolean) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setLinearView`);
        state.isLinearView = isLinearView;
    },
    clearFilter(state: TimelineState) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:clearFilter`);
        state.filter = TimelineFilterBuilder.buildEmpty();
    },
    setLinearDate(state: TimelineState, linearDate: DateWrapper) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setLinearDate`);
        state.linearDate = linearDate;
    },
    setCalendarDate(state: TimelineState, calendarDate: DateWrapper) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setCalendarDate`);
        state.calendarDate = calendarDate;
    },
    setSelectedDate(state: TimelineState, selectedDate: DateWrapper | null) {
        const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setSelectedDate`);
        state.selectedDate = selectedDate;
    },
};
