<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import ProtectiveWordComponent from "@/components/modal/ProtectiveWordComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
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

interface MedicationRow {
    date: string;
    din_pin: string;
    brand: string;
    generic: string;
    practitioner: string;
    quantity: string;
    strength: string;
    form: string;
    manufacturer: string;
}

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const store = useStore();

const notFoundText = "Not Found";
const headerClass = "medication-report-table-header";
const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        tdAttr: { "data-testid": "medicationDateItem" },
    },
    {
        key: "din_pin",
        label: "DIN/PIN",
        thClass: headerClass,
    },
    {
        key: "brand",
        thClass: headerClass,
        tdAttr: { "data-testid": "medicationReportBrandNameItem" },
    },
    {
        key: "generic",
        thClass: headerClass,
    },
    {
        key: "practitioner",
        thClass: headerClass,
    },
    {
        key: "quantity",
        thClass: headerClass,
    },
    {
        key: "strength",
        thClass: headerClass,
    },
    {
        key: "form",
        thClass: headerClass,
    },
    {
        key: "manufacturer",
        thClass: headerClass,
    },
];

const medicationsAreLoading = computed<boolean>(() =>
    store.getters["medication/medicationsAreLoading"](props.hdid)
);
const medications = computed<MedicationStatementHistory[]>(() =>
    store.getters["medication/medications"](props.hdid)
);

const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    medications.value
        .filter(
            (record) =>
                props.filter.allowsDate(record.dispensedDate) &&
                props.filter.allowsMedication(
                    record.medicationSummary.brandName
                )
        )
        .sort((a, b) => {
            const firstDate = new DateWrapper(a.dispensedDate);
            const secondDate = new DateWrapper(b.dispensedDate);

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
    visibleRecords.value.map<MedicationRow>((x) => ({
        date: DateWrapper.format(x.dispensedDate),
        din_pin: x.medicationSummary.din,
        brand: x.medicationSummary.brandName,
        generic: x.medicationSummary.genericName || notFoundText,
        practitioner: x.practitionerSurname || "",
        quantity:
            x.medicationSummary.quantity === undefined
                ? ""
                : x.medicationSummary.quantity.toString(),
        strength:
            (x.medicationSummary.strength || "") +
                (x.medicationSummary.strengthUnit || "") || notFoundText,
        form: x.medicationSummary.form || notFoundText,
        manufacturer: x.medicationSummary.manufacturer || notFoundText,
    }))
);

function retrieveMedications(hdid: string): Promise<void> {
    return store.dispatch("medication/retrieveMedications", { hdid });
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
        template: TemplateType.Medication,
        type: reportFormatType,
    });
}

watch(medicationsAreLoading, () => {
    emit("on-is-loading-changed", medicationsAreLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

retrieveMedications(props.hdid).catch((err) =>
    logger.error(`Error loading medication data: ${err}`)
);
</script>

<template>
    <div>
        <section>
            <b-row v-if="isEmpty && !medicationsAreLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-else-if="!isDependent"
                :striped="true"
                :fixed="true"
                :busy="medicationsAreLoading"
                :items="items"
                :fields="fields"
                data-testid="medication-history-report-table"
                class="table-style d-none d-md-table"
            >
                <template #table-busy>
                    <content-placeholders>
                        <content-placeholders-text :lines="7" />
                    </content-placeholders>
                </template>
            </b-table>
        </section>
        <ProtectiveWordComponent ref="protectiveWordModal" :hdid="hdid" />
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.medication-report-table-header {
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
