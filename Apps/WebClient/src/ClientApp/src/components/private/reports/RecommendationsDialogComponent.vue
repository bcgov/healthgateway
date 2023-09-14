<script setup lang="ts">
import saveAs from "file-saver";
import { ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import ImmunizationReportComponent from "@/components/private/reports/ImmunizationReportComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import Report from "@/models/report";
import { ReportFilterBuilder } from "@/models/reportFilter";
import { ReportFormatType, reportMimeTypeMap } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import { ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useReportStore } from "@/stores/report";

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

function showConfirmationModal(type: ReportFormatType): void {
    reportFormatType.value = type;
    messageModal.value?.showModal();
}

function downloadReport() {
    isGeneratingReport.value = true;
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

function showDialog() {
    isVisible.value = true;
}
</script>

<template>
    <div class="d-flex justify-content">
        <v-dialog v-model="isVisible" max-width="1000px" persistent>
            <v-card data-testid="recommendations-dialog">
                <v-card-title class="px-o">
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
                <v-card-text>
                    <div class="mx-2">
                        <TooManyRequestsComponent
                            location="recommendationsDialog"
                        />
                    </div>
                    <ImmunizationReportComponent
                        ref="recommendationsReportComponent"
                        :hdid="hdid"
                        :filter="reportFilter"
                        hide-immunizations
                        hide-recommendation-header
                        @on-is-empty-changed="hasRecords = !$event"
                    />
                </v-card-text>
                <v-card-actions class="pa-4 justify-end">
                    <HgButtonComponent
                        variant="secondary"
                        text="Close"
                        data-testid="close-recommendations-dialog-btn"
                        @click="isVisible = false"
                    />
                    <v-menu data-testid="export-record-menu">
                        <template #activator="{ props: slotProps }">
                            <HgButtonComponent
                                id="export-recommendations-record-btn"
                                text="Download"
                                variant="primary"
                                data-testid="export-recommendations-record-btn"
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
