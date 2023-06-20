<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import { Encounter } from "@/models/encounter";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

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

const headerClass = "encounter-report-table-header";
const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        tdAttr: { "data-testid": "mspVisitDateItem" },
    },
    {
        key: "specialty_description",
        thClass: headerClass,
    },
    {
        key: "practitioner",
        thClass: headerClass,
    },
    {
        key: "clinic_practitioner",
        label: "Clinic/Practitioner",
        thClass: headerClass,
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const store = useStore();

const healthVisitsAreLoading = computed<boolean>(() =>
    store.getters["encounter/healthVisitsAreLoading"](props.hdid)
);
const healthVisits = computed<Encounter[]>(() =>
    store.getters["encounter/healthVisits"](props.hdid)
);

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

function retrieveHealthVisits(hdid: string): Promise<void> {
    return store.dispatch("encounter/retrieveHealthVisits", { hdid });
}

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

retrieveHealthVisits(props.hdid).catch((err) =>
    logger.error(`Error loading encounter data: ${err}`)
);
</script>

<template>
    <div>
        <div>
            <section>
                <b-row v-if="isEmpty && !healthVisitsAreLoading">
                    <b-col>No records found.</b-col>
                </b-row>

                <b-table
                    v-else-if="!isDependent"
                    :striped="true"
                    :fixed="true"
                    :busy="healthVisitsAreLoading"
                    :items="items"
                    :fields="fields"
                    data-testid="msp-visits-report-table"
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

.encounter-report-table-header {
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
