<script lang="ts">
import html2pdf from "html2pdf.js";
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import ReportHeaderComponent from "@/components/report/header.vue";
import { ActionType } from "@/constants/actionType";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import PatientData from "@/models/patientData";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import PDFDefinition from "@/plugins/pdfDefinition";
import { ILogger } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

@Component({
    components: {
        ProtectiveWordComponent,
        ReportHeaderComponent,
    },
})
export default class MedicationHistoryReportComponent extends Vue {
    @Prop() private startDate?: string;
    @Prop() private endDate?: string;
    @Prop() private patientData?: PatientData;
    @Getter("user", { namespace: "user" })
    private user!: User;
    @Action("getMedicationStatements", { namespace: "medication" })
    private getMedicationStatements!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<RequestResult<MedicationStatementHistory[]>>;
    @Action("addBannerError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;
    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;
    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private fileMaxRecords = 1000;
    private protectiveWordAttempts = 0;
    private recordsPage: MedicationStatementHistory[] = [];
    private records: MedicationStatementHistory[] = [];
    private isPreview = true;
    private isLoading = false;

    @Watch("startDate")
    @Watch("endDate")
    private onDateChanged() {
        this.fetchMedicationStatements();
    }

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private get totalFiles(): number {
        return Math.ceil(this.records.length / this.fileMaxRecords);
    }

    private get isEmpty() {
        return this.records.length == 0;
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchMedicationStatements();
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    private hasGenerated() {
        this.logger.debug("finished generating Medication History PDF...");
        this.isLoading = false;
    }

    private filterAndSortEntries() {
        this.records = this.records.filter((record) => {
            return (
                (!this.startDate ||
                    new DateWrapper(record.dispensedDate).isAfterOrSame(
                        new DateWrapper(this.startDate)
                    )) &&
                (!this.endDate ||
                    new DateWrapper(record.dispensedDate).isBeforeOrSame(
                        new DateWrapper(this.endDate)
                    ))
            );
        });
        this.records.sort((a, b) =>
            a.dispensedDate > b.dispensedDate
                ? -1
                : a.dispensedDate < b.dispensedDate
                ? 1
                : 0
        );
    }

    private onProtectiveWordSubmit(value: string) {
        this.fetchMedicationStatements(value);
    }

    private onProtectiveWordCancel() {
        // Does nothing as it won't be able to fetch pharmanet data.
        this.logger.debug("protective word cancelled");
    }

    private fetchMedicationStatements(protectiveWord?: string) {
        this.isLoading = true;

        this.getMedicationStatements({
            hdid: this.user.hdid,
            protectiveWord: protectiveWord,
        })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.protectiveWordAttempts = 0;
                    this.records = results.resourcePayload;
                    this.filterAndSortEntries();
                    // Required for the sample page
                    this.recordsPage = this.records;
                } else if (
                    results.resultStatus == ResultType.ActionRequired &&
                    results.resultError?.actionCode == ActionType.Protected
                ) {
                    //this.protectiveWordModal.showModal();
                    this.protectiveWordAttempts++;
                } else {
                    this.logger.error(
                        "Error returned from the medication statements call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Medications Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Fetch Medications Error",
                        err
                    )
                );
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    public async generatePdf(fileIndex = 0): Promise<void> {
        this.logger.debug("generating Medication History PDF...");
        this.isPreview = false;
        // Breaks records into chunks for multiple files.
        this.recordsPage = this.records.slice(
            fileIndex * this.fileMaxRecords,
            (fileIndex + 1) * this.fileMaxRecords
        );

        let opt = {
            margin: [25, 15],
            filename: `HealthGateway_MedicationHistory_File${
                fileIndex + 1
            }.pdf`,
            image: { type: "jpeg", quality: 1 },
            html2canvas: { dpi: 192, scale: 1.1, letterRendering: true },
            jsPDF: { unit: "pt", format: "letter", orientation: "portrait" },
            pagebreak: { mode: ["avoid-all", "css", "legacy"] },
        };
        return html2pdf()
            .set(opt)
            .from(this.report)
            .toPdf()
            .get("pdf")
            .then((pdf: PDFDefinition) => {
                // Add footer with page numbers
                var totalPages = pdf.internal.getNumberOfPages();
                for (let i = 1; i <= totalPages; i++) {
                    pdf.setPage(i);
                    pdf.setFontSize(10);
                    pdf.setTextColor(150);
                    pdf.text(
                        `Page ${i} of ${totalPages} - File ${
                            fileIndex + 1
                        } of ${this.totalFiles}`,
                        pdf.internal.pageSize.getWidth() / 2 - 55,
                        pdf.internal.pageSize.getHeight() - 10
                    );
                }
            })
            .save()
            .output("bloburl")
            .then((pdfBlobUrl: RequestInfo) => {
                fetch(pdfBlobUrl).then((res) => {
                    res.blob().then(() => {
                        if (fileIndex + 1 < this.totalFiles) {
                            this.generatePdf(fileIndex + 1);
                        } else {
                            this.isPreview = true;
                        }
                    });
                });
            });
    }
}
</script>

<template>
    <div>
        <div ref="report">
            <section class="pdf-item">
                <ReportHeaderComponent
                    v-show="!isPreview"
                    :start-date="startDate"
                    :end-date="endDate"
                    title="Health Gateway Medication History"
                    :patient-data="patientData"
                />
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col class="col-1">Date</b-col>
                    <b-col class="col-1">DIN/PIN</b-col>
                    <b-col class="col-2">Brand</b-col>
                    <b-col class="col-2">Generic</b-col>
                    <b-col class="col-1">Practitioner</b-col>
                    <b-col class="col-1">Quantity</b-col>
                    <b-col class="col-1">Strength</b-col>
                    <b-col class="col-1">Form</b-col>
                    <b-col class="col-1">Manufacturer</b-col>
                </b-row>
                <b-row
                    v-for="item in recordsPage"
                    :key="item.prescriptionIdentifier + item.dispensedDate"
                    class="item py-1"
                >
                    <b-col class="col-1 my-auto text-nowrap">
                        {{ formatDate(item.dispensedDate) }}
                    </b-col>
                    <b-col class="col-1 my-auto">
                        {{ item.medicationSummary.din }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ item.medicationSummary.brandName }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ item.medicationSummary.genericName || notFoundText }}
                    </b-col>
                    <b-col class="col-1 my-auto">{{
                        item.practitionerSurname
                    }}</b-col>
                    <b-col class="col-1 my-auto">
                        {{ item.medicationSummary.quantity }}
                    </b-col>
                    <b-col class="col-1 my-auto">
                        {{
                            item.medicationSummary.strength +
                                item.medicationSummary.strengthUnit ||
                            notFoundText
                        }}
                    </b-col>
                    <b-col class="col-1 my-auto">
                        {{ item.medicationSummary.form || notFoundText }}
                    </b-col>
                    <b-col class="col-1 my-auto">
                        {{
                            item.medicationSummary.manufacturer || notFoundText
                        }}
                    </b-col>
                </b-row>
            </section>
        </div>
        <ProtectiveWordComponent
            ref="protectiveWordModal"
            :error="protectiveWordAttempts > 1"
            :is-loading="isLoading"
            @submit="onProtectiveWordSubmit"
            @cancel="onProtectiveWordCancel"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.header {
    background-color: $soft_background;
    color: $primary;
    font-weight: bold;
    font-size: 0.8em;
    text-align: center;
}

.item {
    font-size: 0.6em;
    border-bottom: solid 1px $soft_background;
    page-break-inside: avoid;
    text-align: center;
}

.item:nth-child(odd) {
    background-color: $medium_background;
}
.item:nth-child(even) {
    background-color: $soft_background;
}
</style>
