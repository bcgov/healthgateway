<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import EncodedMedia from "@/models/encodedMedia";
import { LoadStatus } from "@/models/storeOperations";
import ClinicalDocumentTimelineEntry from "@/models/timeline/clinicalDocumentTimelineEntry";
import { ILogger } from "@/services/interfaces";
import { useClinicalDocumentStore } from "@/stores/clinicalDocument";
import { useTimelineStore } from "@/stores/timeline";
import SnowPlow from "@/utility/snowPlow";

interface Props {
    hdid: string;
    entry: ClinicalDocumentTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const clinicalDocumentStore = useClinicalDocumentStore();
const timelineStore = useTimelineStore();

const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const cols = computed(() => timelineStore.columnCount);
const isLoadingFile = computed(() => {
    if (props.entry.fileId in clinicalDocumentStore.files) {
        return (
            clinicalDocumentStore.files.get(props.entry.fileId)?.status ===
            LoadStatus.REQUESTED
        );
    }

    return false;
});
const entryIcon = computed(
    () => entryTypeMap.get(EntryType.ClinicalDocument)?.icon
);

function showConfirmationModal(): void {
    sensitiveDocumentModal.value?.showModal();
}

function downloadFile(): void {
    SnowPlow.trackEvent({
        action: "download_report",
        text: "Clinical Document PDF",
    });

    clinicalDocumentStore
        .getFile(props.entry.fileId, props.hdid)
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
    <TimelineEntryComponent
        v-bind="$attrs"
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.name"
        :subtitle="entry.documentType"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        has-attachment
        @click-attachment-button="showConfirmationModal"
    >
        <v-row>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="clinical-document-discipline"
                    name="Type"
                    name-class="font-weight-bold"
                    :value="entry.discipline"
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="clinical-document-facility"
                    name="Facility"
                    name-class="font-weight-bold"
                    :value="entry.facilityName"
                />
            </v-col>
        </v-row>
        <v-row>
            <v-col>
                <HgButtonComponent
                    data-testid="clinical-document-download-button"
                    variant="secondary"
                    prepend-icon="download"
                    text="Download Full Report"
                    :loading="isLoadingFile"
                    @click="showConfirmationModal"
                />
            </v-col>
        </v-row>
    </TimelineEntryComponent>
    <MessageModalComponent
        ref="sensitiveDocumentModal"
        title="Sensitive Document"
        message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
        @submit="downloadFile"
    />
</template>
