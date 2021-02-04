<script lang="ts">
import html2pdf from "html2pdf.js";
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import BannerError from "@/models/bannerError";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import PatientData from "@/models/patientData";
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
export default class ImmunizationHistoryReportComponent extends Vue {
    @Prop() private startDate?: string;
    @Prop() private endDate?: string;
    @Prop() private patientData?: PatientData;
    @Getter("user", { namespace: "user" })
    private user!: User;
    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;
    @Getter("getStoredImmunizations", { namespace: "immunization" })
    patientImmunizations!: ImmunizationEvent[];
    @Getter("getStoredRecommendations", { namespace: "immunization" })
    patientRecommendations!: Recommendation[];
    @Action("addError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;
    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: {
        hdid: string;
    }) => Promise<ImmunizationEvent[]>;
    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private immunizationRecords: ImmunizationEvent[] = [];
    private recommendationRecords: Recommendation[] = [];
    private isPreview = true;
    private isLoading = false;

    @Watch("immunizationIsDeferred")
    private onImmunizationIsDeferredChanged(newVal: boolean) {
        if (newVal) {
            this.isLoading = true;
        } else {
            this.loadRecords();
        }
    }

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    @Watch("startDate")
    @Watch("endDate")
    private onDateChanged() {
        this.fetchPatientImmunizations();
    }

    private loadRecords() {
        this.immunizationRecords = this.patientImmunizations;
        this.recommendationRecords = this.patientRecommendations;
        this.filterAndSortEntries();
        this.isLoading = false;
    }

    private fetchPatientImmunizations() {
        this.isLoading = true;
        this.retrieveImmunizations({ hdid: this.user.hdid })
            .then(() => {
                if (!this.immunizationIsDeferred) {
                    this.loadRecords();
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
                this.isLoading = false;
            });
    }

    private filterAndSortEntries() {
        this.immunizationRecords = this.immunizationRecords.filter((record) => {
            return (
                (!this.startDate ||
                    new DateWrapper(record.dateOfImmunization).isAfterOrSame(
                        new DateWrapper(this.startDate)
                    )) &&
                (!this.endDate ||
                    new DateWrapper(record.dateOfImmunization).isBeforeOrSame(
                        new DateWrapper(this.endDate)
                    ))
            );
        });
        this.immunizationRecords.sort((a, b) =>
            a.dateOfImmunization > b.dateOfImmunization
                ? -1
                : a.dateOfImmunization < b.dateOfImmunization
                ? 1
                : 0
        );
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format("yyyy-MM-dd");
    }

    private get isEmpty() {
        return this.immunizationRecords.length == 0;
    }

    private get isRecommendationEmpty() {
        return this.recommendationRecords.length == 0;
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchPatientImmunizations();
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Immunization History PDF...");
        this.isPreview = false;

        let opt = {
            margin: [25, 15],
            filename: `HealthGateway_ImmunizationHistory.pdf`,
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
        <div v-show="!isLoading" ref="report">
            <section class="pdf-item">
                <div>
                    <ReportHeaderComponent
                        v-show="!isPreview"
                        :start-date="startDate"
                        :end-date="endDate"
                        title="Health Gateway Immunization Record"
                        :patient-data="patientData"
                    />
                    <hr />
                </div>
                <b-row>
                    <b-col>
                        <h4>Immunization History</h4>
                    </b-col>
                </b-row>
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col data-testid="immunizationDateTitle" class="col-2"
                        >Date</b-col
                    >
                    <b-col data-testid="immunizationItemTitle" class="col-3"
                        >Immunization</b-col
                    >
                    <b-col data-testid="immunizationAgentTitle" class="col-5">
                        <b-row>
                            <b-col>Agent</b-col>
                            <b-col>Product</b-col>
                            <b-col>Lot Number</b-col>
                        </b-row>
                    </b-col>
                    <b-col data-testid="immunizationProviderTitle" class="col-2"
                        >Provider / Clinic</b-col
                    >
                </b-row>
                <b-row
                    v-for="immzRecord in immunizationRecords"
                    :key="immzRecord.id"
                    class="item py-1"
                >
                    <b-col
                        data-testid="immunizationItemDate"
                        class="col-2 text-nowrap"
                    >
                        {{ formatDate(immzRecord.dateOfImmunization) }}
                    </b-col>
                    <b-col data-testid="immunizationItemName" class="col-3">
                        {{ immzRecord.immunization.name }}
                    </b-col>
                    <b-col data-testid="immunizationItemAgent" class="col-5">
                        <b-row
                            v-for="agent in immzRecord.immunization
                                .immunizationAgents"
                            :key="agent.code"
                        >
                            <b-col>
                                {{ agent.name }}
                            </b-col>
                            <b-col>
                                {{ agent.productName }}
                            </b-col>
                            <b-col>
                                {{ agent.lotNumber }}
                            </b-col>
                        </b-row>
                    </b-col>
                    <b-col
                        data-testid="immunizationItemProviderClinic"
                        class="col-2"
                    >
                        {{ immzRecord.providerOrClinic }}
                    </b-col>
                </b-row>
                <b-row>
                    <b-col class="col-7">
                        <b-row class="mt-3">
                            <b-col>
                                <h4>Recommended Immunizations</h4>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col>
                                <div id="disclaimer">
                                    DISCLAIMER: Provincial Immunization Registry
                                    record only. Immunization history displayed
                                    may not portray the client’s complete
                                    immunization history and may impact
                                    forecasted vaccines. For information on
                                    recommended immunizations, please visit
                                    <a>https://www.immunizebc.ca</a> or contact
                                    your local Public Health Unit.
                                </div>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="
                                isRecommendationEmpty &&
                                (!isLoading || !isPreview)
                            "
                            class="mt-2"
                        >
                            <b-col>No recommendations found.</b-col>
                        </b-row>
                        <b-row
                            v-else-if="!isRecommendationEmpty"
                            class="py-3 mt-4 header"
                        >
                            <b-col
                                data-testid="recommendationTitle"
                                class="col-6"
                                >Immunization</b-col
                            >
                            <b-col
                                data-testid="recommendationDateTitle"
                                class="col-3"
                                >Date</b-col
                            >
                            <b-col
                                data-testid="recommendationStatusTitle"
                                class="col-3"
                                >Status</b-col
                            >
                        </b-row>
                        <b-row
                            v-for="recommendation in recommendationRecords"
                            :key="recommendation.recommendationId"
                            class="item py-1"
                        >
                            <b-col
                                data-testid="recommendation"
                                class="col-6 text-nowrap"
                            >
                                {{ recommendation.immunization.name }}
                            </b-col>
                            <b-col
                                data-testid="recommendationDate"
                                class="col-3"
                            >
                                {{ formatDate(recommendation.agentDueDate) }}
                            </b-col>
                            <b-col
                                data-testid="recommendationStatus"
                                class="col-3"
                            >
                                {{ recommendation.status }}
                            </b-col>
                        </b-row>
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
    background-color: $soft_background;
}
.item:nth-child(even) {
    background-color: $medium_background;
}

#disclaimer {
    font-size: 0.7em;
    font-weight: bold !important;
}
</style>
