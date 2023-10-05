<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import ImmunizationReportComponent from "@/components/private/reports/ImmunizationReportComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import Report from "@/models/report";
import { ReportFilterBuilder } from "@/models/reportFilter";
import {
    ReportFormatType,
    reportMimeTypeMap,
    TemplateType,
} from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useReportStore } from "@/stores/report";
import EventTracker from "@/utility/eventTracker";

interface Props {
    hdid: string;
    isDependent?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isDependent: false,
});

const reportFilter = ReportFilterBuilder.buildEmpty();

defineExpose({ showDialog });

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

const reportStore = useReportStore();
const errorStore = useErrorStore();

const messageModal = ref<InstanceType<typeof MessageModalComponent>>();
const recommendationsReportComponent =
    ref<InstanceType<typeof ImmunizationReportComponent>>();

const isVisible = ref(false);
const isGeneratingReport = ref(false);
const hasRecords = ref(false);
const reportFormatType = ref<ReportFormatType>(ReportFormatType.PDF);

const template = computed(() =>
    props.isDependent
        ? TemplateType.DependentImmunizationRecommendation
        : TemplateType.ImmunizationRecommendation
);

function showConfirmationModal(type: ReportFormatType): void {
    reportFormatType.value = type;
    messageModal.value?.showModal();
}

function trackDownload() {
    let reportEventName =
        entryTypeMap.get(EntryType.Immunization)?.reportEventName ?? "";

    const formatTypeName = ReportFormatType[reportFormatType.value];
    const eventName = `${reportEventName} (${formatTypeName})`;

    if (!props.isDependent) {
        EventTracker.downloadReport(eventName);
    } else {
        EventTracker.downloadReport(`Dependent_${eventName}`);
    }
}

function downloadReport() {
    isGeneratingReport.value = true;

    trackDownload();

    recommendationsReportComponent.value
        ?.generateReport(
            reportFormatType.value,
            reportStore.getHeaderData(props.hdid, reportFilter)
        )
        .then((result: RequestResult<Report>) => {
            const mimeType =
                reportMimeTypeMap.get(reportFormatType.value) ?? "";
            const downloadLink = `data:${mimeType};base64,${result.resourcePayload.data}`;
            fetch(downloadLink).then((res) =>
                res
                    .blob()
                    .then((blob) =>
                        saveAs(blob, result.resourcePayload.fileName)
                    )
            );
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("recommendationsDialog");
            } else {
                errorStore.addError(
                    ErrorType.Download,
                    ErrorSourceType.ExportRecords,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isGeneratingReport.value = false;
        });
}

function visitVaccinationBooking() {
    EventTracker.click("bookvaccine");
    window.open("https://www.getvaccinated.gov.bc.ca/s/", "_blank", "noopener");
}

function showDialog() {
    isVisible.value = true;
}
</script>

<template>
    <div class="d-flex justify-content">
        <v-dialog v-model="isVisible" max-width="1000px" persistent>
            <v-card data-testid="recommendations-dialog">
                <v-card-title class="px-0">
                    <v-toolbar
                        title="Vaccine Recommendations"
                        color="transparent"
                    >
                        <HgIconButtonComponent
                            id="recommendations-dialog-close-button"
                            data-testid="recommendations-dialog-close-button"
                            icon="close"
                            @click="isVisible = false"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="pa-4">
                    <TooManyRequestsComponent
                        location="recommendationsDialog"
                    />
                    <ImmunizationReportComponent
                        ref="recommendationsReportComponent"
                        :hdid="hdid"
                        :filter="reportFilter"
                        :is-dependent="isDependent"
                        :template="template"
                        force-show
                        hide-immunizations
                        hide-recommendation-header
                        @on-is-empty-changed="hasRecords = !$event"
                    >
                        <template #recommendations-description>
                            <p v-if="isDependent">
                                School-aged children are offered most
                                immunizations in their school, particularly in
                                grades 6 and 9. The school can let you know
                                which vaccines are offered. You need to book an
                                appointment to get your child vaccinated against
                                COVID‑19.
                                <a
                                    href="https://www2.gov.bc.ca/gov/content/covid-19/vaccine"
                                    target="_blank"
                                    rel="noopener"
                                    class="text-link"
                                    >Find out how.</a
                                >
                            </p>
                            <template v-else>
                                <p>
                                    Vaccine recommendations are based on the
                                    <a
                                        href="https://immunizebc.ca/tools-resources/immunization-schedules"
                                        target="_blank"
                                        rel="noopener"
                                        class="text-link"
                                        >BC Immunization Schedule</a
                                    >. For information on booking COVID or Flu
                                    vaccinations, please visit the
                                    <a
                                        href="https://www2.gov.bc.ca/gov/content/covid-19/info/response"
                                        target="_blank"
                                        rel="noopener"
                                        class="text-link"
                                        >BC respiratory illness page</a
                                    >.
                                </p>
                                <HgButtonComponent
                                    class="mb-4"
                                    variant="primary"
                                    text="Book a COVID or Flu Vaccination"
                                    data-testid="book-vaccination-button"
                                    @click="visitVaccinationBooking"
                                />
                            </template>
                        </template>
                    </ImmunizationReportComponent>
                </v-card-text>
                <v-card-actions class="pa-4 justify-end">
                    <HgButtonComponent
                        variant="secondary"
                        text="Close"
                        data-testid="close-recommendations-dialog-button"
                        @click="isVisible = false"
                    />
                    <v-menu data-testid="export-record-menu">
                        <template #activator="{ props: slotProps }">
                            <HgButtonComponent
                                id="export-recommendations-record-button"
                                text="Download Immunization Record"
                                variant="primary"
                                data-testid="export-recommendations-record-button"
                                v-bind="slotProps"
                                :disabled="!hasRecords"
                                :loading="isGeneratingReport"
                            />
                        </template>
                        <v-list>
                            <v-list-item
                                title="PDF"
                                @click="
                                    showConfirmationModal(ReportFormatType.PDF)
                                "
                            />
                            <v-list-item
                                title="CSV"
                                @click="
                                    showConfirmationModal(ReportFormatType.CSV)
                                "
                            />
                            <v-list-item
                                title="XLSX"
                                @click="
                                    showConfirmationModal(ReportFormatType.XLSX)
                                "
                            />
                        </v-list>
                    </v-menu>
                </v-card-actions>
            </v-card>
        </v-dialog>
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="downloadReport"
        />
    </div>
</template>
