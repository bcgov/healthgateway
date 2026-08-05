<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";

import HgCardComponent from "@/components/common/HgCardComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import RecommendationsDialogComponent from "@/components/private/reports/RecommendationsDialogComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import type { Dependent } from "@/models/dependent";
import { LoadStatus } from "@/models/storeOperations";
import VaccineRecordState from "@/models/vaccineRecordState";
import { Action, Destination, Origin, Text } from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import { useGrid } from "@/utility/useGrid";

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const configStore = useConfigStore();
const vaccinationStatusStore = useVaccinationStatusAuthenticatedStore();

const router = useRouter();
const { columns } = useGrid();

const recommendationsDialogComponent =
    ref<InstanceType<typeof RecommendationsDialogComponent>>();
const vaccineRecordResultModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const vaccineRecordState = computed<VaccineRecordState>(() =>
    vaccinationStatusStore.vaccineRecordState(props.dependent.ownerId)
);
const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const showRecommendations = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.homepage
            .showRecommendationsLink
);
const vaccineRecordStatusMessage = computed(
    () => vaccineRecordState.value.statusMessage
);
const vaccineRecordResultMessage = computed(
    () => vaccineRecordState.value.resultMessage
);

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    vaccinationStatusStore.stopVaccineRecordDownload(hdid);
}

function handleClickHealthRecordsButton(): void {
    trackingService.trackEvent({
        action: Action.Visit,
        text: Text.InternalLink,
        destination: Destination.Timeline,
        origin: Origin.Dependents,
    });
    router.push({
        path: `/dependents/${props.dependent.ownerId}/timeline`,
    });
}

function showRecommendationsDialog(): void {
    trackingService.trackEvent({
        action: Action.Visit,
        text: Text.InternalLink,
        destination: Destination.ImmunizationRecommendationDialog,
        origin: Origin.Dependents,
    });
    recommendationsDialogComponent.value?.showDialog();
}

watch(vaccineRecordState, () => {
    if (vaccineRecordState.value.resultMessage.length > 0) {
        vaccineRecordResultModal.value?.showModal();
    }

    if (
        vaccineRecordState.value.record !== undefined &&
        vaccineRecordState.value.status === LoadStatus.LOADED &&
        vaccineRecordState.value.download
    ) {
        const mimeType = vaccineRecordState.value.record.document.mediaType;
        const downloadLink = `data:${mimeType};base64,${vaccineRecordState.value.record.document.data}`;
        fetch(downloadLink).then((res) => {
            res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
        });
        stopAuthenticatedVaccineRecordDownload(props.dependent.ownerId);
    }
});
</script>

<template>
    <LoadingComponent
        :is-loading="isVaccineRecordDownloading"
        :text="vaccineRecordStatusMessage"
    />
    <v-row>
        <v-col :cols="columns" class="d-flex">
            <HgCardComponent
                title="Health Records"
                density="compact"
                class="flex-grow-1 ma-1"
                :data-testid="`dependent-health-records-button-${dependent.ownerId}`"
                @click="handleClickHealthRecordsButton"
            >
                <template #icon>
                    <img
                        src="@/assets/images/gov/health-gateway-logo.svg"
                        alt="Health Gateway Logo"
                        :height="25"
                    />
                </template>
                <template #action-icon>
                    <v-icon icon="chevron-right" color="primary" size="small" />
                </template>
            </HgCardComponent>
        </v-col>
        <v-col v-if="showRecommendations" :cols="columns" class="d-flex">
            <HgCardComponent
                title="Vaccine Recommendations"
                density="compact"
                class="flex-grow-1 ma-1"
                :data-testid="`recommendations-card-${dependent.ownerId}`"
                @click="showRecommendationsDialog()"
            >
                <template #icon>
                    <v-icon
                        icon="calendar-check"
                        color="primary"
                        size="small"
                    />
                </template>
            </HgCardComponent>
        </v-col>
    </v-row>
    <RecommendationsDialogComponent
        ref="recommendationsDialogComponent"
        :hdid="dependent.ownerId"
        :is-dependent="true"
    />
    <MessageModalComponent
        ref="vaccineRecordResultModal"
        ok-only
        title="Alert"
        :message="vaccineRecordResultMessage"
    />
</template>
