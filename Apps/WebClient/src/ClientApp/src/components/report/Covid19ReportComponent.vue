<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import { Covid19LaboratoryOrder } from "@/models/laboratory";
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

interface Covid19LaboratoryOrderRow {
    date: string;
    test_type: string;
    test_location: string;
    result: string;
}

const headerClass = "covid19-laboratory-report-table-header";

const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        tdAttr: { "data-testid": "covid19DateItem" },
    },
    {
        key: "test_type",
        thClass: headerClass,
        tdAttr: { "data-testid": "covid19TestTypeItem" },
    },
    {
        key: "test_location",
        thClass: headerClass,
        tdAttr: { "data-testid": "covid19LocationItem" },
    },
    {
        key: "result",
        thClass: headerClass,
        tdAttr: { "data-testid": "covid19ResultItem" },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const store = useStore();

const covid19LaboratoryOrders = computed<
    (hdid: string) => Covid19LaboratoryOrder[]
>(() => store.getters["laboratory/covid19LaboratoryOrders"]);

const covid19LaboratoryOrdersAreLoading = computed<(hdid: string) => boolean>(
    () => store.getters["laboratory/covid19LaboratoryOrdersAreLoading"]
);

const isEmpty = computed(() => visibleRecords.value.length === 0);

const isCovid19LaboratoryLoading = computed(() =>
    covid19LaboratoryOrdersAreLoading.value(props.hdid)
);

const visibleRecords = computed(() =>
    covid19LaboratoryOrders
        .value(props.hdid)
        .filter((r) =>
            props.filter.allowsDate(r.labResults[0].collectedDateTime)
        )
        .sort((a, b) => {
            const firstDate = new DateWrapper(
                a.labResults[0].collectedDateTime
            );
            const secondDate = new DateWrapper(
                b.labResults[0].collectedDateTime
            );

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
    visibleRecords.value.map<Covid19LaboratoryOrderRow>((x) => {
        const labResult = x.labResults[0];
        return {
            date: DateWrapper.format(labResult.collectedDateTime),
            test_type: labResult.testType,
            test_location: x.location || "",
            result: labResult.filteredLabResultOutcome,
        };
    })
);

function retrieveCovid19LaboratoryOrders(hdid: string): Promise<void> {
    return store.dispatch("laboratory/retrieveCovid19LaboratoryOrders", {
        hdid,
    });
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
        template: TemplateType.COVID,
        type: reportFormatType,
    });
}

watch(isCovid19LaboratoryLoading, () => {
    emit("on-is-loading-changed", isCovid19LaboratoryLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

retrieveCovid19LaboratoryOrders(props.hdid).catch((err) =>
    logger.error(`Error loading Covid19 data: ${err}`)
);
</script>

<template>
    <div>
        <section>
            <b-row v-if="isEmpty && !isCovid19LaboratoryLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-else-if="!isDependent"
                :striped="true"
                :fixed="true"
                :busy="isCovid19LaboratoryLoading"
                :items="items"
                :fields="fields"
                data-testid="covid19-report-table"
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

.covid19-laboratory-report-table-header {
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
