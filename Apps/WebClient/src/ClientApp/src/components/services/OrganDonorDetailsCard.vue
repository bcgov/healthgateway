<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload, faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { DataSource } from "@/constants/dataSource";
import { ErrorSourceType } from "@/constants/errorType";
import {
    OrganDonorRegistration,
    PatientData,
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faDownload, faInfoCircle);

interface Props {
    hdid: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const patientData = computed<PatientData[]>(() => {
    return store.getters["patientData/patientData"](props.hdid, [
        PatientDataType.OrganDonorRegistrationStatus,
    ]);
});

const isLoadingFile = computed<boolean>(() => {
    return (
        registrationData.value?.registrationFileId !== undefined &&
        isPatientDataFileLoading(registrationData.value.registrationFileId)
    );
});

const registrationData = computed<OrganDonorRegistration | undefined>(() => {
    return patientData.value
        ? (patientData.value[0] as OrganDonorRegistration)
        : undefined;
});

const blockedDataSources = computed<DataSource[]>(
    () => store.getters["user/blockedDataSources"]
);

const showOrganDonorRegistration = computed<boolean>(() => {
    return canAccessOrganDonorRegistration();
});

function addCustomError(
    title: string,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addCustomError", { title, source, traceId });
}

function canAccessOrganDonorRegistration(showDatasetWarning = false): boolean {
    const canAccess = !blockedDataSources.value.includes(
        DataSource.OrganDonorRegistration
    );
    logger.debug(
        `Can access data source for ${DataSource.OrganDonorRegistration}: ${canAccess}`
    );

    if (showDatasetWarning && !canAccess) {
        addCustomError(
            "Organ Donor Registration is not available at this time. Please try again later.",
            ErrorSourceType.User,
            undefined
        );
    }
    return canAccess;
}

function isPatientDataFileLoading(fileId: string): boolean {
    return store.getters["patientData/isPatientDataFileLoading"](fileId);
}

function getDecisionFile(): void {
    const registrationDataValue = registrationData.value;
    if (registrationDataValue && registrationDataValue.registrationFileId) {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Organ Donor",
        });
        store
            .dispatch("patientData/retrievePatientDataFile", {
                fileId: registrationDataValue.registrationFileId,
                hdid: props.hdid,
            })
            .then(
                (patientFile: PatientDataFile) =>
                    new Blob([new Uint8Array(patientFile.content)], {
                        type: patientFile.contentType,
                    })
            )
            .then((blob) => saveAs(blob, `Organ_Donor_Registration.pdf`))
            .catch((err) => logger.error(err));
    }
}

function showConfirmationModal(): void {
    sensitiveDocumentModal.value?.showModal();
}

canAccessOrganDonorRegistration(true);
</script>

<template>
    <hg-card
        v-if="showOrganDonorRegistration"
        title="Organ Donor Registration"
        data-testid="organ-donor-registration-card"
    >
        <template #icon>
            <img
                class="organ-donor-registry-logo align-self-center"
                src="@/assets/images/services/odr-logo.svg"
                alt="Organ Donor Registry Logo"
            />
        </template>
        <div class="flex-grow-1 d-flex flex-column card-content">
            <div>
                <span class="text-muted">Status: </span>
                <strong data-testid="organ-donor-registration-status">{{
                    registrationData?.status
                }}</strong>
                <hg-button
                    :id="`organ-donor-registration-status-info-${registrationData?.registrationFileId}`"
                    aria-label="Info"
                    href="#"
                    variant="link"
                    data-testid="organ-donor-registration-status-info-button"
                    class="shadow-none align-baseline p-0 ml-1"
                >
                    <hg-icon icon="info-circle" size="small" />
                </hg-button>
                <b-popover
                    :target="`organ-donor-registration-status-info-${registrationData?.registrationFileId}`"
                    triggers="hover focus"
                    placement="topright"
                    boundary="viewport"
                    data-testid="organ-donor-registration-status-info-popover"
                >
                    {{ registrationData?.statusMessage }}
                </b-popover>
            </div>
            <div>
                <span class="text-muted">Decision: </span>
                <hg-button
                    v-if="registrationData?.registrationFileId"
                    data-testid="organ-donor-registration-download-button"
                    variant="secondary"
                    :disabled="isLoadingFile"
                    class="ml-1"
                    @click="showConfirmationModal"
                >
                    <b-spinner v-if="isLoadingFile" class="mr-1" small />
                    <hg-icon
                        v-else
                        icon="download"
                        size="medium"
                        square
                        aria-hidden="true"
                        class="mr-1"
                    />
                    <span>Download</span>
                </hg-button>
                <strong
                    v-else
                    data-testid="organ-donor-registration-decision-no-file"
                    >Not Available</strong
                >
            </div>
            <div>
                <a
                    href="http://www.transplant.bc.ca/Pages/Register-your-Decision.aspx"
                    target="_blank"
                    data-testid="organ-donor-registration-link"
                >
                    {{ registrationData?.organDonorRegistrationLinkText }}
                </a>
            </div>
            <div>It may take 2 hours for updates to show here</div>
        </div>
        <MessageModalComponent
            ref="sensitiveDocumentModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getDecisionFile"
        />
    </hg-card>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.card-content {
    gap: 1rem;
}
</style>
