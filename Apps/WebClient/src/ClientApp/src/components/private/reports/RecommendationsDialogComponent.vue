<script setup lang="ts">
import saveAs from "file-saver";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import ImmunizationReportComponent from "@/components/private/reports/ImmunizationReportComponent.vue";
import { EntryType } from "@/constants/entryType";
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
import { Action, Actor, Destination, Origin, Text } from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useLayoutStore } from "@/stores/layout";
import { useReportStore } from "@/stores/report";
import EventDataUtility from "@/utility/eventDataUtility";

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
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const reportStore = useReportStore();
const errorStore = useErrorStore();
const layoutStore = useLayoutStore();

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
    trackingService.trackEvent({
        action: Action.Download,
        text: Text.Export,
        dataset: EventDataUtility.getDataset(EntryType.Immunization),
        format: EventDataUtility.getFormat(reportFormatType.value),
        actor: props.isDependent ? Actor.Guardian : Actor.User,
    });
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
            logger.error(err.message);
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
    trackingService.trackEvent({
        action: Action.Visit,
        text: Text.ExternalLink,
        destination: Destination.BookVaccine,
        origin: Origin.ImmunizationRecommendationDialog,
    });
    window.open("https://www.getvaccinated.gov.bc.ca/s/", "_blank", "noopener");
}

function showDialog() {
    isVisible.value = true;
}
</script>

<template>
    <div class="d-flex justify-content">
        <v-dialog
            v-model="isVisible"
            max-width="1000px"
            :fullscreen="layoutStore.isMobile"
            persistent
        >
            <v-card
                data-testid="recommendations-dialog"
                :class="{ 'mobile-padding': layoutStore.isMobile }"
            >
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
                                        href="https://www.healthlinkbc.ca/health-library/immunizations/schedules"
                                        target="_blank"
                                        rel="noopener"
                                        class="text-link"
                                        >BC Vaccine Schedule</a
                                    >. For more information, please visit
                                    <a
                                        href="https://www.healthlinkbc.ca/health-library/immunizations"
                                        target="_blank"
                                        rel="noopener"
                                        class="text-link"
                                        >Immunizations | HealthLink BC</a
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
                <v-card-actions
                    class="pa-4 justify-end"
                    :class="{ 'fixed-bottom-actions': layoutStore.isMobile }"
                >
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
                                text="Download Record"
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

<style scoped lang="scss">
.fixed-bottom-actions {
    background-color: white;
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    z-index: 2;
}

.mobile-padding {
    // Padding is required to ensure that all dialog content is visible and not hidden by the fixed actions
    padding-bottom: 65px;
}
</style>
