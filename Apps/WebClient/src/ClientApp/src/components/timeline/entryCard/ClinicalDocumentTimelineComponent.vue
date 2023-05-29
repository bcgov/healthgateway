<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import type { Dictionary } from "@/models/baseTypes";
import { ClinicalDocumentFile } from "@/models/clinicalDocument";
import ClinicalDocumentTimelineEntry from "@/models/clinicalDocumentTimelineEntry";
import EncodedMedia from "@/models/encodedMedia";
import { LoadStatus } from "@/models/storeOperations";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

import EntryCardTimelineComponent from "./EntrycardTimelineComponent.vue";

interface Props {
    hdid: string;
    entry: ClinicalDocumentTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    commentsAreEnabled: false,
});

const store = useStore();
const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const messageModal = ref<MessageModalComponent>();

const files = computed<Dictionary<ClinicalDocumentFile>>(
    () => store.getters["clinicalDocument/files"]
);
function getFile(fileId: string, hdid: string): Promise<EncodedMedia> {
    return store.dispatch("clinicalDocument/getFile", { fileId, hdid });
}

const isLoadingFile = computed<boolean>(() => {
    if (props.entry.fileId in files.value) {
        return files.value[props.entry.fileId]?.status === LoadStatus.REQUESTED;
    }

    return false;
});

const entryIcon = computed<string | undefined>(
    () => entryTypeMap.get(EntryType.ClinicalDocument)?.icon
);

function showConfirmationModal(): void {
    messageModal.value?.showModal();
}

function downloadFile(): void {
    SnowPlow.trackEvent({
        action: "download_report",
        text: "Clinical Document PDF",
    });

    getFile(props.entry.fileId, props.hdid)
        .then((result: EncodedMedia) => {
            const dateString = props.entry.date.format("yyyy_MM_dd-HH_mm");
            fetch(`data:${result.mediaType};${result.encoding},${result.data}`)
                .then((response) => response.blob())
                .then((blob) =>
                    saveAs(blob, `Clinical_Document_${dateString}.pdf`)
                );
        })
        .catch((err) => logger.error(err));
}
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.name"
        :subtitle="entry.documentType"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
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
    </EntryCardTimelineComponent>
</template>
