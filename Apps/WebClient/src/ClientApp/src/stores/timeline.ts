import { defineStore } from "pinia";
import { computed, ref } from "vue";

import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper, IDateWrapper, StringISODate } from "@/models/dateWrapper";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { ILogger } from "@/services/interfaces";

export const useTimelineStore = defineStore("timeline", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    const keyword = ref("");
    const timeLineFilter = ref(TimelineFilterBuilder.buildEmpty());
    const timeLineLinearDate = ref(DateWrapper.now().toISO());
    const timeLineSelectedDate = ref<StringISODate>();

    const filter = computed(() => timeLineFilter.value);
    const hasActiveFilter = computed(
        () => timeLineFilter.value.hasActiveFilter() || keyword.value !== ""
    );
    const linearDate = computed<IDateWrapper>(() => {
        return DateWrapper.fromIso(timeLineLinearDate.value);
    });
    const selectedDate = computed<IDateWrapper | undefined>(() => {
        if (timeLineSelectedDate.value === undefined) {
            return undefined;
        }

        return DateWrapper.fromIso(timeLineLinearDate.value);
    });
    const selectedEntryTypes = computed(() => timeLineFilter.value.entryTypes);
    const columnCount = computed(() => 12);

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
        columnCount,
        setFilter,
        clearFilter,
        setLinearDate,
        setSelectedDate,
    };
});
