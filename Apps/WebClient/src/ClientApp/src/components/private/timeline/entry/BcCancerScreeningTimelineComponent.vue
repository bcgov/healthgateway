<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    BcCancerScreeningType,
    PatientDataFile,
} from "@/models/patientDataResponse";
import BcCancerScreeningTimelineEntry from "@/models/timeline/bcCancerScreeningTimelineEntry";
import { ILogger } from "@/services/interfaces";
import { usePatientDataStore } from "@/stores/patientData";
import SnowPlow from "@/utility/snowPlow";

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
const patientDataStore = usePatientDataStore();
const messageModal = ref<InstanceType<typeof MessageModalComponent>>();

const entryIcon = computed(
    () => entryTypeMap.get(EntryType.BcCancerScreening)?.icon
);
const isLoadingFile = computed(
    () =>
        props.entry.fileId !== undefined &&
        patientDataStore.isPatientDataFileLoading(props.entry.fileId)
);
const hasFile = computed(
    () => props.entry.fileId !== undefined && props.entry.fileId !== ""
);
const isResult = computed(
    () => props.entry.screeningType === BcCancerScreeningType.Result
);

function showConfirmationModal(): void {
    messageModal.value?.showModal();
}

function downloadFile(): void {
    if (props.entry.fileId) {
        const eventText = isResult.value
            ? "BC Cancer Result PDF"
            : "BC Cancer Screening PDF";
        const fileName = isResult.value
            ? "bc_cancer_result"
            : "bc_cancer_screening";
        SnowPlow.trackEvent({
            action: "download_report",
            text: eventText,
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
            .then((blob) => saveAs(blob, `${fileName}_${dateString}.pdf`))
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
        :subtitle="entry.subtitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="Boolean(entry.fileId)"
    >
        <p class="text-body-1 mb-3">
            <span v-if="isResult" data-testid="bc-cancer-result-body">
                For information about your results, you can contact
                <a
                    href="http://www.bccancer.bc.ca/contact"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >BC Cancer</a
                >.
            </span>
            <span v-else data-testid="bc-cancer-screening-body">
                <a
                    href="http://www.bccancer.bc.ca/screening/cervix/get-screened/what-is-cervical-screening"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >Cervix screening</a
                >
                (Pap test) can stop at age 69 if your results have always been
                normal. Ask your health care provider if you should still be
                tested. To book your next Pap test, contact your health care
                provider or a
                <a
                    href="http://www.bccancer.bc.ca/screening/cervix/clinic-locator"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >medical clinic</a
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
