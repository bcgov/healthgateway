<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { Action, Getter } from "vuex-class";
import ImmunizationModel from "@/models/immunizationModel";
import { DateWrapper } from "@/models/dateWrapper";
import { ILogger } from "@/services/interfaces";
import BannerError from "@/models/bannerError";
import ErrorTranslator from "@/utility/errorTranslator";
import html2pdf from "html2pdf.js";
import PDFDefinition from "@/plugins/pdfDefinition";
import ReportHeaderComponent from "@/components/report/header.vue";
import User from "@/models/user";

@Component({
    components: {
        ReportHeaderComponent,
    },
})
export default class ImmunizationHistoryReportComponent extends Vue {
    @Prop() private startDate?: string;
    @Prop() private endDate?: string;
    @Getter("user", { namespace: "user" })
    private user!: User;
    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;
    @Getter("getStoredImmunizations", { namespace: "immunization" })
    patientImmunizations!: ImmunizationModel[];
    @Action("addError", { namespace: "errorBanner" })
    private addError!: (error: BannerError) => void;
    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: {
        hdid: string;
    }) => Promise<ImmunizationModel[]>;
    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private immunizationRecords: ImmunizationModel[] = [];
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
        <div ref="report">
            <section class="pdf-item">
                <div v-show="!isPreview">
                    <ReportHeaderComponent
                        :start-date="startDate"
                        :end-date="endDate"
                        title="Health Gateway Immunization History"
                    />
                    <hr />
                    <b-row>
                        <b-col>
                            <h4>Immunization History</h4>
                        </b-col>
                    </b-row>
                </div>
                <b-row
                    v-if="isEmpty && (!isLoading || !isPreview)"
                    class="mt-2"
                >
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 mt-4 header">
                    <b-col data-testid="immunizationDateTitle" class="col-1"
                        >Date</b-col
                    >
                    <b-col data-testid="immunizationProviderTitle" class="col-2"
                        >Provider / Clinic</b-col
                    >
                    <b-col data-testid="immunizationItemTitle" class="col-2"
                        >Immunization</b-col
                    >
                    <b-col data-testid="immunizationAgentTitle" class="col-5">
                        <b-row>
                            <b-col>Agent</b-col>
                            <b-col>Product</b-col>
                            <b-col>Lot Number</b-col>
                        </b-row>
                    </b-col>
                    <b-col data-testid="immunizationStatusTitle" class="col-2"
                        >Status</b-col
                    >
                </b-row>
                <b-row
                    v-for="immzRecord in immunizationRecords"
                    :key="immzRecord.id"
                    class="item py-1"
                >
                    <b-col
                        data-testid="immunizationItemDate"
                        class="col-1 text-nowrap"
                    >
                        {{ formatDate(immzRecord.dateOfImmunization) }}
                    </b-col>
                    <b-col
                        data-testid="immunizationItemProviderClinic"
                        class="col-2"
                    >
                        {{ immzRecord.providerOrClinic }}
                    </b-col>
                    <b-col data-testid="immunizationItemName" class="col-2">
                        {{ immzRecord.name }}
                    </b-col>
                    <b-col data-testid="immunizationItemAgent" class="col-5">
                        <b-row
                            v-for="agent in immzRecord.immunizationAgents"
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
                    <b-col data-testid="immunizationItemStatus" class="col-2">
                        {{ immzRecord.status }}
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
    background-color: $medium_background;
}
.item:nth-child(even) {
    background-color: $soft_background;
}
</style>
