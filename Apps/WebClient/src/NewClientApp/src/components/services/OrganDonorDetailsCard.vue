<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import MessageModalComponent from "@/components/shared/MessageModalComponent.vue";
import {
    OrganDonorRegistration,
    PatientData,
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { usePatientDataStore } from "@/stores/patientData";
import HgCardComponent from "@/components/shared/HgCardComponent.vue";
import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";

interface Props {
    hdid: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const patientDataStore = usePatientDataStore();

const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const patientData = computed<PatientData[]>(() => {
    return patientDataStore.patientData(props.hdid, [
        PatientDataType.OrganDonorRegistrationStatus,
    ]);
});

const isLoadingFile = computed<boolean>(
    () =>
        registrationData.value?.registrationFileId !== undefined &&
        patientDataStore.isPatientDataFileLoading(
            registrationData.value?.registrationFileId
        )
);

const registrationData = computed<OrganDonorRegistration | undefined>(() => {
    return patientData.value
        ? (patientData.value[0] as OrganDonorRegistration)
        : undefined;
});

function getDecisionFile(): void {
    const registrationDataValue = registrationData.value;
    if (registrationDataValue && registrationDataValue.registrationFileId) {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Organ Donor",
        });
        patientDataStore
            .retrievePatientDataFile(
                props.hdid,
                registrationDataValue.registrationFileId
            )
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
</script>

<template>
    <HgCardComponent
        title="Organ Donor Registration"
        data-testid="organ-donor-registration-card"
    >
        <template #icon>
            <img
                src="@/assets/images/services/odr-logo.svg"
                alt="Organ Donor Registry Logo"
                :height="30"
            />
        </template>
        <div class="flex-grow-1 d-flex flex-column text-body-1 card-content">
            <div>
                <span class="text-grey-darken-2">Status: </span>
                <strong data-testid="organ-donor-registration-status">{{
                    registrationData?.status
                }}</strong>
                <v-tooltip
                    data-testid="organ-donor-registration-status-info-popover"
                    location="top start"
                    content-class="bg-grey-darken-3"
                    :text="registrationData?.statusMessage"
                >
                    <template v-slot:activator="{ props }">
                        <v-icon
                            ata-testid="organ-donor-registration-status-info-button"
                            color="primary"
                            icon="info-circle"
                            v-bind="props"
                            size="x-small"
                            class="ml-1"
                        />
                    </template>
                </v-tooltip>
            </div>
            <div>
                <span class="text-grey-darken-2">Decision: </span>
                <HgButtonComponent
                    v-if="registrationData?.registrationFileId"
                    data-testid="organ-donor-registration-download-button"
                    variant="secondary"
                    :disabled="isLoadingFile"
                    @click="showConfirmationModal"
                >
                    <v-progress-circular
                        indeterminate
                        v-if="isLoadingFile"
                        size="20"
                    />
                    <v-icon
                        v-else
                        icon="download"
                        size="medium"
                        square
                        aria-hidden="true"
                    />
                    <span class="text-body-1 ml-3">Download</span>
                </HgButtonComponent>
                <strong
                    v-else
                    data-testid="organ-donor-registration-decision-no-file"
                    >Not Available</strong
                >
            </div>
            <div class="text-body-1">
                <p>
                    <a
                        href="http://www.transplant.bc.ca/Pages/Register-your-Decision.aspx"
                        target="_blank"
                        data-testid="organ-donor-registration-link"
                    >
                        {{ registrationData?.organDonorRegistrationLinkText }}
                    </a>
                </p>
                <p>It may take 2 hours for updates to show here</p>
            </div>
        </div>
        <MessageModalComponent
            ref="sensitiveDocumentModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getDecisionFile"
        />
    </HgCardComponent>
</template>

<style lang="scss" scoped>
.card-content {
    gap: 1rem;
}
</style>
