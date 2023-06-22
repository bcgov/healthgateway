import { DateWrapper } from "./../models/dateWrapper";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { StringISODate } from "@/models/dateWrapper";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { ILogger } from "@/services/interfaces";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import { EntryType } from "@/constants/entryType";

export const useNoteStore = defineStore("timeline", () => {
    const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

    // Refs
    const keyword = ref("");
    const timeLineFilter = ref(TimelineFilterBuilder.buildEmpty());
    const timeLineLinearDate = ref(new DateWrapper().toISO());
    const timeLineSelectedDate = ref<StringISODate>();

    // Computed
    const filter = computed(() => {
        if (timeLineFilter.value instanceof TimelineFilter) {
            return timeLineFilter.value;
        } else {
            return Object.assign(
                TimelineFilterBuilder.buildEmpty(),
                timeLineFilter.value
            );
        }
    });

    const hasActiveFilter = computed(() => {
        if (timeLineFilter.value instanceof TimelineFilter) {
            return (
                timeLineFilter.value.hasActiveFilter() || keyword.value !== ""
            );
        }

        const timelineFilter: TimelineFilter = Object.assign(
            TimelineFilterBuilder.buildEmpty(),
            timeLineFilter.value
        );

        return timelineFilter.hasActiveFilter() || keyword.value !== "";
    });

    const linearDate = computed(() => {
        return new DateWrapper(timeLineLinearDate.value);
    });

    const selectedDate = computed(() => {
        if (timeLineSelectedDate.value === undefined) {
            return undefined;
        }

        return new DateWrapper(timeLineLinearDate.value);
    });

    const selectedEntryTypes = computed(() => {
        if (
            timeLineFilter.value instanceof TimelineFilter &&
            timeLineFilter.value.entryTypes instanceof Set<EntryType>
        ) {
            return timeLineFilter.value.entryTypes;
        }

        return Object.assign(
            new Set<EntryType>(),
            timeLineFilter.value.entryTypes
        );
    });

    // Actions
    function setFilter(filterBuilder: TimelineFilterBuilder) {
        logger.verbose(`Setting filter to time line store.`);
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
        logger.verbose(`Setting selected date to time line store.`);
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
