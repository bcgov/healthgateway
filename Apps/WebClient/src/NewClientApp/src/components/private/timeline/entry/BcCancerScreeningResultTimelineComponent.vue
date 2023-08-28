<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { PatientDataFile } from "@/models/patientDataResponse";
import BcCancerScreeningResultTimelineEntry from "@/models/timeline/BcCancerScreeningResultTimelineEntry";
import { ILogger } from "@/services/interfaces";
import { usePatientDataStore } from "@/stores/patientData";
import SnowPlow from "@/utility/snowPlow";

interface Props {
    hdid: string;
    entry: BcCancerScreeningResultTimelineEntry;
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
const patientDataStore = usePatientDataStore();
const messageModal = ref<InstanceType<typeof MessageModalComponent>>();

const entryIcon = computed(
    () => entryTypeMap.get(EntryType.CancerScreening)?.icon
);
const isLoadingFile = computed(
    () =>
        props.entry.fileId !== undefined &&
        patientDataStore.isPatientDataFileLoading(props.entry.fileId)
);
const hasFile = computed(
    () => props.entry.fileId !== undefined && props.entry.fileId !== ""
);

function showConfirmationModal(): void {
    messageModal.value?.showModal();
}

function downloadFile(): void {
    if (props.entry.fileId) {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Cancer Screening PDF",
        });
        const dateString = props.entry.date.format("yyyy_MM_dd-HH_mm");
        patientDataStore
            .retrievePatientDataFile(props.entry.fileId, props.hdid)
            .then(
                (patientFile: PatientDataFile) =>
                    new Blob([new Uint8Array(patientFile.content)], {
                        type: patientFile.contentType,
                    })
            )
            .then((blob) => saveAs(blob, `cancer_screening_${dateString}.pdf`))
            .catch((err) => logger.error(err));
    }
}
</script>

<template>
    <TimelineEntryComponent
        :card-id="`${index}-${datekey}`"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.title"
        :subtitle="`Program: ${entry.programName}`"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="Boolean(entry.fileId)"
    >
        <p class="text-body-1 mb-3">
            For information about your results, you can contact
            <a
                href="http://www.bccancer.bc.ca/"
                target="_blank"
                rel="noopener"
                class="text-link"
                >BC Cancer</a
            >
        </p>
        <HgButtonComponent
            v-if="hasFile"
            data-testid="bc-cancer-screening-download-button"
            class="mb-6"
            variant="secondary"
            text="View PDF"
            prepend-icon="eye"
            :loading="isLoadingFile"
            @click="showConfirmationModal()"
        />
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadFile"
        />
    </TimelineEntryComponent>
</template>
