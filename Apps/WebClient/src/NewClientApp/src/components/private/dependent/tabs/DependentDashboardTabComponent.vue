<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref, watch } from "vue";
import { useRouter } from "vue-router";

import HgCardComponent from "@/components/common/HgCardComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import type { Dependent } from "@/models/dependent";
import { LoadStatus } from "@/models/storeOperations";
import VaccineRecordState from "@/models/vaccineRecordState";
import { useConfigStore } from "@/stores/config";
import { useVaccinationStatusAuthenticatedStore } from "@/stores/vaccinationStatusAuthenticated";
import { getGridCols } from "@/utility/gridUtilty";
import SnowPlow from "@/utility/snowPlow";

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const configStore = useConfigStore();
const vaccinationStatusStore = useVaccinationStatusAuthenticatedStore();

const router = useRouter();

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
    SnowPlow.trackEvent({
        action: "click",
        text: "dependent_all_records",
    });
    router.push({
        path: `/dependents/${props.dependent.ownerId}/timeline`,
    });
}

function handleFederalProofOfVaccinationDownload(): void {
    SnowPlow.trackEvent({
        action: "click_button",
        text: "Dependent_Proof",
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
    <div class="pa-2">
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordStatusMessage"
        />
        <v-row>
            <v-col :cols="getGridCols" class="d-flex">
                <HgCardComponent
                    title="Health Records"
                    class="flex-grow-1"
                    :data-testid="`dependent-health-records-button-${dependent.ownerId}`"
                    @click="handleClickHealthRecordsButton"
                >
                    <template #icon>
                        <img
                            src="@/assets/images/gov/health-gateway-logo.svg"
                            alt="Health Gateway Logo"
                            :height="30"
                        />
                    </template>
                    <template #action-icon>
                        <v-icon icon="chevron-right" />
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
                    class="flex-grow-1"
                    :data-testid="`proof-vaccination-card-btn-${dependent.ownerId}`"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    <template #icon>
                        <v-icon icon="check-circle" />
                    </template>
                    <template #action-icon>
                        <v-icon icon="download" />
                    </template>
                </HgCardComponent>
            </v-col>
        </v-row>
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
    </div>
</template>
