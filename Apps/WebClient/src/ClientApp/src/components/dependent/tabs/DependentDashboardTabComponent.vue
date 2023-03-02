<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faChevronRight,
    faDownload,
    faFileWaveform,
    faMicroscope,
    faSyringe,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import {
    EntryType,
    EntryTypeDetails,
    entryTypeMap,
} from "@/constants/entryType";
import type { WebClientConfiguration } from "@/models/configData";
import CovidVaccineRecord from "@/models/covidVaccineRecord";
import type { Dependent } from "@/models/dependent";
import { LoadStatus } from "@/models/storeOperations";
import { TimelineFilterBuilder } from "@/models/timelineFilter";
import VaccinationRecord from "@/models/vaccinationRecord";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import SnowPlow from "@/utility/snowPlow";

library.add(
    faChevronRight,
    faDownload,
    faFileWaveform,
    faMicroscope,
    faSyringe,
    faVial
);

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

    @Getter("authenticatedVaccineRecords", { namespace: "vaccinationStatus" })
    vaccineRecords!: Map<string, VaccinationRecord>;

    @Getter("authenticatedVaccineRecordStatusChanges", {
        namespace: "vaccinationStatus",
    })
    vaccineRecordStatusChanges!: number;

    @Action("retrieveAuthenticatedVaccineRecord", {
        namespace: "vaccinationStatus",
    })
    retrieveAuthenticatedVaccineRecord!: (params: {
        hdid: string;
    }) => Promise<CovidVaccineRecord>;

    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Action("stopAuthenticatedVaccineRecordDownload", {
        namespace: "vaccinationStatus",
    })
    stopAuthenticatedVaccineRecordDownload!: (params: { hdid: string }) => void;

    @Ref("sensitivedocumentDownloadModal")
    readonly sensitiveDocumentDownloadModal!: MessageModalComponent;

    @Ref("vaccineRecordResultModal")
    readonly vaccineRecordResultModal!: MessageModalComponent;

    @Watch("vaccineRecordStatusChanges")
    private showVaccineRecordResultModal(): void {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            vaccinationRecord !== undefined &&
            vaccinationRecord.resultMessage.length > 0
        ) {
            this.vaccineRecordResultModal.showModal();
        }
    }

    @Watch("vaccineRecordStatusChanges")
    private saveVaccinePdf(): void {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();

        if (
            vaccinationRecord?.record !== undefined &&
            vaccinationRecord.hdid === this.dependent.ownerId &&
            vaccinationRecord.status === LoadStatus.LOADED &&
            vaccinationRecord.download
        ) {
            const mimeType = vaccinationRecord.record.document.mediaType;
            const downloadLink = `data:${mimeType};base64,${vaccinationRecord.record.document.data}`;
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
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            this.vaccineRecordStatusChanges > 0 &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.status === LoadStatus.REQUESTED;
        }
        return false;
    }

    get showFederalProofOfVaccination(): boolean {
        return this.config.featureToggleConfiguration.homepage
            .showFederalProofOfVaccination;
    }

    get vaccineRecordStatusMessage(): string {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            this.vaccineRecordStatusChanges > 0 &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.statusMessage;
        }
        return "";
    }

    get vaccineRecordResultMessage(): string {
        const vaccinationRecord: VaccinationRecord | undefined =
            this.getVaccinationRecord();
        if (
            this.vaccineRecordStatusChanges > 0 &&
            vaccinationRecord !== undefined
        ) {
            return vaccinationRecord.resultMessage;
        }
        return "";
    }

    get entryTypes(): EntryTypeDetails[] {
        return [...entryTypeMap.values()].filter((d) =>
            ConfigUtil.isDependentDatasetEnabled(d.type)
        );
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private getVaccinationRecord(): VaccinationRecord | undefined {
        return this.vaccineRecords.get(this.dependent.ownerId);
    }

    private handleClickEntryType(type: EntryType): void {
        this.logger.debug(`Handle entry type clicked: ${type}`);
        SnowPlow.trackEvent({
            action: "click",
            text: `Dependent_${type.toLocaleLowerCase}`,
        });
        const entryTypes: EntryType[] = [type];
        const builder =
            TimelineFilterBuilder.create().withEntryTypes(entryTypes);
        this.setFilter(builder);
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
            <b-col v-for="entry in entryTypes" :key="entry.type" class="p-3">
                <hg-card-button
                    :title="entry.name"
                    has-chevron
                    :data-testid="`dependent-entry-type-${dependent.ownerId}`"
                    @click="handleClickEntryType(entry.type)"
                >
                    <template #icon>
                        <hg-icon
                            :icon="entry.icon"
                            class="entry-link-card-icon align-self-center"
                            size="large"
                            square
                        />
                    </template>
                </hg-card-button>
            </b-col>
            <b-col v-if="showFederalProofOfVaccination" class="p-3">
                <hg-card-button
                    title="Proof of Vaccination"
                    :data-testid="`proof-vaccination-card-btn-${dependent.ownerId}`"
                    @click="showSensitiveDocumentDownloadModal()"
                >
                    <template #icon>
                        <hg-icon
                            icon="download"
                            class="entry-link-card-icon align-self-center"
                            size="large"
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

.canada-government-logo {
    height: 1.5em;
}

.entry-link-card-icon {
    color: $primary;
}
</style>
