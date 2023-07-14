<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTable from "@/components/common/HgDataTable.vue";
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

interface Props {
    hdid: string;
    filter: ReportFilter;
    isDependent: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
});

const emit = defineEmits<{
    (e: "on-is-loading-changed", newValue: boolean): void;
    (e: "on-is-empty-changed", newValue: boolean): void;
}>();

defineExpose({ generateReport });

interface MedicationRequestRow {
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

const specialAuthorityRequestsAreLoading = computed<boolean>(() =>
    specialAuthorityStore.specialAuthorityRequestsAreLoading(props.hdid)
);
const specialAuthorityRequests = computed<MedicationRequest[]>(() =>
    specialAuthorityStore.specialAuthorityRequests(props.hdid)
);

const isEmpty = computed<boolean>(() => visibleRecords.value.length === 0);

const visibleRecords = computed<MedicationRequest[]>(() => {
    const records = specialAuthorityRequests.value.filter(
        (record: MedicationRequest) =>
            props.filter.allowsDate(record.requestedDate)
    );
    records.sort((a: MedicationRequest, b: MedicationRequest) => {
        const firstDate = new DateWrapper(a.requestedDate);
        const secondDate = new DateWrapper(b.requestedDate);

        if (firstDate.isBefore(secondDate)) {
            return 1;
        }

        if (firstDate.isAfter(secondDate)) {
            return -1;
        }

        return 0;
    });
    return records;
});

const items = computed<MedicationRequestRow[]>(() => {
    return visibleRecords.value.map<MedicationRequestRow>(
        (x: MedicationRequest) => ({
            date: DateWrapper.format(x.requestedDate),
            requested_drug_name: x.drugName || "",
            status: x.requestStatus || "",
            prescriber_name: prescriberName(x),
            effective_date:
                x.effectiveDate === undefined
                    ? ""
                    : DateWrapper.format(x.effectiveDate),
            expiry_date:
                x.expiryDate === undefined
                    ? ""
                    : DateWrapper.format(x.expiryDate),
            reference_number: x.referenceNumber,
        })
    );
});

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
            <HgDataTable
                v-else-if="!isDependent"
                class="d-none d-md-block"
                fixed-header
                :loading="specialAuthorityRequestsAreLoading"
                :items="items"
                :fields="fields"
                density="compact"
                height="600px"
                data-testid="medication-request-report-table"
            />
        </section>
    </div>
</template>
