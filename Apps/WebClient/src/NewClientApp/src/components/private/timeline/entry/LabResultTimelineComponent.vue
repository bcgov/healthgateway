<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDataTableComponent from "@/components/common/HgDataTableComponent.vue";
import InfoTooltipComponent from "@/components/common/InfoTooltipComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import ReportField from "@/models/reportField";
import LabResultTimelineEntry from "@/models/timeline/labResultTimelineEntry";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useTimelineStore } from "@/stores/timeline";
import SnowPlow from "@/utility/snowPlow";

interface Props {
    hdid: string;
    entry: LabResultTimelineEntry;
    index: number;
    datekey: string;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const fields: ReportField[] = [
    {
        key: "testName",
        title: "Test Name",
    },
    {
        key: "result",
        title: "Result",
    },
    {
        key: "status",
        title: "Status",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const laboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);
const errorStore = useErrorStore();
const timelineStore = useTimelineStore();

const isLoadingDocument = ref(false);
const showInfoDetails = ref(false);
const messageModal = ref<InstanceType<typeof MessageModalComponent>>();

const cols = computed(() => timelineStore.columnCount);
const entryIcon = computed<string | undefined>(
    () => entryTypeMap.get(EntryType.LabResult)?.icon
);

function getStatusInfoId(labPdfId: string, index: number): string {
    const isModalIndicator = props.isMobileDetails ? "1" : "0";
    return `laboratory-test-status-info-${labPdfId}-${index}-${isModalIndicator}`;
}

function showConfirmationModal(): void {
    messageModal.value?.showModal();
}

function formatDate(date?: DateWrapper): string | undefined {
    return date?.format("yyyy-MMM-dd, t");
}

function getReport(): void {
    SnowPlow.trackEvent({
        action: "download_report",
        text: "Laboratory Report PDF",
    });

    isLoadingDocument.value = true;
    laboratoryService
        .getReportDocument(props.entry.id, props.hdid, false)
        .then((result) => {
            const dateString =
                props.entry.timelineDateTime.format("yyyy_MM_dd-HH_mm");
            const report = result.resourcePayload;
            fetch(`data:${report.mediaType};${report.encoding},${report.data}`)
                .then((response) => response.blob())
                .then((blob) =>
                    saveAs(blob, `Laboratory_Report_${dateString}.pdf`)
                );
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Download,
                    ErrorSourceType.LaboratoryReport,
                    err.traceId
                );
            }
        })
        .finally(() => (isLoadingDocument.value = false));
}
</script>

<template>
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.commonName"
        :subtitle="`Order Status: ${entry.orderStatus}`"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :has-attachment="entry.reportAvailable"
        :allow-comment="commentsAreEnabled"
    >
        <v-row>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="laboratory-collection-date"
                    name="Collection Date"
                    name-class="font-weight-bold"
                    :value="formatDate(entry.collectionDateTime)"
                />
            </v-col>
            <v-col :cols="cols">
                <DisplayFieldComponent
                    data-testid="laboratory-ordering-provider"
                    name="Ordering Provider"
                    name-class="font-weight-bold"
                    :value="entry.orderingProvider"
                />
            </v-col>
        </v-row>
        <v-row class="mb-3">
            <v-col>
                <DisplayFieldComponent
                    data-testid="laboratory-reporting-lab"
                    name="Reporting Lab"
                    name-class="font-weight-bold"
                    :value="entry.reportingLab"
                />
            </v-col>
        </v-row>
        <p class="text-body-1">
            <span data-testid="reporting-lab-information-text"
                >Find resources about your lab tests.</span
            >
            <v-icon
                data-testid="other-resources-info-button"
                aria-label="More Information"
                class="ml-2"
                icon="info-circle"
                color="primary"
                size="small"
                @click="showInfoDetails = !showInfoDetails"
            />
        </p>
        <v-slide-y-transition>
            <v-alert
                v-show="showInfoDetails"
                data-testid="other-resources-info-popover"
                class="d-print-none mb-6"
                type="info"
                variant="outlined"
                border
            >
                <p>
                    Use these websites to learn about specific types of lab
                    tests:
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
                            href="https://www.mayocliniclabs.com/"
                            target="_blank"
                            rel="noopener"
                            class="text-link"
                        >
                            Mayo Clinic Laboratories
                        </a>
                    </li>
                    <li>
                        <a
                            href="https://www.mypathologyreport.ca/"
                            target="_blank"
                            rel="noopener"
                            class="text-link"
                        >
                            For pathology tests (like a biopsy)
                        </a>
                    </li>
                </ul>
            </v-alert>
        </v-slide-y-transition>
        <HgButtonComponent
            v-if="entry.reportAvailable"
            class="mt-2 mb-6"
            data-testid="laboratory-report-download-btn"
            variant="secondary"
            text="Download Full Report"
            prepend-icon="download"
            :loading="isLoadingDocument"
            @click="showConfirmationModal"
        />
        <HgDataTableComponent
            data-testid="laboratoryResultTable"
            class="overflow-auto"
            :style="{ 'max-height': isMobileDetails ? 'none' : '300px' }"
            fixed-header
            :items="entry.tests"
            :fields="fields"
            density="compact"
        >
            <template #header-result>
                <span>Result</span>
                <InfoTooltipComponent
                    data-testid="result-info-button"
                    tooltip-testid="result-info-popover"
                    class="ml-2"
                    size="x-small"
                >
                    <p>
                        Follow the instructions from your health care provider.
                        When needed, they can explain what your results mean.
                    </p>
                    <p class="mb-2">Remember:</p>
                    <ul>
                        <li>
                            <span class="font-weight-bold">Ranges</span> are
                            different between laboratories
                        </li>
                        <li>
                            <span class="font-weight-bold">“Out of range”</span>
                            results may be
                            <span class="font-weight-bold">normal</span> for you
                        </li>
                    </ul>
                </InfoTooltipComponent>
            </template>
            <template #item-status="data">
                <span>{{ data.item.status }}</span>
                <InfoTooltipComponent
                    :id="`${getStatusInfoId(entry.labPdfId, data.item.index)}`"
                    data-testid="laboratory-test-status-info-button"
                    :tooltip-testid="`${getStatusInfoId(
                        entry.labPdfId,
                        data.item.index
                    )}-popover`"
                    class="ml-2"
                >
                    <p
                        v-for="(paragraph, statusIndex) in data.item.statusInfo"
                        :key="statusIndex"
                    >
                        {{ paragraph }}
                    </p>
                </InfoTooltipComponent>
            </template>
        </HgDataTableComponent>
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getReport"
        />
    </TimelineEntryComponent>
</template>
