<script lang="ts">
import Vue from "vue";
import { Component, Prop, Ref } from "vue-property-decorator";
import moment from "moment";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ResultType } from "@/constants/resulttype";
import { Action, Getter } from "vuex-class";
import ImmunizationModel from "@/models/immunizationModel";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import RequestResult from "@/models/requestResult";
import PatientData from "@/models/patientData";
import User from "@/models/user";
import { ILogger, IImmunizationService } from "@/services/interfaces";
import BannerError from "@/models/bannerError";
import ErrorTranslator from "@/utility/errorTranslator";
import LoadingComponent from "@/components/loading.vue";
import html2pdf from "html2pdf.js";
import PDFDefinition from "@/plugins/pdfDefinition";

const userNamespace = "user";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class ImmunizationHistoryReportComponent extends Vue {
    @Prop() private name!: string;
    @Getter("user", { namespace: userNamespace })
    private user!: User;
    @Action("addError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;
    @Action("getPatientData", { namespace: userNamespace })
    getPatientData!: (params: {
        hdid: string;
    }) => Promise<RequestResult<PatientData>>;
    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private isLoading = false;
    private notFoundText = "Not Found";
    private immunizationRecords: ImmunizationModel[] = [];
    private isPreview = true;
    private phn = "";
    private dateOfBirth = "";

    private fetchPatientData() {
        this.getPatientData({
            hdid: this.user.hdid,
        })
            .then((result) => {
                // Load patient data
                if (result) {
                    this.phn = result.resourcePayload.personalhealthnumber;
                    if (result.resourcePayload.birthdate != null) {
                        this.dateOfBirth = this.formatStringISODate(
                            result.resourcePayload.birthdate
                        );
                    }
                }
            })
            .catch((err) => {
                this.logger.error(`Error fetching Patient Data: ${err}`);
                this.addError(
                    ErrorTranslator.toBannerError("Patient Data loading", err)
                );
                this.isLoading = false;
            });
    }

    private fetchPatientImmunizations() {
        const immunizationService: IImmunizationService = container.get(
            SERVICE_IDENTIFIER.ImmunizationService
        );
        this.isLoading = true;
        immunizationService
            .getPatientImmunizations(this.user.hdid)
            .then((results) => {
                if (results.resultStatus == ResultType.Success) {
                    this.immunizationRecords = results.resourcePayload;
                    this.sortEntries();
                    this.fetchPatientData();
                } else {
                    this.logger.error(
                        "Error returned from the Patient Immunizations call: " +
                            JSON.stringify(results.resultError)
                    );
                    this.addError(
                        ErrorTranslator.toBannerError(
                            "Fetch Patient Immunizations Error",
                            results.resultError
                        )
                    );
                }
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError(
                        "Fetch Patient Immunizations Error",
                        err
                    )
                );
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private sortEntries() {
        this.immunizationRecords.sort((a, b) =>
            a.dateOfImmunization > b.dateOfImmunization
                ? -1
                : a.dateOfImmunization < b.dateOfImmunization
                ? 1
                : 0
        );
    }

    private formatStringISODate(date: StringISODate): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    private get currentDate() {
        return moment(new Date()).format("ll");
    }
    private get isEmpty() {
        return this.immunizationRecords.length == 0;
    }

    private formatDate(date: Date): string {
        return moment(date).format("yyyy-MM-DD");
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchPatientImmunizations();
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Immunization History PDF...");
        this.isLoading = true;
        this.isPreview = false;

        let opt = {
            margin: [25, 15],
            filename: `HealthGateway_ImmunizationHistory.pdf`,
            image: { type: "jpeg", quality: 0.98 },
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
                        this.isPreview = true;
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
                <div v-show="!isPreview">
                    <div id="pageTitle">
                        <h3 id="subject">
                            Health Gateway Immunization History
                        </h3>
                    </div>
                    <div id="disclaimer" class="mr-1">
                        <span>Disclaimer:</span> This record was generated by
                        Provincial Health Gateway application. For any
                        questions, please contact HealthGateway@gov.bc.ca
                    </div>
                    <hr />
                    <b-row align-h="end">
                        <b-col class="datePrintedCol" cols="4">
                            <label>Date Printed:</label>&nbsp;
                            <span>{{ currentDate }}&nbsp;</span>
                        </b-col>
                    </b-row>
                    <b-row class="pt-2">
                        <b-col>
                            <label>Name:&nbsp;</label> <span>{{ name }}</span>
                        </b-col>
                        <b-col>
                            <label>PHN:</label>
                            <span>&nbsp;{{ phn }}</span>
                        </b-col>
                        <b-col>
                            <label>Date of Birth:</label>
                            <span>&nbsp;{{ dateOfBirth }}</span>
                        </b-col>
                    </b-row>
                    <hr />
                    <b-row>
                        <b-col>
                            <h4>Immunization History</h4>
                        </b-col>
                    </b-row>
                </div>

                <b-row v-if="isEmpty" class="mt-2">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else class="py-3 mt-4 header">
                    <b-col class="col">Date</b-col>
                    <b-col class="col">Provider/Clinic</b-col>
                    <b-col class="col">Immunization</b-col>
                    <b-col class="col">Product</b-col>
                    <b-col class="col">Lot Number</b-col>
                </b-row>
                <b-row
                    v-for="item in immunizationRecords"
                    :key="item.id"
                    class="item py-1"
                >
                    <b-col
                        data-testid="immunizationItemDate"
                        class="col my-auto text-nowrap"
                    >
                        {{ formatDate(item.dateOfImmunization) }}
                    </b-col>
                    <b-col
                        data-testid="immunizationItemProviderClinic"
                        class="col my-auto"
                    >
                        {{ "pending" }}
                    </b-col>
                    <b-col
                        data-testid="immunizationItemName"
                        class="col my-auto"
                    >
                        {{ item.name }}
                    </b-col>
                    <b-col
                        data-testid="immunizationItemProduct"
                        class="col my-auto"
                    >
                        {{ "pending" }}
                    </b-col>
                    <b-col
                        data-testid="immunizationItemLotNumber"
                        class="col my-auto"
                    >
                        {{ "pending" }}
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
    span {
        font-weight: bold !important;
    }
}

.datePrintedCol {
    label,
    span {
        color: #808080 !important;
    }
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
    color: $primary;
    background-color: $soft_background;
    font-weight: bold;
    font-size: 0.8em;
    text-align: center;
}

h4 {
    color: $primary;
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
