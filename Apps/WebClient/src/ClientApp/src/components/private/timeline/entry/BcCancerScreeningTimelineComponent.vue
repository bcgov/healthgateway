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
import BcCancerScreeningTimelineEntry from "@/models/timeline/bcCancerScreeningTimelineEntry";
import { Action, Actor, Dataset, Format, Text } from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { usePatientDataStore } from "@/stores/patientData";
import EventDataUtility from "@/utility/eventDataUtility";

interface Props {
    hdid: string;
    entry: BcCancerScreeningTimelineEntry;
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
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const patientDataStore = usePatientDataStore();
const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const entryIcon = computed(
    () => entryTypeMap.get(EntryType.BcCancerScreening)?.icon
);
const isLoadingFile = computed(
    () =>
        props.entry.fileId !== undefined &&
        patientDataStore.isPatientDataFileLoading(props.entry.fileId)
);
const hasFile = computed(() => Boolean(props.entry.fileId));

const programLink = computed(() => {
    const name = props.entry.programName?.toLowerCase().trim() ?? "unknown";
    if (name.includes("breast")) {
        return "http://www.bccancer.bc.ca/screening/breast";
    } else if (name.includes("colon")) {
        return "http://www.bccancer.bc.ca/screening/colon";
    } else if (name.includes("lung")) {
        return "http://www.bccancer.bc.ca/screening/lung";
    } else if (name.includes("cervical")) {
        return "http://www.bccancer.bc.ca/screening/cervix";
    } else {
        return "http://www.bccancer.bc.ca/screening";
    }
});

function showConfirmationModal(): void {
    sensitiveDocumentModal.value?.showModal();
}

function downloadFile(): void {
    if (props.entry.fileId) {
        trackingService.trackEvent({
            action: Action.Download,
            text: Text.Document,
            dataset: Dataset.BcCancer,
            type: EventDataUtility.getType(props.entry.screeningType),
            format: Format.Pdf,
            actor: Actor.User,
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
            .then((blob) =>
                saveAs(blob, `${props.entry.fileName}_${dateString}.pdf`)
            )
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
        :subtitle="entry.subtitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="hasFile"
        @click-attachment-button="showConfirmationModal"
    >
        <p class="text-body-1 mb-3">
            <span
                v-if="props.entry.isResult"
                data-testid="bc-cancer-result-body"
            >
                Download your
                {{
                    entry.programName?.toLowerCase() === "unknown"
                        ? "unknown program"
                        : entry.programName?.toLowerCase()
                }}
                screening result letter. It may include important information
                about next steps. If you have questions,
                <a
                    :href="programLink"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >check the BC Cancer website</a
                >
                or talk to your care provider.
            </span>
            <span v-else data-testid="bc-cancer-screening-body">
                Find out about your
                {{
                    entry.programName?.toLowerCase() === "unknown"
                        ? "unknown program"
                        : entry.programName?.toLowerCase()
                }}
                screening next steps. You will also get this letter in the mail.
                <a
                    :href="programLink"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >Learn more about
                    {{
                        entry.programName?.toLowerCase() === "unknown"
                            ? "unknown program"
                            : entry.programName?.toLowerCase()
                    }}
                    screening</a
                >.
            </span>
        </p>
        <HgButtonComponent
            v-if="hasFile"
            data-testid="bc-cancer-screening-download-button"
            class="mb-6"
            variant="secondary"
            :text="entry.callToActionText"
            prepend-icon="eye"
            :loading="isLoadingFile"
            @click="showConfirmationModal"
        />
    </TimelineEntryComponent>
    <MessageModalComponent
        ref="sensitiveDocumentModal"
        title="Sensitive Document"
        message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
        @submit="downloadFile"
    />
</template>
