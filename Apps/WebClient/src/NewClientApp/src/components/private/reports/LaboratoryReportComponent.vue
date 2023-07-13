<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTable from "@/components/common/HgDataTable.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder } from "@/models/laboratory";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger, IReportService } from "@/services/interfaces";
import { useLabResultStore } from "@/stores/labResult";

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

interface LabTestRow {
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

const laboratoryOrders = computed<LaboratoryOrder[]>(() =>
    labResultStore.laboratoryOrders(props.hdid)
);
const laboratoryOrdersAreLoading = computed<boolean>(() =>
    labResultStore.laboratoryResultsAreLoading(props.hdid)
);

const isEmpty = computed(() => visibleRecords.value.length === 0);

const visibleRecords = computed(() =>
    laboratoryOrders.value
        .filter((record) => props.filter.allowsDate(record.timelineDateTime))
        .sort((a, b) => {
            const firstDate = new DateWrapper(a.timelineDateTime);
            const secondDate = new DateWrapper(b.timelineDateTime);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        })
);

const items = computed(() =>
    visibleRecords.value.flatMap<LabTestRow>((x) => {
        const timelineDateTime = DateWrapper.format(x.timelineDateTime);
        return x.laboratoryTests.map<LabTestRow>((y) => ({
            date: timelineDateTime,
            test: y.batteryType || "",
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

watch(laboratoryOrdersAreLoading, () => {
    emit("on-is-loading-changed", laboratoryOrdersAreLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

labResultStore
    .retrieveLaboratoryOrders(props.hdid)
    .catch((err) => logger.error(`Error loading Laboratory data: ${err}`));
</script>

<template>
    <p v-if="isEmpty && !laboratoryOrdersAreLoading" class="px-4">
        <v-col>No records found.</v-col>
    </p>
    <HgDataTable
        v-else-if="!isDependent"
        fixed-header
        :loading="laboratoryOrdersAreLoading"
        :items="items"
        :fields="fields"
        height="600px"
        density="compact"
        data-testid="laboratory-report-table"
    >
    </HgDataTable>
</template>
