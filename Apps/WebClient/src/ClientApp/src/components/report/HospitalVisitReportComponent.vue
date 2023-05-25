<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import { HospitalVisit } from "@/models/encounter";
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

interface HospitalVisitRow {
    date: string;
    health_service: string;
    visit_type: string;
    location: string;
    provider: string;
}

const headerClass = "hospital-visit-report-table-header";

const fields: ReportField[] = [
    {
        key: "date",
        thClass: headerClass,
        tdAttr: { "data-testid": "hospital-visit-date" },
    },
    {
        key: "health_service",
        thClass: headerClass,
    },
    {
        key: "visit_type",
        thClass: headerClass,
    },
    {
        key: "location",
        thClass: headerClass,
    },
    {
        key: "provider",
        thClass: headerClass,
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const hospitalVisits = computed<(hdid: string) => HospitalVisit[]>(
    () => store.getters["encounter/hospitalVisits"]
);

const hospitalVisitsAreLoading = computed<(hdid: string) => boolean>(
    () => store.getters["encounter/hospitalVisitsAreLoading"]
);

const isEmpty = computed(() => visibleRecords.value.length === 0);

const isLoading = computed(() => hospitalVisitsAreLoading.value(props.hdid));

const visibleRecords = computed(() =>
    hospitalVisits
        .value(props.hdid)
        .filter((record) => props.filter.allowsDate(record.admitDateTime))
        .sort((a, b) => {
            const firstDate = new DateWrapper(a.admitDateTime);
            const secondDate = new DateWrapper(b.admitDateTime);

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
    visibleRecords.value.map<HospitalVisitRow>((x) => {
        return {
            date: DateWrapper.format(x.admitDateTime),
            health_service: x.healthService,
            visit_type: x.visitType,
            location: x.facility,
            provider: x.provider,
        };
    })
);

function retrieveHospitalVisits(hdid: string): Promise<void> {
    return store.dispatch("encounter/retrieveHospitalVisits", { hdid });
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
        template: TemplateType.HospitalVisit,
        type: reportFormatType,
    });
}

watch(isLoading, () => {
    emit("on-is-loading-changed", isLoading.value);
});

watch(isEmpty, () => {
    emit("on-is-empty-changed", isEmpty.value);
});

onMounted(() => {
    emit("on-is-empty-changed", isEmpty.value);
});

retrieveHospitalVisits(props.hdid).catch((err) =>
    logger.error(`Error loading hospital visit data: ${err}`)
);
</script>

<template>
    <div>
        <div>
            <section>
                <b-row v-if="isEmpty && !isLoading">
                    <b-col>No records found.</b-col>
                </b-row>

                <b-table
                    v-else-if="!isDependent"
                    :striped="true"
                    :fixed="true"
                    :busy="isLoading"
                    :items="items"
                    :fields="fields"
                    data-testid="hospital-visit-report-table"
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

.hospital-visit-report-table-header {
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
