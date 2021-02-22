<script lang="ts">
import html2pdf from "html2pdf.js";
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder } from "@/models/laboratory";
import PatientData from "@/models/patientData";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import PDFDefinition from "@/plugins/pdfDefinition";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        ReportHeaderComponent,
    },
})
export default class COVID19ReportComponent extends Vue {
    @Prop() private startDate!: string | null;
    @Prop() private endDate!: string | null;
    @Prop() private patientData!: PatientData | null;

    @Action("retrieve", { namespace: "laboratory" })
    retrieveLaboratory!: (params: { hdid: string }) => Promise<void>;

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: LaboratoryOrder[];

    @Getter("isLoading", { namespace: "laboratory" })
    isLaboratoryLoading!: boolean;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private isPreview = true;

    @Watch("isLaboratoryLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLaboratoryLoading;
    }

    private get visibleRecords(): LaboratoryOrder[] {
        let records = this.laboratoryOrders.filter((record) => {
            let filterStart = true;
            if (this.startDate !== null) {
                filterStart = new DateWrapper(
                    record.labResults[0].collectedDateTime
                ).isAfterOrSame(new DateWrapper(this.startDate));
            }

            let filterEnd = true;
            if (this.endDate !== null) {
                filterEnd = new DateWrapper(
                    record.labResults[0].collectedDateTime
                ).isBeforeOrSame(new DateWrapper(this.endDate));
            }
            return filterStart && filterEnd;
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(
                a.labResults[0].collectedDateTime
            );
            const secondDate = new DateWrapper(
                b.labResults[0].collectedDateTime
            );

            const value = firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;

            return value;
        });

        return records;
    }

    private get isEmpty() {
        return this.visibleRecords.length == 0;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveLaboratory({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading Covid19 data: ${err}`);
        });
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating COVID-19 PDF...");
        this.isPreview = false;

        let opt = {
            margin: [25, 15],
            filename: `HealthGateway_COVID19.pdf`,
            image: { type: "jpeg", quality: 1 },
            html2canvas: { dpi: 96, scale: 2, letterRendering: true },
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
                        `Page ${i} of ${totalPages}`,
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
                        this.isPreview = true;
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
                    title="Health Gateway COVID-19 Test Result History"
                    :patient-data="patientData"
                />
                <b-row v-if="isEmpty && (!isLaboratoryLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col>Date</b-col>
                    <b-col>Test Type</b-col>
                    <b-col>Test Location</b-col>
                    <b-col>Result</b-col>
                </b-row>
                <b-row
                    v-for="item in visibleRecords"
                    :key="item.id"
                    class="item py-1"
                >
                    <b-col
                        data-testid="covid19ItemDate"
                        class="my-auto text-nowrap"
                    >
                        {{ formatDate(item.labResults[0].collectedDateTime) }}
                    </b-col>
                    <b-col data-testid="covid19ItemTestType" class="my-auto">
                        {{ item.labResults[0].testType }}
                    </b-col>
                    <b-col data-testid="covid19ItemLocation" class="my-auto">
                        {{ item.location }}
                    </b-col>
                    <b-col data-testid="covid19ItemResult" class="my-auto">
                        {{ item.labResults[0].labResultOutcome }}
                    </b-col>
                </b-row>
            </section>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.header {
    color: $primary;
    background-color: $soft_background;
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
