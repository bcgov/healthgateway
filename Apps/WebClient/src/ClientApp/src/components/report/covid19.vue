<script lang="ts">
import html2pdf from "html2pdf.js";
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { ResultType } from "@/constants/resulttype";
import BannerError from "@/models/bannerError";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder } from "@/models/laboratory";
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
        ReportHeaderComponent,
    },
})
export default class COVID19ReportComponent extends Vue {
    @Prop() private startDate?: string;
    @Prop() private endDate?: string;
    @Prop() private patientData?: PatientData;
    @Getter("user", { namespace: "user" })
    private user!: User;
    @Action("addError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;
    @Action("getOrders", { namespace: "laboratory" })
    getLaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<RequestResult<LaboratoryOrder[]>>;
    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private records: LaboratoryOrder[] = [];
    private isPreview = true;
    private isLoading = false;

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    @Watch("startDate")
    @Watch("endDate")
    private onDateChanged() {
        this.fetchLaboratoryResults();
    }

    private fetchLaboratoryResults() {
        this.isLoading = true;
        this.getLaboratoryOrders({ hdid: this.user.hdid })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.records = results.resourcePayload;
                    this.filterAndSortEntries();
                } else {
                    this.logger.error(
                        "Error returned from the LaboratoryResults call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Laboratory Results Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Fetch Laboratory Results Error",
                        err
                    )
                );
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private filterAndSortEntries() {
        this.records = this.records.filter((record) => {
            return (
                (!this.startDate ||
                    new DateWrapper(
                        record.labResults[0].collectedDateTime
                    ).isAfterOrSame(new DateWrapper(this.startDate))) &&
                (!this.endDate ||
                    new DateWrapper(
                        record.labResults[0].collectedDateTime
                    ).isBeforeOrSame(new DateWrapper(this.endDate)))
            );
        });
        this.records.sort((a, b) =>
            a.labResults[0].collectedDateTime >
            b.labResults[0].collectedDateTime
                ? -1
                : a.labResults[0].collectedDateTime <
                  b.labResults[0].collectedDateTime
                ? 1
                : 0
        );
    }

    private get isEmpty() {
        return this.records.length == 0;
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchLaboratoryResults();
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
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col>Date</b-col>
                    <b-col>Test Type</b-col>
                    <b-col>Test Location</b-col>
                    <b-col>Result</b-col>
                </b-row>
                <b-row v-for="item in records" :key="item.id" class="item py-1">
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
