import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import { StringISODate } from "@/models/dateWrapper";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { ILogger } from "@/services/interfaces";

export const useTimelineStore = defineStore("timeline", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    // Refs
    const keyword = ref("");
    const timeLineFilter = ref(TimelineFilterBuilder.buildEmpty());
    const timeLineLinearDate = ref(new DateWrapper().toISO());
    const timeLineSelectedDate = ref<StringISODate>();

    // Computed
    const filter = computed(() => timeLineFilter.value);

    const hasActiveFilter = computed(
        () => timeLineFilter.value.hasActiveFilter() || keyword.value !== ""
    );

    const linearDate = computed<IDateWrapper>(() => {
        return new DateWrapper(timeLineLinearDate.value);
    });

    const selectedDate = computed<IDateWrapper | undefined>(() => {
        if (timeLineSelectedDate.value === undefined) {
            return undefined;
        }

        return new DateWrapper(timeLineLinearDate.value);
    });

    const selectedEntryTypes = computed(() => timeLineFilter.value.entryTypes);

    // Actions
    function setFilter(filterBuilder: TimelineFilterBuilder) {
        logger.verbose(`Setting filter in timeline store.`);
        timeLineFilter.value = filterBuilder.build();
    }

    function clearFilter() {
        logger.verbose(`Clearing filter in timeline store.`);
        timeLineFilter.value = TimelineFilterBuilder.buildEmpty();
    }

    function setLinearDate(linearDate: DateWrapper) {
        logger.verbose(`Setting linear date in timeline store.`);
        timeLineLinearDate.value = linearDate.toISO();
    }

    function setSelectedDate(selectedDate?: DateWrapper) {
        logger.verbose(`Setting selected date in timeline store.`);
        timeLineSelectedDate.value = selectedDate?.toISO();
    }

    return {
        filter,
        hasActiveFilter,
        keyword,
        linearDate,
        selectedDate,
        selectedEntryTypes,
        setFilter,
        clearFilter,
        setLinearDate,
        setSelectedDate,
    };
});
