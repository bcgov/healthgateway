<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTable from "@/components/common/HgDataTable.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { Encounter } from "@/models/encounter";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger, IReportService } from "@/services/interfaces";
import { useHealthVisitsStore } from "@/stores/healthVisits";

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
const healthVisitStore = useHealthVisitsStore();

const healthVisitsAreLoading = computed(() =>
    healthVisitStore.healthVisitsAreLoading(props.hdid)
);
const healthVisits = computed(() => healthVisitStore.healthVisits(props.hdid));

const isEmpty = computed<boolean>(() => visibleRecords.value.length === 0);
const visibleRecords = computed<Encounter[]>(() =>
    healthVisits.value
        .filter((record) => props.filter.allowsDate(record.encounterDate))
        .sort((a, b) => {
            const firstDate = new DateWrapper(a.encounterDate);
            const secondDate = new DateWrapper(b.encounterDate);

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

        <HgDataTable
            v-else-if="!isDependent"
            fixed-header
            :loading="healthVisitsAreLoading"
            :items="items"
            :fields="fields"
            density="compact"
            height="600px"
            data-testid="msp-visits-report-table"
        >
        </HgDataTable>
    </section>
</template>
