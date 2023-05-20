<script setup lang="ts">
import { computed, onMounted, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { DateWrapper } from "@/models/dateWrapper";
import {
    ImmunizationAgent,
    ImmunizationEvent,
    Recommendation,
} from "@/models/immunizationModel";
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

interface ImmunizationRow {
    date: string;
    immunization: string;
    agents: ImmunizationAgent[];
    provider_clinic: string;
}

interface RecomendationRow {
    immunization: string;
    due_date: string;
}

const headerClass = "immunization-report-table-header";

const immunizationFields: ReportField[] = [
    {
        key: "date",
        thStyle: { width: "15%" },
        thClass: headerClass,
        thAttr: { "data-testid": "immunizationDateTitle" },
        tdAttr: { "data-testid": "immunizationDateItem" },
    },
    {
        key: "immunization",
        thStyle: { width: "25%" },
        thClass: headerClass,
        thAttr: { "data-testid": "immunizationNameTitle" },
        tdAttr: { "data-testid": "immunizationNameItem" },
    },
    {
        key: "agents",
        thStyle: { width: "45%" },
        thClass: headerClass,
        thAttr: { "data-testid": "immunizationAgentTitle" },
        tdAttr: { "data-testid": "immunizationAgentItem" },
    },
    {
        key: "provider_clinic",
        label: "Provider / Clinic",
        thStyle: { width: "15%" },
        thClass: headerClass,
        thAttr: { "data-testid": "immunizationProviderClinicTitle" },
        tdAttr: { "data-testid": "immunizationProviderClinicItem" },
    },
];

const recomendationFields: ReportField[] = [
    {
        key: "immunization",
        thStyle: { width: "50%" },
        thClass: headerClass,
        thAttr: { "data-testid": "recommendationTitle" },
        tdAttr: { "data-testid": "recommendationItem" },
    },
    {
        key: "due_date",
        thStyle: { width: "50%" },
        thClass: headerClass,
        thAttr: { "data-testid": "recommendationDateTitle" },
        tdAttr: { "data-testid": "recommendationDateItem" },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const patientImmunizations = computed<(hdid: string) => ImmunizationEvent[]>(
    () => store.getters["immunization/immunizations"]
);

const patientRecommendations = computed<(hdid: string) => Recommendation[]>(
    () => store.getters["immunization/recomendations"]
);

const immunizationsAreLoading = computed<(hdid: string) => boolean>(
    () => store.getters["immunization/immunizationsAreLoading"]
);

const immunizationsAreDeferred = computed<(hdid: string) => boolean>(
    () => store.getters["immunization/immunizationsAreDeferred"]
);

const isEmpty = computed(() => visibleImmunizations.value.length === 0);

const isRecommendationEmpty = computed(
    () => visibleRecomendations.value.length === 0
);

const isLoading = computed(
    () =>
        immunizationsAreDeferred.value(props.hdid) ||
        immunizationsAreLoading.value(props.hdid)
);

const visibleImmunizations = computed(() =>
    patientImmunizations
        .value(props.hdid)
        .filter((record) => props.filter.allowsDate(record.dateOfImmunization))
        .sort((a, b) => {
            const firstDate = new DateWrapper(a.dateOfImmunization);
            const secondDate = new DateWrapper(b.dateOfImmunization);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        })
);

const immunizationItems = computed(() =>
    visibleImmunizations.value.map<ImmunizationRow>((x) => {
        return {
            date: DateWrapper.format(x.dateOfImmunization),
            immunization: x.immunization.name,
            agents: x.immunization.immunizationAgents,
            provider_clinic: x.providerOrClinic,
        };
    })
);

const visibleRecomendations = computed(() =>
    patientRecommendations
        .value(props.hdid)
        .filter((x) => x.recommendedVaccinations)
        .sort((a, b) => {
            const firstDateEmpty =
                a.agentDueDate === null || a.agentDueDate === undefined;
            const secondDateEmpty =
                b.agentDueDate === null || b.agentDueDate === undefined;

            if (firstDateEmpty && secondDateEmpty) {
                return 0;
            }

            if (firstDateEmpty) {
                return 1;
            }

            if (secondDateEmpty) {
                return -1;
            }

            const firstDate = new DateWrapper(a.agentDueDate);
            const secondDate = new DateWrapper(b.agentDueDate);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        })
);

const recomendationItems = computed(() =>
    visibleRecomendations.value.map<RecomendationRow>((x) => {
        return {
            immunization: x.recommendedVaccinations,
            due_date:
                x.agentDueDate === undefined || x.agentDueDate === null
                    ? ""
                    : DateWrapper.format(x.agentDueDate),
        };
    })
);

function retrieveImmunizations(hdid: string): Promise<void> {
    return store.dispatch("immunization/retrieveImmunizations", { hdid });
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
            records: immunizationItems.value,
            recommendations: recomendationItems.value,
        },
        template: props.isDependent
            ? TemplateType.DependentImmunization
            : TemplateType.Immunization,
        type: reportFormatType,
    });
}

watch(isLoading, () => {
    emit("on-is-loading-changed", isLoading.value);
});

function onIsEmptyChanged(): void {
    emit("on-is-empty-changed", isEmpty.value && isRecommendationEmpty.value);
}

watch(isEmpty, () => {
    onIsEmptyChanged();
});

watch(isRecommendationEmpty, () => {
    onIsEmptyChanged();
});

onMounted(() => {
    onIsEmptyChanged();
});

logger.debug(`Retrieving immunizations for Hdid: ${props.hdid}`);
retrieveImmunizations(props.hdid).catch((err) =>
    logger.error(`Error loading immunization data: ${err}`)
);
</script>

<template>
    <div>
        <div v-if="isRecommendationEmpty && isEmpty && !isLoading">
            No records found.
        </div>
        <section v-else-if="!isDependent" class="d-none d-md-block">
            <b-row>
                <b-col>
                    <h4>Immunization History</h4>
                </b-col>
            </b-row>
            <b-row v-if="isEmpty && !isLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-if="!isEmpty || isLoading"
                striped
                :busy="isLoading"
                :items="immunizationItems"
                :fields="immunizationFields"
                data-testid="immunization-history-report-table"
                class="table-style"
            >
                <!-- A custom formatted header cell for field 'name' -->
                <template #head(agents)>
                    <b-row>
                        <b-col>Agent</b-col>
                        <b-col>Product</b-col>
                        <b-col>Lot Number</b-col>
                    </b-row>
                </template>
                <template #cell(agents)="data">
                    <b-row
                        v-for="(agent, index) in data.item.agents"
                        :key="index"
                    >
                        <b-col> {{ agent.name }} </b-col>
                        <b-col> {{ agent.productName }} </b-col>
                        <b-col> {{ agent.lotNumber }} </b-col>
                    </b-row>
                </template>
                <template #table-busy>
                    <content-placeholders>
                        <content-placeholders-text :lines="7" />
                    </content-placeholders>
                </template>
            </b-table>
            <b-row class="mt-3">
                <b-col class="col-7">
                    <b-row>
                        <b-col>
                            <h4>Recommended Immunizations</h4>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col>
                            <div id="disclaimer">
                                <p>
                                    Health Gateway shows immunizations from
                                    public health clinics and pharmacies in B.C.
                                    If you got vaccinated at a pharmacy, try
                                    searching your medications, too.
                                </p>
                                <p class="mb-0">
                                    You can add or update immunizations by
                                    visiting
                                    <a
                                        href="https://www.immunizationrecord.gov.bc.ca"
                                        target="_blank"
                                        rel="noopener"
                                        >immunizationrecord.gov.bc.ca</a
                                    >.
                                </p>
                            </div>
                        </b-col>
                    </b-row>
                    <b-row v-if="isRecommendationEmpty && !isLoading">
                        <b-col>No recommendations found.</b-col>
                    </b-row>
                    <b-table
                        v-if="!isRecommendationEmpty || isLoading"
                        :striped="true"
                        :fixed="true"
                        :busy="isLoading"
                        :items="recomendationItems"
                        :fields="recomendationFields"
                        data-testid="recommendation-history-report-table"
                        class="mt-2 table-style"
                    >
                        <template #table-busy>
                            <content-placeholders>
                                <content-placeholders-text :lines="5" />
                            </content-placeholders>
                        </template>
                    </b-table>
                </b-col>
            </b-row>
        </section>
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.immunization-report-table-header {
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

h4 {
    color: $primary;
}

#disclaimer {
    font-size: 0.7em;
    font-weight: bold !important;
}
</style>
