<script lang="ts">
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import moment from "moment";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ResultType } from "@/constants/resulttype";
import { Action, Getter } from "vuex-class";
import RequestResult from "@/models/requestResult";
import { LaboratoryOrder } from "@/models/laboratory";
import User from "@/models/user";
import { ILogger } from "@/services/interfaces";
import BannerError from "@/models/bannerError";
import ErrorTranslator from "@/utility/errorTranslator";
import LoadingComponent from "@/components/loading.vue";
import html2pdf from "html2pdf.js";
import PDFDefinition from "@/plugins/pdfDefinition";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class COVID19ReportComponent extends Vue {
    @Prop() private name!: string;
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
    private isLoading = false;
    private notFoundText = "Not Found";
    private records: LaboratoryOrder[] = [];

    private fetchLaboratoryResults() {
        this.isLoading = true;
        this.getLaboratoryOrders({ hdid: this.user.hdid })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.records = results.resourcePayload;
                    this.sortEntries();
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

    private sortEntries() {
        this.records.sort((a, b) =>
            a.messageDateTime > b.messageDateTime
                ? -1
                : a.messageDateTime < b.messageDateTime
                ? 1
                : 0
        );
    }

    private get currentDate() {
        return moment(new Date()).format("ll");
    }

    private get isEmpty() {
        return this.records.length == 0;
    }

    private formatDate(date: Date): string {
        return moment(date).format("yyyy-MM-DD");
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchLaboratoryResults();
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating COVID-19 PDF...");
        this.isLoading = true;

        let opt = {
            margin: [25, 15],
            filename: `HealthGateway_COVID19.pdf`,
            image: { type: "jpeg", quality: 1 },
            html2canvas: { dpi: 192, scale: 1.1, letterRendering: true },
            jsPDF: { unit: "pt", format: "letter", orientation: "portrait" },
            pagebreak: { mode: ["avoid-all", "css", "legacy"] },
        };
        html2pdf()
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
                        this.isLoading = false;
                    });
                });
            });
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <div ref="report">
            <section class="pdf-item">
                <div id="pageTitle">
                    <h3 id="subject">Health Gateway COVID-19 History</h3>
                </div>
                <div id="disclaimer" class="mr-1">
                    This record was generated by the BC Provincial Health
                    Gateway application. For any questions, please contact
                    HealthGateway@gov.bc.ca
                </div>
                <hr class="mb-0" />
                <b-row class="pt-2">
                    <b-col>
                        <label>Name:</label> <span>{{ name }}</span>
                    </b-col>
                </b-row>
                <b-row class="pt-2">
                    <b-col>
                        <label>Date Reported:</label>
                        <span>{{ currentDate }}</span>
                    </b-col>
                </b-row>

                <b-row v-if="isEmpty" class="mt-2">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else class="py-3 mt-4 header">
                    <b-col class="col">Date</b-col>
                    <b-col class="col">Test Type</b-col>
                    <b-col class="col">Test Location</b-col>
                    <b-col class="col">Result</b-col>
                </b-row>
                <b-row v-for="item in records" :key="item.id" class="item py-1">
                    <b-col class="col my-auto text-nowrap">
                        {{ formatDate(item.collectedDateTime) }}
                    </b-col>
                    <b-col class="col my-auto">
                        {{ item.labResults[0].testType }}
                    </b-col>
                    <b-col class="col my-auto">
                        {{ item.location }}
                    </b-col>
                    <b-col class="col my-auto">
                        {{ item.labResults[0].labResultOutcome }}
                    </b-col>
                </b-row>
            </section>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle,
#disclaimer,
label,
span {
    color: $primary;
}

#disclaimer {
    font-size: 0.7em;
}

#disclaimer,
span {
    font-weight: bold;
}

label,
#disclaimer {
    font-size: 0.9em;
}
hr {
    border-top: 2px solid $primary;
}

.header {
    background-color: $soft_background;
    font-weight: bold;
    font-size: 0.7em;
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
