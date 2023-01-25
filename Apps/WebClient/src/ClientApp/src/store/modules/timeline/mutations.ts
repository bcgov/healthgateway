import { DateWrapper } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

import { TimelineMutations, TimelineState } from "./types";

export const mutations: TimelineMutations = {
    setFilter(state: TimelineState, filter: TimelineFilter) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setFilter`);
        state.filter = filter;
    },
    clearFilter(state: TimelineState) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:clearFilter`);
        state.filter = TimelineFilterBuilder.buildEmpty();
    },
    setLinearDate(state: TimelineState, linearDate: DateWrapper) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(
            `TimelineState:setLinearDate: ${JSON.stringify(linearDate)}`
        );
        state.linearDate = linearDate.toISO();
    },
    setSelectedDate(state: TimelineState, selectedDate: DateWrapper | null) {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.verbose(`TimelineState:setSelectedDate: ${selectedDate}`);
        if (selectedDate === null) {
            state.selectedDate = null;
        } else {
            state.selectedDate = selectedDate.toISO();
        }
    },
};
