<script lang="ts">
import saveAs from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import DiagnosticImagingTimelineEntry from "@/models/diagnosticImagingTimelineEntry";
import { PatientDataFile } from "@/models/patientDataResponse";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

const options = {
    components: {
        EntryCard: EntryCardTimelineComponent,
        MessageModalComponent,
    },
};
@Component(options)
export default class DiagnosticImagingTimelineComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop()
    entry!: DiagnosticImagingTimelineEntry;

    @Prop()
    index!: number;

    @Prop()
    datekey!: string;

    @Prop()
    isMobileDetails!: boolean;

    @Prop({ default: false })
    commentsAreEnabled!: boolean;

    @Action("retrievePatientDataFile", { namespace: "patientData" })
    retrievePatientDataFile!: (params: {
        fileId: string;
        hdid: string;
    }) => Promise<PatientDataFile>;

    @Getter("isPatientDataFileLoading", { namespace: "patientData" })
    isPatientDataFileLoading!: (fileId: string) => boolean;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private logger!: ILogger;

    get isLoadingFile(): boolean {
        return (
            this.entry.fileId !== undefined &&
            this.isPatientDataFileLoading(this.entry.fileId)
        );
    }

    get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.DiagnosticImaging)?.icon;
    }

    created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    downloadFile(): void {
        if (this.entry.fileId) {
            SnowPlow.trackEvent({
                action: "download_report",
                text: "Diagnostic Imaging PDF",
            });
            const dateString = this.entry.date.format("yyyy_MM_dd-HH_mm");
            this.retrievePatientDataFile({
                fileId: this.entry.fileId,
                hdid: this.hdid,
            })
                .then(
                    (patientFile: PatientDataFile) =>
                        new Blob([new Uint8Array(patientFile.content)], {
                            type: patientFile.contentType,
                        })
                )
                .then((blob) =>
                    saveAs(blob, `diagnostic_image_${dateString}.pdf`)
                )
                .catch((err) => this.logger.error(err));
        }
    }
}
</script>

<template>
    <EntryCard
        :card-id="`${index}-${entry.id}`"
        :entry-icon="entryIcon"
        :title="entry.modality"
        :subtitle="`status: ${entry.examStatus}`"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="entry.fileId !== undefined"
    >
        <div slot="details-body">
            <div class="my-2">
                <div data-testid="diagnostic-imaging-body-part">
                    <strong>Body Part: </strong>
                    <span>{{ entry.bodyPart }}</span>
                </div>
                <div data-testid="diagnostic-imaging-procedure-description">
                    <strong>Description: </strong>
                    <span>{{ entry.procedureDescription }}</span>
                </div>
                <div data-testid="diagnostic-imaging-health-authority">
                    <strong>Health Authority: </strong>
                    <span>{{ entry.healthAuthority }}</span>
                </div>
                <div data-testid="diagnostic-imaging-facility">
                    <strong>Facility: </strong>
                    <span>{{ entry.facility }}</span>
                </div>
            </div>
            <div class="mt-3">
                <hg-button
                    data-testid="diagnostic-imaging-download-button"
                    variant="secondary"
                    :if="entry.fileId !== undefined"
                    :disabled="isLoadingFile"
                    @click="showConfirmationModal()"
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
                    <span>Download Full Report</span>
                </hg-button>
                <p class="mt-2 mb-2">
                    If you have questions, contact the doctor or care provider
                    who ordered your imaging.
                </p>
            </div>
            <MessageModalComponent
                ref="messageModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="downloadFile"
            />
        </div>
    </EntryCard>
</template>
