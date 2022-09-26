<script lang="ts">
import saveAs from "file-saver";
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import type { Dictionary } from "@/models/baseTypes";
import ClinicalDocumentTimelineEntry from "@/models/clinicalDocumentTimelineEntry";
import EncodedMedia from "@/models/encodedMedia";
import { LoadStatus } from "@/models/storeOperations";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { ClinicalDocumentFileState } from "@/store/modules/clinicalDocument/types";
import SnowPlow from "@/utility/snowPlow";

import EntrycardTimelineComponent from "./EntrycardTimelineComponent.vue";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        EntryCard: EntrycardTimelineComponent,
        MessageModalComponent,
    },
};

@Component(options)
export default class ClinicalDocumentTimelineComponent extends Vue {
    @Prop()
    entry!: ClinicalDocumentTimelineEntry;

    @Prop()
    index!: number;

    @Prop()
    datekey!: string;

    @Prop()
    isMobileDetails!: boolean;

    @Action("getFile", { namespace: "clinicalDocument" })
    getFile!: (params: {
        fileId: string;
        hdid: string;
    }) => Promise<EncodedMedia>;

    @Getter("files", { namespace: "clinicalDocument" })
    files!: Dictionary<ClinicalDocumentFileState>;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;

    private logger!: ILogger;

    private get isLoadingFile(): boolean {
        if (this.entry.fileId in this.files) {
            return (
                this.files[this.entry.fileId]?.status === LoadStatus.REQUESTED
            );
        }

        return false;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private get entryIcon(): string | undefined {
        return entryTypeMap.get(EntryType.ClinicalDocument)?.icon;
    }

    private showConfirmationModal(): void {
        this.messageModal.showModal();
    }

    private downloadFile(): void {
        SnowPlow.trackEvent({
            action: "download",
            text: "document",
        });

        this.getFile({ fileId: this.entry.fileId, hdid: this.user.hdid })
            .then((result: EncodedMedia) => {
                const dateString = this.entry.date.format("yyyy_MM_dd-HH_mm");
                fetch(
                    `data:${result.mediaType};${result.encoding},${result.data}`
                )
                    .then((response) => response.blob())
                    .then((blob) =>
                        saveAs(blob, `Clinical_Document_${dateString}.pdf`)
                    );
            })
            .catch((err) => this.logger.error(err));
    }
}
</script>

<template>
    <EntryCard
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.name"
        :subtitle="entry.documentType"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        has-attachment
    >
        <div slot="details-body">
            <div class="my-2">
                <div data-testid="clinical-document-discipline">
                    <strong>Discipline: </strong>
                    <span>{{ entry.discipline }}</span>
                </div>
                <div data-testid="clinical-document-facility">
                    <strong>Facility: </strong>
                    <span>{{ entry.facilityName }}</span>
                </div>
            </div>
            <div class="mt-3">
                <hg-button
                    data-testid="clinical-document-download-button"
                    variant="secondary"
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
