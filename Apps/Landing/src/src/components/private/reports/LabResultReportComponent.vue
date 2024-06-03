<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTableComponent from "@/components/common/HgDataTableComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger, IReportService } from "@/services/interfaces";
import { useLabResultStore } from "@/stores/labResult";
import DateSortUtility from "@/utility/dateSortUtility";

interface Props {
    hdid: string;
    filter: ReportFilter;
    isDependent?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
});

const emit = defineEmits<{
    (e: "on-is-loading-changed", newValue: boolean): void;
    (e: "on-is-empty-changed", newValue: boolean): void;
}>();

defineExpose({ generateReport });

interface LabResultRow {
    date: string;
    test: string;
    result: string;
    status: string;
}

const fields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        tdAttr: { "data-testid": "labResultDateItem" },
    },
    {
        key: "test",
        title: "Test",
        tdAttr: { "data-testid": "labResultTestTypeItem" },
    },
    {
        key: "result",
        title: "Result",
        tdAttr: { "data-testid": "labResultItem" },
    },
    {
        key: "status",
        title: "Status",
        tdAttr: { "data-testid": "labStatusItem" },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const labResultStore = useLabResultStore();

const labResultsAreLoading = computed(() =>
    labResultStore.labResultsAreLoading(props.hdid)
);
const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    labResultStore
        .labResults(props.hdid)
        .filter((record) =>
            props.filter.allowsDate(
                DateWrapper.fromIso(record.timelineDateTime)
            )
        )
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIso(a.timelineDateTime),
                DateWrapper.fromIso(b.timelineDateTime)
            )
        )
);
const items = computed(() =>
    visibleRecords.value.flatMap<LabResultRow>((x) => {
        const timelineDateTime = DateWrapper.fromIso(
            x.timelineDateTime
        ).format();
        return x.laboratoryTests.map<LabResultRow>((y) => ({
            date: timelineDateTime,
            test: y.batteryType ?? "",
            result: y.result,
            status: y.testStatus,
        }));
    })
);

function generateReport(
    reportFormatType: ReportFormatType,
    headerData: ReportHeader
): Promise<RequestResult<Report>> {
    return reportService.generateReport({
        data: {
            header: headerData,
            records: items.value,
        },
        template: TemplateType.Laboratory,
        type: reportFormatType,
    });
}

watch(labResultsAreLoading, () => {
    emit("on-is-loading-changed", labResultsAreLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

labResultStore
    .retrieveLabResults(props.hdid)
    .catch((err) => logger.error(`Error loading Laboratory data: ${err}`));
</script>

<template>
    <p v-if="isEmpty && !labResultsAreLoading" class="px-4">
        <v-col>No records found.</v-col>
    </p>
    <HgDataTableComponent
        v-else-if="!isDependent"
        class="d-none d-md-block"
        :loading="labResultsAreLoading"
        :items="items"
        :fields="fields"
        density="compact"
        data-testid="laboratory-report-table"
    />
</template>
