<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTableComponent from "@/components/common/HgDataTableComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequest from "@/models/medicationRequest";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger, IReportService } from "@/services/interfaces";
import { useSpecialAuthorityRequestStore } from "@/stores/specialAuthorityRequest";
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

interface SpecialAuthorityRequestRow {
    date: string;
    requested_drug_name: string;
    status: string;
    prescriber_name: string;
    effective_date: string;
    expiry_date: string;
    reference_number: string;
}

const fields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        tdAttr: { "data-testid": "medicationRequestDateItem" },
    },
    {
        key: "requested_drug_name",
        title: "Requested Drug Name",
    },
    {
        key: "status",
        title: "Status",
    },
    {
        key: "prescriber_name",
        title: "Prescriber Name",
    },
    {
        key: "effective_date",
        title: "Effective Date",
    },
    {
        key: "expiry_date",
        title: "Expiry Date",
    },
    {
        key: "reference_number",
        title: "Reference Number",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const specialAuthorityStore = useSpecialAuthorityRequestStore();

const specialAuthorityRequestsAreLoading = computed(() =>
    specialAuthorityStore.specialAuthorityRequestsAreLoading(props.hdid)
);
const isEmpty = computed(() => visibleRecords.value.length === 0);
const visibleRecords = computed(() =>
    specialAuthorityStore
        .specialAuthorityRequests(props.hdid)
        .filter((record) =>
            props.filter.allowsDate(
                DateWrapper.fromIsoDate(record.requestedDate)
            )
        )
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIsoDate(a.requestedDate),
                DateWrapper.fromIsoDate(b.requestedDate)
            )
        )
);
const items = computed(() =>
    visibleRecords.value.map<SpecialAuthorityRequestRow>((x) => ({
        date: DateWrapper.fromIsoDate(x.requestedDate).format(),
        requested_drug_name: x.drugName ?? "",
        status: x.requestStatus ?? "",
        prescriber_name: prescriberName(x),
        effective_date: DateWrapper.fromIsoDate(x.effectiveDate).format(),
        expiry_date: DateWrapper.fromIsoDate(x.expiryDate).format(),
        reference_number: x.referenceNumber,
    }))
);

function prescriberName(medication: MedicationRequest): string {
    return (
        (medication.prescriberFirstName || " ") +
        " " +
        (medication.prescriberLastName || " ")
    );
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
        template: TemplateType.MedicationRequest,
        type: reportFormatType,
    });
}

function onIsLoadingChanged(): void {
    emit("on-is-loading-changed", specialAuthorityRequestsAreLoading.value);
}

function onIsEmptyChanged(): void {
    emit("on-is-empty-changed", isEmpty.value);
}

watch(specialAuthorityRequestsAreLoading, () => {
    onIsLoadingChanged();
});

watch(isEmpty, () => {
    onIsEmptyChanged();
});

onMounted(() => {
    onIsEmptyChanged();
});

// Created Hook
specialAuthorityStore
    .retrieveSpecialAuthorityRequests(props.hdid)
    .catch((err) =>
        logger.error(`Error loading Special Authority requests data: ${err}`)
    );
</script>

<template>
    <div>
        <section>
            <v-row v-if="isEmpty && !specialAuthorityRequestsAreLoading">
                <v-col>No records found.</v-col>
            </v-row>
            <HgDataTableComponent
                v-else-if="!isDependent"
                class="d-none d-md-block"
                :loading="specialAuthorityRequestsAreLoading"
                :items="items"
                :fields="fields"
                density="compact"
                data-testid="medication-request-report-table"
            />
        </section>
    </div>
</template>
