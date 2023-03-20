<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCircleInfo, faDownload } from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import PatientData, {
    HealthOptionType,
    OrganDonorRegistrationData,
    PatientDataFile,
    PatientHealthOption,
} from "@/models/patientData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faCircleInfo, faDownload);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: { MessageModalComponent },
};

@Component(options)
export default class OrganDonorDetailsCard extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Getter("isPatientDataFileLoading", { namespace: "patientData" })
    isPatientDataFileLoading!: (fileId: string) => boolean;

    @Getter("patientData", { namespace: "patientData" })
    patientData!: (hdid: string) => PatientData;

    @Action("retrievePatientDataFile", { namespace: "patientData" })
    retrievePatientDataFile!: (params: {
        fileId: string;
        hdid: string;
    }) => Promise<PatientDataFile>;

    @Ref("sensitiveDocumentModal")
    readonly sensitiveDocumentModal!: MessageModalComponent;

    logger!: ILogger;

    get isLoadingFile(): boolean {
        return (
            this.registrationData?.registrationFileId !== undefined &&
            this.isPatientDataFileLoading(
                this.registrationData.registrationFileId
            )
        );
    }

    get registrationData(): OrganDonorRegistrationData | undefined {
        return this.patientData(this.hdid).items.find(
            (ho: PatientHealthOption) =>
                ho.type === HealthOptionType.OrganDonorRegistrationData
        ) as OrganDonorRegistrationData;
    }

    async created(): Promise<void> {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private showConfirmationModal(): void {
        this.sensitiveDocumentModal.showModal();
    }

    private getDecisionFile(): void {
        if (this.registrationData && this.registrationData.registrationFileId) {
            SnowPlow.trackEvent({
                action: "download_report",
                text: "Organ Donor",
            });
            this.retrievePatientDataFile({
                fileId: this.registrationData.registrationFileId,
                hdid: this.hdid,
            })
                .then(
                    (patientFile: PatientDataFile) =>
                        new Blob(patientFile.content, {
                            type: patientFile.contentType,
                        })
                )
                .then((blob) => saveAs(blob, `Organ_Donor_Registration.pdf`))
                .catch((err) => this.logger.error(err));
        }
    }
}
</script>

<template>
    <hg-card
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
            <div data-testid="organ-donor-registration-status">
                <span class="text-muted">Status: </span>
                <strong>{{ registrationData?.status }}</strong>
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
            <div data-testid="organ-donor-registration-decision">
                <span class="text-muted">Decision: </span>
                <hg-button
                    v-if="registrationData?.registrationFileId"
                    data-testid="clinical-document-download-button"
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
                <strong v-else>Not Available</strong>
            </div>
            <div>
                <a
                    href="http://www.transplant.bc.ca/Pages/Register-your-Decision.aspx"
                    target="_blank"
                    data-testid="organ-donor-registration-link"
                >
                    Register or update your decision
                </a>
            </div>
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
