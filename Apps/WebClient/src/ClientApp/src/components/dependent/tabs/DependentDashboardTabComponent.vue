<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faChevronRight,
    faDownload,
} from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import { computed, ref, watch } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import type { Dependent } from "@/models/dependent";
import { LoadStatus } from "@/models/storeOperations";
import VaccineRecordState from "@/models/vaccineRecordState";
import SnowPlow from "@/utility/snowPlow";

library.add(faCheckCircle, faChevronRight, faDownload);

interface Props {
    dependent: Dependent;
}
const props = defineProps<Props>();

const router = useRouter();
const store = useStore();

const sensitiveDocumentDownloadModal =
    ref<InstanceType<typeof MessageModalComponent>>();
const vaccineRecordResultModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const vaccineRecordState = computed<VaccineRecordState>(() =>
    store.getters["vaccinationStatus/authenticatedVaccineRecordState"](
        props.dependent.ownerId
    )
);
const isVaccineRecordDownloading = computed(
    () => vaccineRecordState.value.status === LoadStatus.REQUESTED
);
const showFederalProofOfVaccination = computed(
    () =>
        config.value.featureToggleConfiguration.homepage
            .showFederalProofOfVaccination
);
const vaccineRecordStatusMessage = computed(
    () => vaccineRecordState.value.statusMessage
);
const vaccineRecordResultMessage = computed(
    () => vaccineRecordState.value.resultMessage
);

function retrieveAuthenticatedVaccineRecord(hdid: string): void {
    store.dispatch("vaccinationStatus/retrieveAuthenticatedVaccineRecord", {
        hdid,
    });
}

function stopAuthenticatedVaccineRecordDownload(hdid: string): void {
    store.dispatch("vaccinationStatus/stopAuthenticatedVaccineRecordDownload", {
        hdid,
    });
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
    <div>
        <LoadingComponent
            :is-loading="isVaccineRecordDownloading"
            :text="vaccineRecordStatusMessage"
        />
        <b-row cols="1" cols-lg="2" cols-xl="3">
            <b-col class="p-3">
                <hg-card-button
                    title="Health Records"
                    dense
                    :data-testid="`dependent-health-records-button-${dependent.ownerId}`"
                    @click="handleClickHealthRecordsButton"
                >
                    <template #icon>
                        <img
                            class="health-gateway-logo align-self-center"
                            src="@/assets/images/gov/health-gateway-logo.svg"
                            alt="Health Gateway Logo"
                        />
                    </template>
                    <template #action-icon>
                        <hg-icon
                            icon="chevron-right"
                            class="chevron-icon align-self-center"
                            size="medium"
                            square
                        />
                    </template>
                </hg-card-button>
            </b-col>
            <b-col v-if="showFederalProofOfVaccination" class="p-3">
                <hg-card-button
                    title="Proof of Vaccination"
                    dense
                    :data-testid="`proof-vaccination-card-btn-${dependent.ownerId}`"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    <template #icon>
                        <hg-icon
                            class="checkmark align-self-center"
                            icon="check-circle"
                            size="large"
                            square
                        />
                    </template>
                    <template #action-icon>
                        <hg-icon
                            icon="download"
                            class="entry-link-card-icon align-self-center"
                            size="medium"
                            square
                        />
                    </template>
                </hg-card-button>
            </b-col>
        </b-row>
        <MessageModalComponent
            ref="sensitiveDocumentDownloadModal"
            title="Sensitive Document Download"
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

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.entry-link-card-icon {
    color: $primary;
}

.health-gateway-logo {
    height: 1.5em;
    width: 1.5em;
}

.checkmark {
    color: $hg-state-success;
}
</style>
