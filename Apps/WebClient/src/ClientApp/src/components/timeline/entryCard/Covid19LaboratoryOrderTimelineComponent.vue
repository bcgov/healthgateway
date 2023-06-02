<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faDownload } from "@fortawesome/free-solid-svg-icons";
import { saveAs } from "file-saver";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import Covid19LaboratoryTestDescriptionComponent from "@/components/laboratory/Covid19LaboratoryTestDescriptionComponent.vue";
import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import Covid19LaboratoryOrderTimelineEntry from "@/models/covid19LaboratoryOrderTimelineEntry";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import SnowPlow from "@/utility/snowPlow";

library.add(faDownload);

interface Props {
    hdid: string;
    entry: Covid19LaboratoryOrderTimelineEntry;
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
const laboratoryService = container.get<ILaboratoryService>(
    SERVICE_IDENTIFIER.LaboratoryService
);
const store = useStore();

const isLoadingDocument = ref(false);

const sensitiveDocumentModal = ref<MessageModalComponent>();

const entryIcon = computed<string | undefined>(
    () => entryTypeMap.get(EntryType.Covid19TestResult)?.icon
);

function addError(
    errorType: ErrorType,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addError", { errorType, source, traceId });
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function formatDate(date: DateWrapper): string {
    return date.format("yyyy-MMM-dd, t");
}

function getOutcomeClasses(outcome: string): string[] {
    switch (outcome?.toUpperCase()) {
        case "NEGATIVE":
            return ["text-success"];
        case "POSITIVE":
            return ["text-danger"];
        default:
            return [];
    }
}

function getReport(): void {
    SnowPlow.trackEvent({
        action: "download_report",
        text: "COVID Test PDF",
    });

    isLoadingDocument.value = true;
    laboratoryService
        .getReportDocument(props.entry.id, props.hdid, true)
        .then((result) => {
            const dateString =
                props.entry.displayDate.format("yyyy_MM_dd-HH_mm");
            const report = result.resourcePayload;
            fetch(`data:${report.mediaType};${report.encoding},${report.data}`)
                .then((response) => response.blob())
                .then((blob) => saveAs(blob, `COVID_Result_${dateString}.pdf`));
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addError(
                    ErrorType.Download,
                    ErrorSourceType.Covid19LaboratoryReport,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoadingDocument.value = false;
        });
}
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        :title="entry.summaryTitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="entry.reportAvailable"
    >
        <div v-if="entry.tests.length === 1" slot="header-description">
            <strong
                v-show="entry.resultReady"
                data-testid="laboratoryHeaderDescription"
            >
                <span>Result:</span>
                <span :class="getOutcomeClasses(entry.labResultOutcome)">
                    {{ entry.labResultOutcome }}
                </span>
            </strong>
        </div>
        <div slot="details-body">
            <div
                v-if="entry.reportAvailable && entry.resultReady"
                class="my-3"
                data-testid="laboratoryReportAvailable"
            >
                <hg-button
                    data-testid="covid-result-download-btn"
                    variant="secondary"
                    :disabled="isLoadingDocument"
                    @click="sensitiveDocumentModal?.showModal()"
                >
                    <b-spinner v-if="isLoadingDocument" class="mr-1" small />
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
            <div class="my-2">
                <div data-testid="laboratoryReportingLab">
                    <strong>Reporting Lab:</strong>
                    {{ entry.reportingLab }}
                </div>
            </div>
            <div
                v-for="test in entry.tests"
                :key="test.id"
                :data-testid="`laboratoryTestBlock-${index}`"
            >
                <hr v-if="entry.tests.length > 1" />
                <div data-testid="laboratoryTestType" class="my-2">
                    <strong
                        v-if="test.resultReady && entry.tests.length > 1"
                        data-testid="laboratoryTestResult"
                    >
                        <span>Result:</span>
                        <span :class="getOutcomeClasses(test.labResultOutcome)">
                            {{ test.labResultOutcome }}
                        </span>
                    </strong>
                </div>
                <div data-testid="laboratoryTestType" class="my-2">
                    <strong>Test Type:</strong>
                    {{ test.testType }}
                </div>
                <div
                    :data-testid="`laboratoryTestStatus-${index}`"
                    class="my-2"
                >
                    <strong>Test Status:</strong>
                    {{ test.testStatus }}
                </div>
                <div class="my-2">
                    <strong>Collection Date:</strong>
                    <span class="text-nowrap">
                        {{ formatDate(test.collectedDateTime) }}
                    </span>
                </div>
                <div class="my-2">
                    <strong>Result Date:</strong>
                    <span class="text-nowrap">
                        {{ formatDate(test.resultDateTime) }}
                    </span>
                </div>
                <div
                    v-if="test.resultDescription.length > 0"
                    class="my-2"
                    :data-testid="`laboratoryResultDescription-${index}`"
                >
                    <strong>Result Description:</strong>
                    <Covid19LaboratoryTestDescriptionComponent
                        :description="test.resultDescription"
                        :link="test.resultLink"
                    />
                </div>
            </div>
            <MessageModalComponent
                ref="sensitiveDocumentModal"
                title="Sensitive Document Download"
                message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
                @submit="getReport"
            />
        </div>
    </EntryCardTimelineComponent>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.col {
    padding: 0px;
    margin: 0px;
}

.row {
    padding: 0;
    margin: 0px;
}
</style>
