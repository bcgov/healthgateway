<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTableComponent from "@/components/common/HgDataTableComponent.vue";
import ProtectiveWordComponent from "@/components/site/ProtectiveWordComponent.vue";
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
import { useMedicationStore } from "@/stores/medication";
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

const notFoundText = "Not Found";
const fields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        tdAttr: { "data-testid": "medicationDateItem" },
    },
    {
        key: "din_pin",
        title: "DIN/PIN",
    },
    {
        key: "brand",
        title: "Brand",
        tdAttr: { "data-testid": "medicationReportBrandNameItem" },
    },
    {
        key: "generic",
        title: "Generic",
    },
    {
        key: "practitioner",
        title: "Practitioner",
    },
    {
        key: "quantity",
        title: "Quantity",
    },
    {
        key: "strength",
        title: "Strength",
    },
    {
        key: "form",
        title: "Form",
    },
    {
        key: "manufacturer",
        title: "Manufacturer",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const medicationStore = useMedicationStore();

const areMedicationsLoading = computed(() =>
    medicationStore.areMedicationsLoading(props.hdid)
);
const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    medicationStore
        .medications(props.hdid)
        .filter(
            (record) =>
                props.filter.allowsDate(record.dispensedDate) &&
                props.filter.allowsMedication(
                    record.medicationSummary.brandName
                )
        )
        .sort((a, b) =>
            DateSortUtility.descendingByString(a.dispensedDate, b.dispensedDate)
        )
);
const items = computed(() =>
    visibleRecords.value.map<MedicationRow>((x) => ({
        date: DateWrapper.format(x.dispensedDate),
        din_pin: x.medicationSummary.din,
        brand: x.medicationSummary.brandName,
        generic: x.medicationSummary.genericName || notFoundText,
        practitioner: x.practitionerSurname ?? "",
        quantity:
            x.medicationSummary.quantity === undefined
                ? ""
                : x.medicationSummary.quantity.toString(),
        strength:
            (x.medicationSummary.strength ?? "") +
                (x.medicationSummary.strengthUnit ?? "") || notFoundText,
        form: x.medicationSummary.form || notFoundText,
        manufacturer: x.medicationSummary.manufacturer || notFoundText,
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
        template: TemplateType.Medication,
        type: reportFormatType,
    });
}

watch(areMedicationsLoading, () => {
    emit("on-is-loading-changed", areMedicationsLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

medicationStore
    .retrieveMedications(props.hdid)
    .catch((err) => logger.error(`Error loading medication data: ${err}`));
</script>

<template>
    <div>
        <section>
            <v-row v-if="isEmpty && !areMedicationsLoading">
                <v-col>No records found.</v-col>
            </v-row>
            <HgDataTableComponent
                v-else-if="!isDependent"
                class="d-none d-md-block"
                :items="items"
                :fields="fields"
                density="compact"
                :loading="areMedicationsLoading"
                data-testid="medication-history-report-table"
            />
        </section>
        <ProtectiveWordComponent ref="protectiveWordModal" :hdid="hdid" />
    </div>
</template>
