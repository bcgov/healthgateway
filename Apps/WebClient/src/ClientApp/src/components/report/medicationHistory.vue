<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

@Component({
    components: {
        ProtectiveWordComponent,
        ReportHeaderComponent,
    },
})
export default class MedicationHistoryReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Action("retrieveMedicationStatements", { namespace: "medication" })
    private retrieveMedications!: (params: { hdid: string }) => Promise<void>;

    @Getter("isMedicationStatementLoading", { namespace: "medication" })
    isLoading!: boolean;

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private notFoundText = "Not Found";
    private fileMaxRecords = 1000;
    private fileIndex = 0;
    private totalFiles = 1;
    private isPreview = true;

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private get isEmpty() {
        return this.visibleRecords.length === 0;
    }

    private get visibleRecords(): MedicationStatementHistory[] {
        let records = this.medicationStatements.filter((record) => {
            return (
                this.filter.allowsDate(record.dispensedDate) &&
                this.filter.allowsMedication(record.medicationSummary.brandName)
            );
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.dispensedDate);
            const secondDate = new DateWrapper(b.dispensedDate);

            return firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;
        });

        if (this.isPreview) {
            return records;
        } else {
            // Breaks records into chunks for multiple files.
            return records.slice(
                this.fileIndex * this.fileMaxRecords,
                (this.fileIndex + 1) * this.fileMaxRecords
            );
        }
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveMedications({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication data: ${err}`);
        });
    }

    private formatDate(date: string): string {
        return DateWrapper.format(date);
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Medication History PDF...");
        this.totalFiles = Math.ceil(
            this.visibleRecords.length / this.fileMaxRecords
        );
        this.isPreview = false;

        this.generatePdfFile().then(() => {
            if (this.fileIndex + 1 < this.totalFiles) {
                this.fileIndex++;
                this.generatePdfFile();
            } else {
                this.isPreview = true;
                this.fileIndex = 0;
            }
        });
    }

    private generatePdfFile(): Promise<void> {
        return PDFUtil.generatePdf(
            `HealthGateway_MedicationHistory_File${this.fileIndex + 1}.pdf`,
            this.report,
            `File ${this.fileIndex + 1} of ${this.totalFiles}`
        );
    }
}
</script>

<template>
    <div>
        <div ref="report">
            <section class="pdf-item">
                <ReportHeaderComponent
                    v-show="!isPreview"
                    :filter="filter"
                    :title="
                        'Health Gateway Medication History' +
                        (filter.hasMedicationsFilter() ? ' (Redacted)' : '')
                    "
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
                    v-for="item in visibleRecords"
                    :key="item.prescriptionIdentifier + item.dispensedDate"
                    class="item py-1"
                    data-testid="medicationReportEntry"
                >
                    <b-col class="col-1 my-auto text-nowrap">
                        {{ formatDate(item.dispensedDate) }}
                    </b-col>
                    <b-col class="col-1 my-auto">
                        {{ item.medicationSummary.din }}
                    </b-col>
                    <b-col
                        class="col-2 my-auto"
                        data-testid="medicationReportEntryBrandName"
                    >
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
