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
</style>
<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <b-row class="my-3 fluid justify-content-md-center">
            <b-col
                id="healthInsights"
                class="col-12 col-md-10 col-lg-9 column-wrapper"
            >
                <PageTitleComponent
                    :title="`Health Gateway Medication History Report`"
                />
                <div>
                    <p>
                        Download a copy of your PharmaNet record of prescription
                        medication dispenses. This report will generate your
                        full history in the PharmaNet system.
                    </p>
                </div>
            </b-col>
        </b-row>
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
            <b-col>
                <b-button
                    variant="primary"
                    class="mx-auto mt-3 d-block"
                    :disabled="!isDataLoaded"
                    @click="showConfirmationModal"
                >
                    Download your report
                </b-button>
            </b-col>
        </b-row>
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

        <MedicationHistoryReportComponent
            :medication-statement-history="medicationStatementHistory"
            :name="fullName"
        />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
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
import html2pdf from "html2pdf";

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
    private authenticationService: IAuthenticationService = container.get(
        SERVICE_IDENTIFIER.AuthenticationService
    );
    private fullName: string = "";
    private medicationStatementHistory: MedicationStatementHistory[] = [];
    private isLoading: boolean = false;
    private protectiveWordAttempts: number = 0;
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private isDataLoaded: boolean = false;
    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;
    @Ref("protectiveWordModal")
    readonly protectiveWordModal!: ProtectiveWordComponent;

    @Ref("pdfGenerator")
    readonly pdfGenerator!: any;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("getMedicationStatements", { namespace: "medication" })
    getMedicationStatements!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<RequestResult<MedicationStatementHistory[]>>;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    private showConfirmationModal() {
        this.messageModal.showModal();
    }
    private generateMedicationHistoryPdf() {
        this.logger.debug("generating Medication History PDF...");
        this.isLoading = true;

        html2pdf(this.$refs.document, {
            margin: 10,
            filename: "my.pdf",
            image: { type: "jpeg", quality: 1 },
            html2canvas: { dpi: 72, letterRendering: true },
            jsPDF: { unit: "mm", format: "a4", orientation: "landscape" },
            pdfCallback: this.pdfCallback,
        });
    }

    private pdfCallback(pdfObject) {
        var number_of_pages = pdfObject.internal.getNumberOfPages();
        var pdf_pages = pdfObject.internal.pages;
        var myFooter = "Footer info";
        for (var i = 1; i < pdf_pages.length; i++) {
            // We are telling our pdfObject that we are now working on this page
            pdfObject.setPage(i);
            // The 10,200 value is only for A4 landscape. You need to define your own for other page sizes
            pdfObject.text(myFooter, 10, 200);
        }
    }

    private mounted() {
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
        this.medicationStatementHistory.sort((a, b) =>
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
        this.authenticationService.getOidcUserProfile().then((oidcUser) => {
            if (oidcUser) {
                this.fullName =
                    oidcUser.given_name + " " + oidcUser.family_name;
            }
        });
    }
}
</script>
