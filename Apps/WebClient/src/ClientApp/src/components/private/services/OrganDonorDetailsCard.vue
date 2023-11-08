<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgCardComponent from "@/components/common/HgCardComponent.vue";
import InfoTooltipComponent from "@/components/common/InfoTooltipComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import { DataSource } from "@/constants/dataSource";
import { ErrorSourceType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    OrganDonorRegistration,
    PatientDataFile,
    PatientDataType,
} from "@/models/patientDataResponse";
import { Action, Actor, Format, Text, Type } from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { usePatientDataStore } from "@/stores/patientData";
import { useUserStore } from "@/stores/user";

interface Props {
    hdid: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const errorStore = useErrorStore();
const patientDataStore = usePatientDataStore();
const userStore = useUserStore();

const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const patientData = computed(() =>
    patientDataStore.patientData(props.hdid, [
        PatientDataType.OrganDonorRegistrationStatus,
    ])
);
const isLoadingFile = computed(
    () =>
        registrationData.value?.registrationFileId !== undefined &&
        patientDataStore.isPatientDataFileLoading(
            registrationData.value?.registrationFileId
        )
);
const registrationData = computed(() => {
    return patientData.value
        ? (patientData.value[0] as OrganDonorRegistration)
        : undefined;
});
const showOrganDonorRegistration = computed(
    () =>
        !userStore.blockedDataSources.includes(
            DataSource.OrganDonorRegistration
        )
);

function getDecisionFile(): void {
    const registrationDataValue = registrationData.value;
    if (registrationDataValue?.registrationFileId) {
        trackingService.track({
            action: Action.Download,
            text: Text.Document,
            type: Type.OrganDonorRegistration,
            format: Format.Pdf,
            actor: Actor.User,
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

if (!showOrganDonorRegistration.value) {
    errorStore.addCustomError(
        "Organ Donor Registration is not available at this time. Please try again later.",
        ErrorSourceType.User,
        undefined
    );
}
</script>

<template>
    <HgCardComponent
        v-if="showOrganDonorRegistration"
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
                <span class="text-medium-emphasis">Status: </span>
                <strong data-testid="organ-donor-registration-status">
                    {{ registrationData?.status }}
                </strong>
                <InfoTooltipComponent
                    data-testid="organ-donor-registration-status-info-button"
                    tooltip-testid="organ-donor-registration-status-info-popover"
                    :text="registrationData?.statusMessage"
                    class="ml-2"
                />
            </div>
            <div>
                <span class="text-medium-emphasis">Decision: </span>
                <HgButtonComponent
                    v-if="registrationData?.registrationFileId"
                    data-testid="organ-donor-registration-download-button"
                    variant="secondary"
                    prepend-icon="download"
                    text="Download"
                    :loading="isLoadingFile"
                    @click="showConfirmationModal"
                />
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
                        rel="noopener"
                        class="text-link"
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
            title="Sensitive Document"
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
