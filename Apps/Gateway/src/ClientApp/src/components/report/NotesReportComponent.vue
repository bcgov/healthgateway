<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import UserNote from "@/models/userNote";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

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

const headerClass = "note-report-table-header";
const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        tdAttr: { "data-testid": "user-note-date" },
        thStyle: { width: "10%" },
    },
    {
        key: "title",
        thClass: headerClass,
        tdAttr: { "data-testid": "user-note-title" },
        thStyle: { width: "30%" },
    },
    {
        key: "note",
        thClass: headerClass,
        thStyle: { width: "60%" },
        tdClass: "text-left",
    },
];

const store = useStore();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const notesAreLoading = computed<boolean>(
    () => store.getters["note/notesAreLoading"]
);

const notes = computed<UserNote[]>(() => store.getters["note/notes"]);

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

function retrieveNotes(hdid: string): Promise<void> {
    return store.dispatch("note/retrieveNotes", { hdid });
}

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
retrieveNotes(props.hdid).catch((err) =>
    logger.error(`Error loading user note data: ${err}`)
);
</script>

<template>
    <div>
        <div>
            <section>
                <b-row v-if="isEmpty && !notesAreLoading">
                    <b-col>No records found.</b-col>
                </b-row>

                <b-table
                    v-else-if="!isDependent"
                    :striped="true"
                    :busy="notesAreLoading"
                    :items="items"
                    :fields="fields"
                    data-testid="notes-report-table"
                    class="table-style d-none d-md-table"
                >
                    <template #table-busy>
                        <content-placeholders>
                            <content-placeholders-text :lines="7" />
                        </content-placeholders>
                    </template>
                </b-table>
            </section>
        </div>
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.note-report-table-header {
    color: $heading_color;
    font-size: 0.8rem;
}
</style>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.table-style {
    font-size: 0.6rem;
    text-align: center;
}
</style>
