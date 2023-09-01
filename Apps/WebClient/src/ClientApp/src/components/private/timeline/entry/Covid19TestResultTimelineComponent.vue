<script setup lang="ts">
import { saveAs } from "file-saver";
import { computed, ref } from "vue";

import DisplayFieldComponent from "@/components/common/DisplayFieldComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import Covid19TestResultDescriptionComponent from "@/components/private/timeline/entry/Covid19TestResultDescriptionComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import Covid19TestResultTimelineEntry from "@/models/timeline/covid19TestResultTimelineEntry";
import { ILaboratoryService, ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useTimelineStore } from "@/stores/timeline";
import SnowPlow from "@/utility/snowPlow";

interface Props {
    hdid: string;
    entry: Covid19TestResultTimelineEntry;
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
const errorStore = useErrorStore();
const timelineStore = useTimelineStore();

const isLoadingDocument = ref(false);

const sensitiveDocumentModal =
    ref<InstanceType<typeof MessageModalComponent>>();

const cols = computed(() => timelineStore.columnCount);
const entryIcon = computed(
    () => entryTypeMap.get(EntryType.Covid19TestResult)?.icon
);

function formatDate(date: DateWrapper): string {
    return date.format("yyyy-MMM-dd, t");
}

function getOutcomeClasses(outcome: string): string[] {
    switch (outcome?.toUpperCase()) {
        case "NEGATIVE":
            return ["text-success", "font-weight-bold"];
        case "POSITIVE":
            return ["text-error", "font-weight-bold"];
        default:
            return ["font-weight-bold"];
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
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
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
    <TimelineEntryComponent
        :card-id="index + '-' + datekey"
        :entry-icon="entryIcon"
        icon-class="bg-primary"
        :title="entry.summaryTitle"
        :entry="entry"
        :is-mobile-details="isMobileDetails"
        :allow-comment="commentsAreEnabled"
        :has-attachment="entry.reportAvailable"
    >
        <template
            v-if="entry.resultReady && entry.tests.length === 1"
            #header-description
        >
            <div data-testid="laboratoryHeaderDescription">
                <span class="font-weight-bold">Result: </span>
                <span :class="getOutcomeClasses(entry.labResultOutcome)">
                    {{ entry.labResultOutcome }}
                </span>
            </div>
        </template>
        <HgButtonComponent
            v-if="entry.reportAvailable && entry.resultReady"
            class="mt-2 mb-6"
            data-testid="covid-result-download-btn"
            variant="secondary"
            text="Download Full Report"
            prepend-icon="download"
            :loading="isLoadingDocument"
            @click="sensitiveDocumentModal?.showModal()"
        />
        <v-row>
            <v-col>
                <DisplayFieldComponent
                    data-testid="laboratoryReportingLab"
                    name="Reporting Lab"
                    name-class="font-weight-bold"
                    :value="entry.reportingLab"
                />
            </v-col>
        </v-row>
        <template v-for="(test, testIndex) in entry.tests" :key="test.id">
            <v-divider v-if="entry.tests.length > 1" class="my-4" />
            <v-row v-if="test.resultReady && entry.tests.length > 1">
                <v-col>
                    <DisplayFieldComponent
                        data-testid="laboratoryTestResult"
                        name="Result"
                        name-class="font-weight-bold"
                        :value="test.labResultOutcome"
                        :value-class="getOutcomeClasses(test.labResultOutcome)"
                    />
                </v-col>
            </v-row>
            <v-row>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        data-testid="laboratoryTestType"
                        name="Test Type"
                        name-class="font-weight-bold"
                        :value="test.testType"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        :data-testid="`laboratoryTestStatus-${testIndex}`"
                        name="Test Status"
                        name-class="font-weight-bold"
                        :value="test.testStatus"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        name="Collection Date"
                        name-class="font-weight-bold"
                        :value="formatDate(test.collectedDateTime)"
                    />
                </v-col>
                <v-col :cols="cols">
                    <DisplayFieldComponent
                        name="Result Date"
                        name-class="font-weight-bold"
                        :value="formatDate(test.resultDateTime)"
                    />
                </v-col>
            </v-row>
            <v-row v-if="test.resultDescription.length > 0">
                <v-col>
                    <DisplayFieldComponent
                        :data-testid="`laboratoryResultDescription-${testIndex}`"
                        name="Result Description"
                        name-class="font-weight-bold"
                        :value="formatDate(test.resultDateTime)"
                    >
                        <template #value>
                            <Covid19TestResultDescriptionComponent
                                :description="test.resultDescription"
                                :link="test.resultLink"
                            />
                        </template>
                    </DisplayFieldComponent>
                </v-col>
            </v-row>
        </template>
        <MessageModalComponent
            ref="sensitiveDocumentModal"
            title="Sensitive Document"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="getReport"
        />
    </TimelineEntryComponent>
</template>
