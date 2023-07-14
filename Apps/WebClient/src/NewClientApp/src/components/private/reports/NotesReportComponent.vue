<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTable from "@/components/common/HgDataTable.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import { ILogger, IReportService } from "@/services/interfaces";
import { useNoteStore } from "@/stores/note";

interface Props {
    hdid: string;
    filter: ReportFilter;
    isDependent: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
});

const emit = defineEmits<{
    (e: "on-is-loading-changed", newValue: boolean): void;
    (e: "on-is-empty-changed", newValue: boolean): void;
}>();

defineExpose({ generateReport });

interface UserNoteRow {
    date: string;
    title: string;
    note: string;
}

const fields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        tdAttr: { "data-testid": "user-note-date" },
    },
    {
        key: "title",
        title: "Title",
        tdAttr: { "data-testid": "user-note-title" },
    },
    {
        key: "note",
        title: "Note",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const noteStore = useNoteStore();

const notesAreLoading = computed<boolean>(() => noteStore.notesAreLoading);

const notes = computed<UserNote[]>(() => noteStore.notes);

const visibleRecords = computed<UserNote[]>(() => {
    const records = notes.value.filter((record) =>
        props.filter.allowsDate(record.journalDate)
    );
    records.sort((a, b) => {
        const firstDate = new DateWrapper(a.journalDate);
        const secondDate = new DateWrapper(b.journalDate);

        if (firstDate.isBefore(secondDate)) {
            return 1;
        }

        if (firstDate.isAfter(secondDate)) {
            return -1;
        }

        return 0;
    });
    return records;
});

const isEmpty = computed<boolean>(() => visibleRecords.value.length === 0);

const items = computed<UserNoteRow[]>(() => {
    return visibleRecords.value.map<UserNoteRow>((x) => ({
        date: DateWrapper.format(x.journalDate),
        title: x.title,
        note: x.text,
    }));
});

function generateReport(
    reportFormatType: ReportFormatType,
    headerData: ReportHeader
): Promise<RequestResult<Report>> {
    const reportService = container.get<IReportService>(
        SERVICE_IDENTIFIER.ReportService
    );

    return reportService.generateReport({
        data: {
            header: headerData,
            records: items.value,
        },
        template: TemplateType.Notes,
        type: reportFormatType,
    });
}
function onIsLoadingChanged(): void {
    emit("on-is-loading-changed", notesAreLoading.value);
}

function onIsEmptyChanged(): void {
    emit("on-is-empty-changed", isEmpty.value);
}

watch(notesAreLoading, () => {
    onIsLoadingChanged();
});

watch(isEmpty, () => {
    onIsEmptyChanged();
});

onMounted(() => {
    onIsEmptyChanged();
});

// Created hook
noteStore
    .retrieveNotes(props.hdid)
    .catch((err) => logger.error(`Error loading user note data: ${err}`));
</script>

<template>
    <section>
        <v-row v-if="isEmpty && !notesAreLoading">
            <v-col>No records found.</v-col>
        </v-row>

        <HgDataTable
            v-else-if="!isDependent"
            class="d-none d-md-block"
            :loading="notesAreLoading"
            :items="items"
            :fields="fields"
            height="600px"
            density="compact"
            data-testid="notes-report-table"
        />
    </section>
</template>
