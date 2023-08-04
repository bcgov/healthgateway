<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import saveAs from "file-saver";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import DiagnosticImagingTimelineEntry from "@/models/diagnosticImagingTimelineEntry";
import { PatientDataFile } from "@/models/patientDataResponse";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faDownload);

interface Props {
    hdid: string;
    entry: DiagnosticImagingTimelineEntry;
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
const store = useStore();

const messageModal = ref<InstanceType<typeof MessageModalComponent>>();

const isPatientDataFileLoading = computed<(fileId: string) => boolean>(
    () => store.getters["patientData/isPatientDataFileLoading"]
);
const isLoadingFile = computed(
    () =>
        props.entry.fileId !== undefined &&
        isPatientDataFileLoading.value(props.entry.fileId)
);
const entryIcon = computed(
    () => entryTypeMap.get(EntryType.DiagnosticImaging)?.icon
);

function retrievePatientDataFile(
    fileId: string,
    hdid: string
): Promise<PatientDataFile> {
    return store.dispatch("patientData/retrievePatientDataFile", {
        fileId,
        hdid,
    });
}

function showConfirmationModal(): void {
    messageModal.value?.showModal();
}

function downloadFile(): void {
    if (props.entry.fileId) {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Diagnostic Imaging PDF",
        });
        const dateString = props.entry.date.format("yyyy_MM_dd-HH_mm");
        retrievePatientDataFile(props.entry.fileId, props.hdid)
            .then(
                (patientFile: PatientDataFile) =>
                    new Blob([new Uint8Array(patientFile.content)], {
                        type: patientFile.contentType,
                    })
            )
            .then((blob) => saveAs(blob, `diagnostic_image_${dateString}.pdf`))
            .catch((err) => logger.error(err));
    }
}
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="`${index}-${datekey}`"
        :entry-icon="entryIcon"
        :title="entry.title"
        :subtitle="`Status: ${entry.examStatus}`"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="entry.fileId !== undefined"
    >
        <div slot="details-body">
            <div class="my-2">
                <div data-testid="diagnostic-imaging-procedure-description">
                    <strong>Description: </strong>
                    <span>{{ entry.procedureDescription }}</span>
                </div>
                <div data-testid="diagnostic-imaging-health-authority">
                    <strong>Health Authority: </strong>
                    <span>{{ entry.healthAuthority }}</span>
                </div>
            </div>
            <div class="mt-3">
                <hg-button
                    class="mb-3"
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
                <p>
                    If you have questions about your imaging report: Contact
                    your primary care provider or call HealthlinkBC at 811.
                </p>
                <p>
                    You can learn more about medical imaging tests on the
                    following websites:
                </p>
                <ul class="mb-0">
                    <li>
                        <a
                            href="https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests"
                            target="_blank"
                            rel="noopener"
                        >
                            HealthLink BC
                        </a>
                    </li>
                    <li>
                        <a
                            href="https://bcradiology.ca/patient-resources/"
                            target="_blank"
                            rel="noopener"
                        >
                            BC Radiological Society – Patient Resources
                        </a>
                    </li>
                </ul>
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
