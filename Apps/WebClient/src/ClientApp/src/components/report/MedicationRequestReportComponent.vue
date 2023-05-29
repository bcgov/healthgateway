<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequest from "@/models/medicationRequest";
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

const headerClass = "medication-request-report-table-header";
const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        thStyle: { width: "10%" },
        tdAttr: { "data-testid": "medicationRequestDateItem" },
    },
    {
        key: "requested_drug_name",
        thClass: headerClass,
        thStyle: { width: "20%" },
    },
    {
        key: "status",
        thClass: headerClass,
        thStyle: { width: "9%" },
    },
    {
        key: "prescriber_name",
        thClass: headerClass,
        thStyle: { width: "15%" },
    },
    {
        key: "effective_date",
        thClass: headerClass,
        thStyle: { width: "15%" },
    },
    {
        key: "expiry_date",
        thClass: headerClass,
        thStyle: { width: "15%" },
    },
    {
        key: "reference_number",
        thClass: headerClass,
        thStyle: { width: "16%" },
    },
];

const store = useStore();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const specialAuthorityRequestsAreLoading = computed<boolean>(() =>
    store.getters["medication/specialAuthorityRequestsAreLoading"](props.hdid)
);
const specialAuthorityRequests = computed<MedicationRequest[]>(() =>
    store.getters["medication/specialAuthorityRequests"](props.hdid)
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

function retrieveSpecialAuthorityRequests(hdid: string): Promise<void> {
    return store.dispatch("medication/retrieveSpecialAuthorityRequests", {
        hdid,
    });
}
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
retrieveSpecialAuthorityRequests(props.hdid).catch((err) =>
    logger.error(`Error loading Special Authority requests data: ${err}`)
);
</script>

<template>
    <div>
        <section>
            <b-row v-if="isEmpty && !specialAuthorityRequestsAreLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-else-if="!isDependent"
                :striped="true"
                :fixed="true"
                :busy="specialAuthorityRequestsAreLoading"
                :items="items"
                :fields="fields"
                data-testid="medication-request-report-table"
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
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.medication-request-report-table-header {
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
