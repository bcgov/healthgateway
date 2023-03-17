<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCircleInfo, faDownload } from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import PatientData, {
    HealthOptionTypes,
    OrganDonorRegistrationData,
    PatientDataFile,
    PatientHealthOptions,
} from "@/models/patientData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faCircleInfo, faDownload);

@Component({
    components: { MessageModalComponent },
})
export default class OrganDonorDetailsCard extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Getter("patientData", { namespace: "patientData" })
    getPatientData!: (hdid: string) => PatientData;

    @Action("retrievePatientDataFile", { namespace: "patientData" })
    getPatientDataFile!: (params: {
        fileId: string;
        hdid: string;
    }) => Promise<PatientDataFile>;

    @Ref("sensitiveDocumentModal")
    readonly sensitiveDocumentModal!: MessageModalComponent;

    logger!: ILogger;

    get registrationData(): OrganDonorRegistrationData | undefined {
        return this.patientData.items.find(
            (ho: PatientHealthOptions) =>
                ho.type === HealthOptionTypes.OrganDonorRegistrationData
        ) as OrganDonorRegistrationData;
    }

    get patientData(): PatientData {
        return this.getPatientData(this.hdid);
    }

    private created(): void {
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
            this.getPatientDataFile({
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
    <b-card
        container
        class="hg-card-button h-100 w-100 d-flex flex-column align-content-start text-left rounded shadow border-0"
        header-class="border-0 bg-transparent d-flex"
        v-bind="$attrs"
    >
        <template #header>
            <img
                class="organ-donor-registry-logo align-self-center float-left mr-2 valign-baseline"
                src="@/assets/images/services/odr-logo.svg"
                alt="Organ Donor Registry Logo"
            />
            <h5 class="align-self-end">Organ Donor Registration</h5>
        </template>
        <b-container>
            <b-row class="align-items-center mb-5">
                <span
                    class="label-color mr-2 font-weight-lighter mb-0 fs-donor-label"
                >
                    Status:
                </span>
                <span class="mr-2 font-weight-bolder mb-0 fs-donor-label">{{
                    registrationData.status
                }}</span>
                <hg-icon
                    :id="`organ-donor-registration-info-${registrationData.registrationFileId}`"
                    class="ml-2 info-color"
                    icon="circle-info"
                ></hg-icon>
                <b-popover
                    :target="`organ-donor-registration-info-${registrationData.registrationFileId}`"
                    triggers="hover"
                    placement="top"
                    boundary="viewport"
                    data-testid="organ-donor-registration-info-popover"
                >
                    {{ registrationData.statusMessage }}
                </b-popover>
            </b-row>
            <b-row class="align-items-center mb-5">
                <span
                    class="label-color mr-2 font-weight-lighter mb-0 fs-donor-label"
                >
                    Decision:
                </span>
                <hg-button
                    v-if="registrationData.registrationFileId"
                    variant="secondary"
                    @click="showConfirmationModal"
                    ><hg-icon
                        icon="download"
                        size="medium"
                        square
                        aria-hidden="true"
                    />
                    Download
                </hg-button>
                <span v-else class="mr-2 font-weight-bolder mb-0 fs-donor-label"
                    >Not Available</span
                >
            </b-row>
            <b-row>
                <a
                    href="http://www.transplant.bc.ca/Pages/Register-your-Decision.aspx"
                    target="_blank"
                >
                    Register or update your decision
                </a>
            </b-row>
        </b-container>
        <MessageModalComponent
            ref="sensitiveDocumentModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getDecisionFile"
        />
    </b-card>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.fs-donor-label {
    font-size: 0.9em;
}

.info-color {
    color: $primary !important;
}

.label-color {
    color: grey;
}
</style>
