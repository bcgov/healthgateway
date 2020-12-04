<script lang="ts">
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import moment from "moment";
import { Action, Getter } from "vuex-class";
import BannerError from "@/models/bannerError";
import RequestResult from "@/models/requestResult";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import html2pdf from "html2pdf.js";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import User from "@/models/user";
import { ResultType } from "@/constants/resulttype";
import ErrorTranslator from "@/utility/errorTranslator";
import PDFDefinition from "@/plugins/pdfDefinition";
import LoadingComponent from "@/components/loading.vue";

@Component({
    components: {
        LoadingComponent,
        ProtectiveWordComponent,
    },
})
export default class MedicationHistoryReportComponent extends Vue {
    @Prop() private name!: string;
    @Getter("user", { namespace: "user" })
    private user!: User;
    @Action("getMedicationStatements", { namespace: "medication" })
    private getMedicationStatements!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<RequestResult<MedicationStatementHistory[]>>;
    @Action("addError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;
    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;
    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private isLoading = false;
    private fileMaxRecords = 1000;
    private protectiveWordAttempts = 0;
    private recordsPage: MedicationStatementHistory[] = [];
    private records: MedicationStatementHistory[] = [];

    private get totalFiles(): number {
        return Math.ceil(this.records.length / this.fileMaxRecords);
    }

    private get currentDate() {
        return moment(new Date()).format("ll");
    }

    private get isEmpty() {
        return this.records.length == 0;
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchMedicationStatements();
    }

    private formatDate(date: Date): string {
        return moment(date).format("yyyy-MM-DD");
    }

    private hasGenerated() {
        this.logger.debug("finished generating Medication History PDF...");
        this.isLoading = false;
    }

    private sortEntries() {
        this.recordsPage.sort((a, b) =>
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
                    // Required for the sample page
                    this.recordsPage = this.records.slice(0, 50);
                    this.sortEntries();
                } else if (results.resultStatus == ResultType.Protected) {
                    this.protectiveWordModal.showModal();
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
        this.isLoading = true;

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
                            this.isLoading = false;
                        }
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
                    <h3 id="subject">Health Gateway Medication History</h3>
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
