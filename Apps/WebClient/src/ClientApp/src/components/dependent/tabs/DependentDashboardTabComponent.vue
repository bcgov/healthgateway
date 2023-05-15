<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faChevronRight,
    faDownload,
} from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import type { Dependent } from "@/models/dependent";
import { LoadStatus } from "@/models/storeOperations";
import VaccineRecordState from "@/models/vaccineRecordState";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faCheckCircle, faChevronRight, faDownload);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        LoadingComponent,
        MessageModalComponent,
    },
};

@Component(options)
export default class DependentDashboardTabComponent extends Vue {
    @Prop({ required: true })
    private dependent!: Dependent;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("authenticatedVaccineRecordState", {
        namespace: "vaccinationStatus",
    })
    getVaccineRecordState!: (hdid: string) => VaccineRecordState;

    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Action("stopAuthenticatedVaccineRecordDownload", {
        namespace: "vaccinationStatus",
    })
    stopAuthenticatedVaccineRecordDownload!: (params: { hdid: string }) => void;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitiveDocumentDownloadModal!: MessageModalComponent;

    @Ref("vaccineRecordResultModal")
    readonly vaccineRecordResultModal!: MessageModalComponent;

    @Watch("vaccineRecordState")
    private watchVaccineRecordState(): void {
        if (this.vaccineRecordState.resultMessage.length > 0) {
            this.vaccineRecordResultModal.showModal();
        }

        if (
            this.vaccineRecordState.record !== undefined &&
            this.vaccineRecordState.status === LoadStatus.LOADED &&
            this.vaccineRecordState.download
        ) {
            const mimeType = this.vaccineRecordState.record.document.mediaType;
            const downloadLink = `data:${mimeType};base64,${this.vaccineRecordState.record.document.data}`;
            fetch(downloadLink).then((res) => {
                res.blob().then((blob) => saveAs(blob, "VaccineProof.pdf"));
            });
            this.stopAuthenticatedVaccineRecordDownload({
                hdid: this.dependent.ownerId,
            });
        }
    }

    private logger!: ILogger;

    get isVaccineRecordDownloading(): boolean {
        return this.vaccineRecordState.status === LoadStatus.REQUESTED;
    }

    get showFederalProofOfVaccination(): boolean {
        return this.config.featureToggleConfiguration.homepage
            .showFederalProofOfVaccination;
    }

    get vaccineRecordStatusMessage(): string {
        return this.vaccineRecordState.statusMessage;
    }

    get vaccineRecordResultMessage(): string {
        return this.vaccineRecordState.resultMessage;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private get vaccineRecordState(): VaccineRecordState {
        return this.getVaccineRecordState(this.dependent.ownerId);
    }

    private handleClickHealthRecordsButton(): void {
        SnowPlow.trackEvent({
            action: "click",
            text: "dependent_all_records",
        });
        this.$router.push({
            path: `/dependents/${this.dependent.ownerId}/timeline`,
        });
    }

    private handleFederalProofOfVaccinationDownload(): void {
        this.logger.debug(`Handle federal proof of vaccination download`);
        SnowPlow.trackEvent({
            action: "click_button",
            text: "Dependent_Proof",
        });
        this.retrieveAuthenticatedVaccineRecord({
            hdid: this.dependent.ownerId,
        });
    }

    private showSensitiveDocumentDownloadModal(): void {
        this.sensitiveDocumentDownloadModal.showModal();
    }
}
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
            ref="sensitivedocumentDownloadModal"
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
