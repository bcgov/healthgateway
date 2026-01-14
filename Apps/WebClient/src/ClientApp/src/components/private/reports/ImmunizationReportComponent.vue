<script setup lang="ts">
import { computed, onMounted, useSlots, watch } from "vue";

import HgDataTableComponent from "@/components/common/HgDataTableComponent.vue";
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
import { Action, Destination, Origin, Text, Type } from "@/plugins/extensions";
import {
    ILogger,
    IReportService,
    ITrackingService,
} from "@/services/interfaces";
import { useImmunizationStore } from "@/stores/immunization";
import DateSortUtility from "@/utility/dateSortUtility";

interface Props {
    hdid: string;
    filter: ReportFilter;
    isDependent?: boolean;
    template?: TemplateType;
    hideImmunizations?: boolean;
    hideRecommendations?: boolean;
    hideRecommendationHeader?: boolean;
    forceShow?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
    template: undefined,
    hideImmunizations: false,
    hideRecommendations: false,
    hideRecommendationHeader: false,
    forceShow: false,
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
        tdAlign: "start",
        thAlign: "start",
        thAttr: { "data-testid": "recommendationTitle" },
        tdAttr: {
            "data-testid": "recommendationItem",
        },
    },
    {
        key: "due_date",
        title: "Due Date",
        thAttr: { "data-testid": "recommendationDateTitle" },
        tdAttr: {
            "data-testid": "recommendationDateItem",
            style: "white-space: nowrap",
        },
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const immunizationStore = useImmunizationStore();
const slots = useSlots();

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
        .filter((record) =>
            props.filter.allowsDate(
                DateWrapper.fromIsoDate(record.dateOfImmunization)
            )
        )
        .sort((a, b) =>
            DateSortUtility.descending(
                DateWrapper.fromIsoDate(a.dateOfImmunization),
                DateWrapper.fromIsoDate(b.dateOfImmunization)
            )
        )
);
const immunizationItems = computed(() =>
    visibleImmunizations.value.map<ImmunizationRow>((x) => ({
        date: DateWrapper.fromIsoDate(x.dateOfImmunization).format(),
        immunization: x.immunization.name,
        agents: x.immunization.immunizationAgents,
        provider_clinic: x.providerOrClinic,
    }))
);
const visibleRecommendations = computed(() =>
    immunizationStore
        .recommendations(props.hdid)
        .filter((x) => x.recommendedVaccinations)
);
const recommendationItems = computed(() =>
    visibleRecommendations.value.map<RecommendationRow>((x) => ({
        immunization: x.recommendedVaccinations,
        due_date: DateWrapper.fromIsoDate(x.agentDueDate).format(),
    }))
);
const hasRecommendationsSlot = computed(
    () => slots["recommendations-description"] !== undefined
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
        template:
            props.template ??
            (props.isDependent
                ? TemplateType.DependentImmunization
                : TemplateType.Immunization),
        type: reportFormatType,
    });
}

function onIsEmptyChanged(): void {
    emit("on-is-empty-changed", isEmpty.value && isRecommendationEmpty.value);
}

function trackLink(href: string, text: Text, destination: Destination) {
    trackingService.trackEvent({
        action: Action.ExternalLink,
        text: text,
        destination: destination,
        origin: Origin.Download,
        type: Type.Immunizations,
    });
    window.open(href, "_blank", "noopener");
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
        <section
            v-else-if="!isDependent || forceShow"
            :class="forceShow ? '' : 'd-none d-md-block'"
        >
            <template v-if="!hideImmunizations">
                <h4 class="text-h6 font-weight-bold mb-2">
                    Immunization Record
                </h4>
                <p>
                    Health Gateway shows immunizations from public health
                    clinics and pharmacies in B.C. If you got vaccinated at a
                    pharmacy, try searching your medications, too.
                </p>
                <p>
                    You can add or update immunizations by visiting
                    <a
                        href="https://www.immunizationrecord.gov.bc.ca/"
                        class="text-link"
                        @click.prevent="
                            trackLink(
                                'https://www.immunizationrecord.gov.bc.ca/',
                                Text.ImmunizationRecordDownload,
                                Destination.ImmunizationRecordBC
                            )
                        "
                        >immunizationrecord.gov.bc.ca</a
                    >.
                </p>
                <p v-if="isEmpty && !isLoading">No records found.</p>
                <HgDataTableComponent
                    v-if="!isEmpty || isLoading"
                    class="d-none d-md-block"
                    :loading="isLoading"
                    :items="immunizationItems"
                    :fields="immunizationFields"
                    density="compact"
                    data-testid="immunization-history-report-table"
                >
                    <template #header-agents>
                        <v-row no-gutters align="center">
                            <v-col class="pr-4">Agent</v-col>
                            <v-col class="px-4">Product</v-col>
                            <v-col class="pl-4">Lot Number</v-col>
                        </v-row>
                    </template>
                    <template #item-agents="data">
                        <v-row
                            v-for="(agent, index) in data.item.agents"
                            :key="index"
                            no-gutters
                        >
                            <v-col class="pr-4">{{ agent.name }}</v-col>
                            <v-col class="px-4">{{ agent.productName }}</v-col>
                            <v-col class="pl-4">{{ agent.lotNumber }}</v-col>
                        </v-row>
                    </template>
                </HgDataTableComponent>
            </template>
            <div
                v-if="!hideRecommendations"
                :class="{ 'mt-4': !hideImmunizations }"
            >
                <h4
                    v-if="!hideRecommendationHeader"
                    class="text-h6 font-weight-bold mb-2"
                >
                    Recommended Immunizations
                </h4>
                <template v-if="hasRecommendationsSlot">
                    <slot name="recommendations-description" />
                </template>
                <template v-else>
                    <p>
                        Recommended immunizations are based on the
                        <a
                            href="https://www.healthlinkbc.ca/health-library/immunizations/schedules"
                            class="text-link"
                            @click.prevent="
                                trackLink(
                                    'https://www.healthlinkbc.ca/health-library/immunizations/schedules',
                                    Text.BcVaccineSchedule,
                                    Destination.ImmunizationSchedule
                                )
                            "
                            >BC Vaccine Schedule</a
                        >. For more information, please visit
                        <a
                            href="https://www.healthlinkbc.ca/health-library/immunizations"
                            class="text-link"
                            @click.prevent="
                                trackLink(
                                    'https://www.healthlinkbc.ca/health-library/immunizations',
                                    Text.ImmunizationsHealthLinkBC,
                                    Destination.ImmunizationsHealthLinkBC
                                )
                            "
                            >Immunizations | HealthLink BC</a
                        >.
                    </p>
                </template>
                <p v-if="isRecommendationEmpty && !isLoading">
                    No recommendations found.
                </p>
                <HgDataTableComponent
                    v-if="!isRecommendationEmpty || isLoading"
                    :class="forceShow ? '' : 'd-none d-md-block'"
                    fixed-header
                    :loading="isLoading"
                    :items="recommendationItems"
                    :fields="recommendationFields"
                    data-testid="recommendation-history-report-table"
                />
            </div>
        </section>
    </div>
</template>
