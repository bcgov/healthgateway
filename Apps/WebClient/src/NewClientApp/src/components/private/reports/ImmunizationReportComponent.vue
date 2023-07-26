<script setup lang="ts">
import { computed, onMounted, watch } from "vue";

import HgDataTable from "@/components/common/HgDataTable.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationAgent } from "@/models/immunizationModel";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger, IReportService } from "@/services/interfaces";
import { useImmunizationStore } from "@/stores/immunization";
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

interface ImmunizationRow {
    date: string;
    immunization: string;
    agents: ImmunizationAgent[];
    provider_clinic: string;
}

interface RecommendationRow {
    immunization: string;
    due_date: string;
}

const immunizationFields: ReportField[] = [
    {
        key: "date",
        title: "Date",
        width: "15%",
        thAttr: { "data-testid": "immunizationDateTitle" },
        tdAttr: { "data-testid": "immunizationDateItem" },
    },
    {
        key: "immunization",
        title: "Immunization",
        width: "25%",
        thAttr: { "data-testid": "immunizationNameTitle" },
        tdAttr: { "data-testid": "immunizationNameItem" },
    },
    {
        key: "agents",
        title: "Agents",
        width: "45%",
        thAttr: { "data-testid": "immunizationAgentTitle" },
        tdAttr: { "data-testid": "immunizationAgentItem" },
    },
    {
        key: "provider_clinic",
        title: "Provider / Clinic",
        width: "15%",
        thAttr: { "data-testid": "immunizationProviderClinicTitle" },
        tdAttr: { "data-testid": "immunizationProviderClinicItem" },
    },
];
const recommendationFields: ReportField[] = [
    {
        key: "immunization",
        title: "Immunization",
        thAttr: { "data-testid": "recommendationTitle" },
        tdAttr: { "data-testid": "recommendationItem" },
    },
    {
        key: "due_date",
        title: "Due Date",
        thAttr: { "data-testid": "recommendationDateTitle" },
        tdAttr: { "data-testid": "recommendationDateItem" },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const immunizationStore = useImmunizationStore();

const isEmpty = computed(() => visibleImmunizations.value.length === 0);
const isRecommendationEmpty = computed(
    () => visibleRecommendations.value.length === 0
);
const isLoading = computed(
    () =>
        immunizationStore.immunizationsAreDeferred(props.hdid) ||
        immunizationStore.immunizationsAreLoading(props.hdid)
);
const visibleImmunizations = computed(() =>
    immunizationStore
        .immunizations(props.hdid)
        .filter((record) => props.filter.allowsDate(record.dateOfImmunization))
        .sort((a, b) =>
            DateSortUtility.descendingByString(
                a.dateOfImmunization,
                b.dateOfImmunization
            )
        )
);
const immunizationItems = computed(() =>
    visibleImmunizations.value.map<ImmunizationRow>((x) => ({
        date: DateWrapper.format(x.dateOfImmunization),
        immunization: x.immunization.name,
        agents: x.immunization.immunizationAgents,
        provider_clinic: x.providerOrClinic,
    }))
);
const visibleRecommendations = computed(() =>
    immunizationStore
        .recommendations(props.hdid)
        .filter((x) => x.recommendedVaccinations)
        .sort((a, b) =>
            DateSortUtility.descendingByOptionalString(
                a.agentDueDate,
                b.agentDueDate
            )
        )
);
const recommendationItems = computed(() =>
    visibleRecommendations.value.map<RecommendationRow>((x) => ({
        immunization: x.recommendedVaccinations,
        due_date:
            x.agentDueDate === undefined || x.agentDueDate === null
                ? ""
                : DateWrapper.format(x.agentDueDate),
    }))
);

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
            recommendations: recommendationItems.value,
        },
        template: props.isDependent
            ? TemplateType.DependentImmunization
            : TemplateType.Immunization,
        type: reportFormatType,
    });
}

function onIsEmptyChanged(): void {
    emit("on-is-empty-changed", isEmpty.value && isRecommendationEmpty.value);
}

watch(isLoading, () => {
    emit("on-is-loading-changed", isLoading.value);
});

watch([isEmpty, isRecommendationEmpty], () => {
    onIsEmptyChanged();
});

onMounted(() => {
    onIsEmptyChanged();
});

logger.debug(`Retrieving immunizations for Hdid: ${props.hdid}`);
immunizationStore
    .retrieveImmunizations(props.hdid)
    .catch((err) => logger.error(`Error loading immunization data: ${err}`));
</script>

<template>
    <div>
        <div v-if="isRecommendationEmpty && isEmpty && !isLoading">
            No records found.
        </div>
        <section v-else-if="!isDependent" class="d-none d-md-block">
            <h4 class="text-h6 font-weight-bold mb-2">Immunization History</h4>
            <p v-if="isEmpty && !isLoading">No records found.</p>
            <HgDataTable
                v-if="!isEmpty || isLoading"
                class="d-none d-md-block"
                striped
                :loading="isLoading"
                :items="immunizationItems"
                :fields="immunizationFields"
                density="compact"
                data-testid="immunization-history-report-table"
            >
                <!-- A custom formatted header cell for field 'name' -->
                <template #header-agents>
                    <v-row>
                        <v-col>Agent</v-col>
                        <v-col>Product</v-col>
                        <v-col>Lot Number</v-col>
                    </v-row>
                </template>
                <template #item-agents="data">
                    <v-row
                        v-for="(agent, index) in data.item.agents"
                        :key="index"
                    >
                        <v-col> {{ agent.name }} </v-col>
                        <v-col> {{ agent.productName }} </v-col>
                        <v-col> {{ agent.lotNumber }} </v-col>
                    </v-row>
                </template>
            </HgDataTable>
            <v-row class="mt-4">
                <v-col class="col-7">
                    <h4 class="text-h6 font-weight-bold mb-2">
                        Recommended Immunizations
                    </h4>
                    <p>
                        Health Gateway shows immunizations from public health
                        clinics and pharmacies in B.C. If you got vaccinated at
                        a pharmacy, try searching your medications, too.
                    </p>
                    <p>
                        You can add or update immunizations by visiting
                        <a
                            href="https://www.immunizationrecord.gov.bc.ca"
                            target="_blank"
                            rel="noopener"
                            >immunizationrecord.gov.bc.ca</a
                        >.
                    </p>
                    <p v-if="isRecommendationEmpty && !isLoading">
                        No recommendations found.
                    </p>
                    <HgDataTable
                        v-if="!isRecommendationEmpty || isLoading"
                        class="d-none d-md-block"
                        fixed-header
                        :loading="isLoading"
                        :items="recommendationItems"
                        :fields="recommendationFields"
                        data-testid="recommendation-history-report-table"
                    />
                </v-col>
            </v-row>
        </section>
    </div>
</template>
