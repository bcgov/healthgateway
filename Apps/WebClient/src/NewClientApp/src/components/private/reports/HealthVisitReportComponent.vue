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
import { useHealthVisitStore } from "@/stores/healthVisit";
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

interface EncounterRow {
    date: string;
    specialty_description: string;
    practitioner: string;
    clinic_practitioner: string;
}

const fields: ReportField[] = [
    {
        key: "date",
        title: "Visit Date",
        tdAttr: { "data-testid": "mspVisitDateItem" },
    },
    {
        key: "specialty_description",
        title: "Specialty",
    },
    {
        key: "practitioner",
        title: "Practitioner",
    },
    {
        key: "clinic_practitioner",
        title: "Clinic/Practitioner",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const healthVisitStore = useHealthVisitStore();

const healthVisitsAreLoading = computed(() =>
    healthVisitStore.healthVisitsAreLoading(props.hdid)
);
const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    healthVisitStore
        .healthVisits(props.hdid)
        .filter((record) => props.filter.allowsDate(record.encounterDate))
        .sort((a, b) =>
            DateSortUtility.descendingByString(a.encounterDate, b.encounterDate)
        )
);
const items = computed(() =>
    visibleRecords.value.map<EncounterRow>((x) => ({
        date: DateWrapper.format(x.encounterDate),
        specialty_description: x.specialtyDescription,
        practitioner: x.practitionerName,
        clinic_practitioner: x.clinic.name,
    }))
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
        template: TemplateType.Encounter,
        type: reportFormatType,
    });
}

watch(healthVisitsAreLoading, () => {
    emit("on-is-loading-changed", healthVisitsAreLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

healthVisitStore
    .retrieveHealthVisits(props.hdid)
    .catch((err) => logger.error(`Error loading encounter data: ${err}`));
</script>

<template>
    <section>
        <v-row v-if="isEmpty && !healthVisitsAreLoading">
            <v-col>No records found.</v-col>
        </v-row>

        <HgDataTableComponent
            v-else-if="!isDependent"
            class="d-none d-md-block"
            fixed-header
            :loading="healthVisitsAreLoading"
            :items="items"
            :fields="fields"
            density="compact"
            height="600px"
            data-testid="msp-visits-report-table"
        />
    </section>
</template>
