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
import {
    Action,
    Actor,
    Destination,
    Format,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import { getGridCols } from "@/utility/gridUtilty";

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

const recommendationsDialogComponent =
    ref<InstanceType<typeof RecommendationsDialogComponent>>();
const sensitiveDocumentDownloadModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const vaccineRecordResultModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const vaccineRecordState = computed<VaccineRecordState>(() =>
    vaccinationStatusStore.vaccineRecordState(props.dependent.ownerId)
);
const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const showFederalProofOfVaccination = computed(
    () =>
        configStore.webConfig.featureToggleConfiguration.homepage
            .showFederalProofOfVaccination
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

function retrieveAuthenticatedVaccineRecord(hdid: string): void {
    vaccinationStatusStore.retrieveVaccineRecord(hdid);
}

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    vaccinationStatusStore.stopVaccineRecordDownload(hdid);
}

function handleClickHealthRecordsButton(): void {
    trackingService.track({
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
    trackingService.track({
        action: Action.Visit,
        text: Text.InternalLink,
        destination: Destination.ImmunizationRecommendationDialog,
        origin: Origin.Dependents,
    });
    recommendationsDialogComponent.value?.showDialog();
}

function handleFederalProofOfVaccinationDownload(): void {
    trackingService.track({
        action: Action.Download,
        text: Text.Document,
        type: Type.Covid19ProofOfVaccination,
        format: Format.Pdf,
        actor: Actor.Guardian,
    });
    retrieveAuthenticatedVaccineRecord(props.dependent.ownerId);
}

function showSensitiveDocumentDownloadModal(): void {
    sensitiveDocumentDownloadModal.value?.showModal();
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
        <v-col :cols="getGridCols" class="d-flex">
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
        <v-col v-if="showRecommendations" :cols="getGridCols" class="d-flex">
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
        <v-col
            v-if="showFederalProofOfVaccination"
            :cols="getGridCols"
            class="d-flex"
        >
            <HgCardComponent
                title="Proof of Vaccination"
                density="compact"
                class="flex-grow-1 ma-1"
                :data-testid="`proof-vaccination-card-btn-${dependent.ownerId}`"
                @click="showSensitiveDocumentDownloadModal()"
            >
                <template #icon>
                    <v-icon icon="check-circle" color="success" size="small" />
                </template>
                <template #action-icon>
                    <v-icon icon="download" color="primary" size="small" />
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
        ref="sensitiveDocumentDownloadModal"
        title="Sensitive Document"
        message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
        @submit="handleFederalProofOfVaccinationDownload"
    />
    <MessageModalComponent
        ref="vaccineRecordResultModal"
        ok-only
        title="Alert"
        :message="vaccineRecordResultMessage"
    />
</template>
