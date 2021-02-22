<script lang="ts">
import html2pdf from "html2pdf.js";
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import PatientData from "@/models/patientData";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import PDFDefinition from "@/plugins/pdfDefinition";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        ProtectiveWordComponent,
        ReportHeaderComponent,
    },
})
export default class MedicationHistoryReportComponent extends Vue {
    @Prop() private startDate!: string | null;
    @Prop() private endDate!: string | null;
    @Prop() private patientData!: PatientData | null;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Action("retrieve", { namespace: "medication" })
    private retrieveMedications!: (params: { hdid: string }) => Promise<void>;

    @Getter("isLoading", { namespace: "medication" })
    isLoading!: boolean;

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private fileMaxRecords = 1000;

    private recordsPage: MedicationStatementHistory[] = [];
    private isPreview = true;

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private get totalFiles(): number {
        return Math.ceil(this.visibleRecords.length / this.fileMaxRecords);
    }

    private get isEmpty() {
        return this.visibleRecords.length === 0;
    }

    private get visibleRecords(): MedicationStatementHistory[] {
        let records = this.medicationStatements.filter((record) => {
            let filterStart = true;
            if (this.startDate !== null) {
                filterStart = new DateWrapper(
                    record.dispensedDate
                ).isAfterOrSame(new DateWrapper(this.startDate));
            }

            let filterEnd = true;
            if (this.endDate !== null) {
                filterEnd = new DateWrapper(
                    record.dispensedDate
                ).isBeforeOrSame(new DateWrapper(this.endDate));
            }
            return filterStart && filterEnd;
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.dispensedDate);
            const secondDate = new DateWrapper(b.dispensedDate);

            const value = firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;

            return value;
        });

        // Required for the sample page
        this.recordsPage = records;

        return records;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveMedications({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication data: ${err}`);
        });
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    public async generatePdf(fileIndex = 0): Promise<void> {
        this.logger.debug("generating Medication History PDF...");
        this.isPreview = false;
        // Breaks records into chunks for multiple files.
        this.recordsPage = this.visibleRecords.slice(
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
            :is-loading="isLoading"
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
