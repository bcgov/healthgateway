<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequest from "@/models/MedicationRequest";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

@Component({
    components: {
        ReportHeaderComponent,
    },
})
export default class MedicationRequestReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Action("retrieveMedicationRequests", { namespace: "medication" })
    private retrieve!: (params: { hdid: string }) => Promise<void>;

    @Getter("isMedicationRequestLoading", { namespace: "medication" })
    isLoading!: boolean;

    @Getter("medicationRequests", { namespace: "medication" })
    medicationRequests!: MedicationRequest[];

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private isPreview = true;

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private get isEmpty() {
        return this.visibleRecords.length === 0;
    }

    private get visibleRecords(): MedicationRequest[] {
        let records = this.medicationRequests.filter((record) => {
            return this.filter.allowsDate(record.requestedDate);
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.requestedDate);
            const secondDate = new DateWrapper(b.requestedDate);

            const value = firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;

            return value;
        });

        return records;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieve({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication requests data: ${err}`);
        });
    }

    private formatDate(date: string | undefined): string {
        if (date === undefined) {
            return "";
        } else {
            return DateWrapper.format(date);
        }
    }

    private prescriberName(medication: MedicationRequest) {
        return (
            (medication.prescriberFirstName || " ") +
            " " +
            (medication.prescriberLastName || " ")
        );
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Medication Request History PDF...");
        this.isPreview = false;

        PDFUtil.generatePdf(
            "HealthGateway_SpecialAuthorityRequestHistory.pdf",
            this.report
        ).then(() => {
            this.isPreview = true;
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
                    :filter="filter"
                    title="Health Gateway Special Authority Request History "
                />
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col class="col-1">Date</b-col>
                    <b-col class="col-2">Requested Drug Name</b-col>
                    <b-col class="col-1">Status</b-col>
                    <b-col class="col-2">Prescriber Name</b-col>
                    <b-col class="col-2">Effective Date</b-col>
                    <b-col class="col-2">Expiry Date</b-col>
                    <b-col class="col-2">Reference Number</b-col>
                </b-row>
                <b-row
                    v-for="item in visibleRecords"
                    :key="item.referenceNumber"
                    class="item py-1"
                >
                    <b-col class="col-1 my-auto text-nowrap">
                        {{ formatDate(item.requestedDate) }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ item.drugName || "" }}
                    </b-col>
                    <b-col class="col-1 my-auto">
                        {{ item.requestStatus || "" }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ prescriberName(item) }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ formatDate(item.effectiveDate) }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ formatDate(item.expiryDate) }}
                    </b-col>
                    <b-col class="col-2 my-auto">
                        {{ item.referenceNumber }}
                    </b-col>
                </b-row>
            </section>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

div[class^="col"],
div[class*=" col"] {
    padding: 0px;
    margin: 0px;
}

div[class^="row"],
div[class*=" row"] {
    padding: 0px;
    margin: 0px;
}

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
