<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder } from "@/models/laboratory";
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

interface LabTestRow {
    date: string;
    test: string;
    result: string;
    status: string;
}

const headerClass = "laboratory-test-report-table-header";

const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        tdAttr: { "data-testid": "labResultDateItem" },
    },
    {
        key: "test",
        thClass: headerClass,
        tdAttr: { "data-testid": "labResultTestTypeItem" },
    },
    {
        key: "result",
        thClass: headerClass,
        tdAttr: { "data-testid": "labResultItem" },
    },
    {
        key: "status",
        thClass: headerClass,
        tdAttr: { "data-testid": "labStatusItem" },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const reportService = container.get<IReportService>(
    SERVICE_IDENTIFIER.ReportService
);
const store = useStore();

const laboratoryOrders = computed<(hdid: string) => LaboratoryOrder[]>(
    () => store.getters["laboratory/laboratoryOrders"]
);

const laboratoryOrdersAreLoading = computed<(hdid: string) => boolean>(
    () => store.getters["laboratory/laboratoryOrdersAreLoading"]
);

const isEmpty = computed(() => visibleRecords.value.length === 0);

const isLaboratoryLoading = computed(() =>
    laboratoryOrdersAreLoading.value(props.hdid)
);

const visibleRecords = computed(() =>
    laboratoryOrders
        .value(props.hdid)
        .filter((record) => props.filter.allowsDate(record.timelineDateTime))
        .sort((a, b) => {
            const firstDate = new DateWrapper(a.timelineDateTime);
            const secondDate = new DateWrapper(b.timelineDateTime);

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
    visibleRecords.value.flatMap<LabTestRow>((x) => {
        const timelineDateTime = DateWrapper.format(x.timelineDateTime);
        return x.laboratoryTests.map<LabTestRow>((y) => ({
            date: timelineDateTime,
            test: y.batteryType || "",
            result: y.result,
            status: y.testStatus,
        }));
    })
);

function retrieveLaboratoryOrders(hdid: string): Promise<void> {
    return store.dispatch("laboratory/retrieveLaboratoryOrders", {
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
        template: TemplateType.Laboratory,
        type: reportFormatType,
    });
}

watch(isLaboratoryLoading, () => {
    emit("on-is-loading-changed", isLaboratoryLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

retrieveLaboratoryOrders(props.hdid).catch((err) =>
    logger.error(`Error loading Laboratory data: ${err}`)
);
</script>

<template>
    <div>
        <section>
            <b-row v-if="isEmpty && !isLaboratoryLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-else-if="!isDependent"
                :striped="true"
                :fixed="true"
                :busy="isLaboratoryLoading"
                :items="items"
                :fields="fields"
                data-testid="laboratory-report-table"
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

.laboratory-test-report-table-header {
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
