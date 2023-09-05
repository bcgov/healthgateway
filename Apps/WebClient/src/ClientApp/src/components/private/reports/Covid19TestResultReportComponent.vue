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
import { useCovid19TestResultStore } from "@/stores/covid19TestResult";
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

interface Covid19TestResultRow {
    date: string;
    test_type: string;
    test_location: string;
    result: string;
}

const fields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        tdAttr: { "data-testid": "covid19DateItem" },
    },
    {
        key: "test_type",
        title: "Type",
        tdAttr: { "data-testid": "covid19TestTypeItem" },
    },
    {
        key: "test_location",
        title: "Location",
        tdAttr: { "data-testid": "covid19LocationItem" },
    },
    {
        key: "result",
        title: "Result",
        tdAttr: { "data-testid": "covid19ResultItem" },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const covid19TestResultStore = useCovid19TestResultStore();

const covid19TestResultsAreLoading = computed(() =>
    covid19TestResultStore.covid19TestResultsAreLoading(props.hdid)
);
const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    covid19TestResultStore
        .covid19TestResults(props.hdid)
        .filter((r) =>
            props.filter.allowsDate(r.labResults[0].collectedDateTime)
        )
        .sort((a, b) =>
            DateSortUtility.descendingByString(
                a.labResults[0].collectedDateTime,
                b.labResults[0].collectedDateTime
            )
        )
);
const items = computed(() =>
    visibleRecords.value.map<Covid19TestResultRow>((x) => {
        const labResult = x.labResults[0];
        return {
            date: DateWrapper.format(labResult.collectedDateTime),
            test_type: labResult.testType,
            test_location: x.location ?? "",
            result: labResult.filteredLabResultOutcome,
        };
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
        template: TemplateType.COVID,
        type: reportFormatType,
    });
}

watch(covid19TestResultsAreLoading, () => {
    emit("on-is-loading-changed", covid19TestResultsAreLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

covid19TestResultStore
    .retrieveCovid19TestResults(props.hdid)
    .catch((err) => logger.error(`Error loading Covid19 data: ${err}`));
</script>

<template>
    <div>
        <section>
            <v-row v-if="isEmpty && !covid19TestResultsAreLoading">
                <v-col>No records found.</v-col>
            </v-row>
            <HgDataTableComponent
                v-else-if="!isDependent"
                class="d-none d-md-block"
                :loading="covid19TestResultsAreLoading"
                :items="items"
                :fields="fields"
                density="compact"
                data-testid="covid19-report-table"
            />
        </section>
    </div>
</template>
