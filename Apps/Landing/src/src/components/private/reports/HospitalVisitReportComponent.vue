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
import { useHospitalVisitStore } from "@/stores/hospitalVisit";
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

interface HospitalVisitRow {
    date: string;
    health_service: string;
    visit_type: string;
    location: string;
    provider: string;
}

const fields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        tdAttr: { "data-testid": "hospital-visit-date" },
    },
    {
        key: "health_service",
        title: "Health Service",
    },
    {
        key: "visit_type",
        title: "Visit Type",
    },
    {
        key: "location",
        title: "Location",
    },
    {
        key: "provider",
        title: "Provider",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const hospitalVisitStore = useHospitalVisitStore();

const hospitalVisitsAreLoading = computed(() =>
    hospitalVisitStore.hospitalVisitsAreLoading(props.hdid)
);
const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    hospitalVisitStore
        .hospitalVisits(props.hdid)
        .filter((record) =>
            props.filter.allowsDate(DateWrapper.fromIso(record.admitDateTime))
        )
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIso(a.admitDateTime),
                DateWrapper.fromIso(b.admitDateTime)
            )
        )
);
const items = computed(() =>
    visibleRecords.value.map<HospitalVisitRow>((x) => {
        return {
            date: DateWrapper.fromIso(x.admitDateTime).format(),
            health_service: x.healthService,
            visit_type: x.visitType,
            location: x.facility,
            provider: x.provider,
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
        template: TemplateType.HospitalVisit,
        type: reportFormatType,
    });
}

watch(hospitalVisitsAreLoading, () => {
    emit("on-is-loading-changed", hospitalVisitsAreLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

hospitalVisitStore
    .retrieveHospitalVisits(props.hdid)
    .catch((err) => logger.error(`Error loading hospital visit data: ${err}`));
</script>

<template>
    <div>
        <div>
            <section>
                <v-row v-if="isEmpty && !hospitalVisitsAreLoading">
                    <v-col>No records found.</v-col>
                </v-row>

                <HgDataTableComponent
                    v-else-if="!isDependent"
                    class="d-none d-md-block"
                    :loading="hospitalVisitsAreLoading"
                    :items="items"
                    :fields="fields"
                    density="compact"
                    data-testid="hospital-visit-report-table"
                />
            </section>
        </div>
    </div>
</template>
