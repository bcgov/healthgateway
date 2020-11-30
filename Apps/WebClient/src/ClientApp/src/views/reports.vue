<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import PageTitleComponent from "@/components/pageTitle.vue";
import { ILogger } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import BannerError from "@/models/bannerError";
import MessageModalComponent from "@/components/modal/genericMessage.vue";
import MedicationHistoryReportComponent from "@/components/report/medicationHistory.vue";
import LoadingComponent from "@/components/loading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import { ResultType } from "@/constants/resulttype";
import ErrorTranslator from "@/utility/errorTranslator";
import { IAuthenticationService } from "@/services/interfaces";
import html2pdf from "html2pdf.js";

/**
 * Shallow representation to be used by html2pdf. Does not define the object complety, but sufficiently to avoid using any.
 * To be expanded as needed.
 */
interface PDFDefinition {
    internal: {
        getNumberOfPages(): number;
        pageSize: { getWidth(): number; getHeight(): number };
    };
    setPage(page: number): void;
    setFontSize(size: number): void;
    setTextColor(color: number): void;
    text(text: string, width: number, height: number): void;
}

@Component({
    components: {
        PageTitleComponent,
        MessageModalComponent,
        MedicationHistoryReportComponent,
        LoadingComponent,
        ProtectiveWordComponent,
    },
})
export default class ReportsView extends Vue {
    @Ref("report")
    readonly report!: HTMLElement;
    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;
    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;
    @Getter("user", { namespace: "user" })
    private user!: User;
    @Action("getMedicationStatements", { namespace: "medication" })
    private getMedicationStatements!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<RequestResult<MedicationStatementHistory[]>>;
    @Action("addError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;

    private fullName = "";
    private medicationStatementHistoryPage: MedicationStatementHistory[] = [];
    private medicationStatementHistory: MedicationStatementHistory[] = [];
    private isLoading = false;
    private protectiveWordAttempts = 0;
    private logger!: ILogger;
    private isDataLoaded = false;
    private fileMaxRecords = 1000;
    private reportType = "";
    private reportTypeOptions = [
        { value: "", text: "Select a service" },
        { value: "MED", text: "Medications" },
    ];
    private get totalFiles(): number {
        return Math.ceil(
            this.medicationStatementHistory.length / this.fileMaxRecords
        );
    }

    private showConfirmationModal() {
        this.messageModal.showModal();
    }
    private async generateMedicationHistoryPdf(fileIndex = 0) {
        this.logger.debug("generating Medication History PDF...");
        this.isLoading = true;

        // Breaks records into chunks for multiple files.
        this.medicationStatementHistoryPage = this.medicationStatementHistory.slice(
            fileIndex * this.fileMaxRecords,
            (fileIndex + 1) * this.fileMaxRecords
        );

        let opt = {
            margin: [15, 15],
            filename: `HealthGateway_MedicationHistory_File${
                fileIndex + 1
            }.pdf`,
            image: { type: "jpeg", quality: 1 },
            html2canvas: { dpi: 192, scale: 1.2, letterRendering: true },
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
                            this.generateMedicationHistoryPdf(fileIndex + 1);
                        } else {
                            this.isLoading = false;
                        }
                    });
                });
            });
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.loadName();
        this.fetchMedicationStatements();
    }

    private fetchMedicationStatements(protectiveWord?: string) {
        this.isDataLoaded = false;
        this.isLoading = true;

        this.getMedicationStatements({
            hdid: this.user.hdid,
            protectiveWord: protectiveWord,
        })
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.protectiveWordAttempts = 0;
                    this.medicationStatementHistory = results.resourcePayload;
                    // Required for the sample page
                    this.medicationStatementHistoryPage = this.medicationStatementHistory.slice(
                        0,
                        50
                    );
                    this.sortEntries();
                    this.isDataLoaded = true;
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

    private hasGenerated() {
        this.logger.debug("finished generating Medication History PDF...");
        this.isLoading = false;
    }

    private sortEntries() {
        this.medicationStatementHistoryPage.sort((a, b) =>
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

    private loadName(): void {
        // Load the user name and current email
        let authenticationService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        authenticationService.getOidcUserProfile().then((oidcUser) => {
            if (oidcUser) {
                this.fullName =
                    oidcUser.given_name + " " + oidcUser.family_name;
            }
        });
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <div class="my-3 fluid">
            <b-row>
                <b-col
                    id="healthInsights"
                    class="col-12 col-md-10 col-lg-9 column-wrapper"
                >
                    <PageTitleComponent :title="`Export Records`" />
                    <div class="my-5 p-5 form">
                        <b-row>
                            <b-col class="col-md-6 col-12">
                                <label for="reportType">Report Type</label>
                                <b-form-select
                                    id="reportType"
                                    v-model="reportType"
                                    data-testid="reportType"
                                    :options="reportTypeOptions"
                                >
                                </b-form-select>
                            </b-col>
                            <b-col class="pt-4 col-md-6 col-12">
                                <b-button
                                    variant="primary"
                                    data-testid="exportRecordBtn"
                                    class="mx-auto mt-1 d-block"
                                    :disabled="!isDataLoaded"
                                    @click="showConfirmationModal"
                                >
                                    Export Records
                                </b-button>
                            </b-col>
                        </b-row>
                    </div>
                    <div
                        v-if="reportType == 'MED'"
                        data-testid="medicationReportSample"
                    >
                        <b-col>
                            <div class="mx-auto sample">
                                <div class="scale">
                                    <div ref="report">
                                        <MedicationHistoryReportComponent
                                            :medication-statement-history="
                                                medicationStatementHistoryPage
                                            "
                                            :name="fullName"
                                        />
                                    </div>
                                </div>
                            </div>
                        </b-col>
                    </div>
                    <div v-else>
                        <b-row>
                            <b-col>
                                <img
                                    class="mx-auto d-block"
                                    src="@/assets/images/reports/reports.png"
                                    width="200"
                                    height="auto"
                                    alt="..."
                                />
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="text-center">
                                <h5>
                                    Select a service above to create a report
                                </h5>
                            </b-col>
                        </b-row>
                    </div>
                </b-col>
            </b-row>
        </div>
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="generateMedicationHistoryPdf"
        />
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
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}

.sample {
    border: 2px $lightGrey solid;
    padding: 15px;
    width: 300px;
    height: 400px;
    overflow: hidden;
}
.form {
    background-color: $soft_background;
    border: $lightGrey solid 1px;
    border-radius: 5px 5px 5px 5px;
}
.scale {
    zoom: 0.5;
    -moz-transform: scale(0.5);
}
</style>
