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
import { PatientDataFile } from "@/models/patientDataResponse";
import DiagnosticImagingTimelineEntry from "@/models/timeline/diagnosticImagingTimelineEntry";
import { ILogger } from "@/services/interfaces";
import { usePatientDataStore } from "@/stores/patientData";
import { useTimelineStore } from "@/stores/timeline";
import SnowPlow from "@/utility/snowPlow";

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
const patientDataStore = usePatientDataStore();
const timelineStore = useTimelineStore();

const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const cols = computed(() => timelineStore.columnCount);
const entryIcon = computed(
    () => entryTypeMap.get(EntryType.DiagnosticImaging)?.icon
);
const isLoadingFile = computed(
    () =>
        props.entry.fileId !== undefined &&
        patientDataStore.isPatientDataFileLoading(props.entry.fileId)
);
const hasFile = computed(() => Boolean(props.entry.fileId));

function showConfirmationModal(): void {
    sensitiveDocumentModal.value?.showModal();
}

function downloadFile(): void {
    if (props.entry.fileId) {
        SnowPlow.trackEvent({
            action: "download_report",
            text: "Diagnostic Imaging PDF",
        });
        const dateString = props.entry.date.format("yyyy_MM_dd-HH_mm");
        patientDataStore
            .retrievePatientDataFile(props.hdid, props.entry.fileId)
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
    <TimelineEntryComponent
        v-bind="$attrs"
        :card-id="`${index}-${datekey}`"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.title"
        :subtitle="`Status: ${entry.examStatus}`"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="hasFile"
        @click-attachment-button="showConfirmationModal"
    >
        <v-row class="mb-3">
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="diagnostic-imaging-procedure-description"
                    name="Description"
                    name-class="font-weight-bold"
                    :value="entry.procedureDescription"
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="diagnostic-imaging-health-authority"
                    name="Health Authority"
                    name-class="font-weight-bold"
                    :value="entry.healthAuthority"
                />
            </v-col>
        </v-row>
        <HgButtonComponent
            v-if="hasFile"
            data-testid="diagnostic-imaging-download-button"
            class="mb-6"
            variant="secondary"
            text="Download Full Report"
            prepend-icon="download"
            :loading="isLoadingFile"
            @click="showConfirmationModal"
        />
        <p class="text-body-1">
            If you have questions about your imaging report: Contact your
            primary care provider or call HealthlinkBC at 811.
        </p>
        <p class="text-body-1">
            You can learn more about medical imaging tests on the following
            websites:
        </p>
        <ul>
            <li>
                <a
                    href="https://www.healthlinkbc.ca/tests-treatments-medications/medical-tests"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                >
                    HealthLink BC
                </a>
            </li>
            <li>
                <a
                    href="https://bcradiology.ca/patient-resources/"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                >
                    BC Radiological Society – Patient Resources
                </a>
            </li>
        </ul>
    </TimelineEntryComponent>
    <MessageModalComponent
        ref="sensitiveDocumentModal"
        title="Sensitive Document"
        message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
        @submit="downloadFile"
    />
</template>
