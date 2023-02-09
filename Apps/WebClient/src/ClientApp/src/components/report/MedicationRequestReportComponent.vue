<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequest from "@/models/medicationRequest";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

interface MedicationRequestRow {
    date: string;
    requested_drug_name: string;
    status: string;
    prescriber_name: string;
    effective_date: string;
    expiry_date: string;
    reference_number: string;
}

@Component
export default class MedicationRequestReportComponent extends Vue {
    @Prop()
    filter!: ReportFilter;

    @Action("retrieveSpecialAuthorityRequests", { namespace: "medication" })
    retrieveSpecialAuthorityRequests!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Getter("specialAuthorityRequestsAreLoading", { namespace: "medication" })
    specialAuthorityRequestsAreLoading!: (hdid: string) => boolean;

    @Getter("specialAuthorityRequests", { namespace: "medication" })
    specialAuthorityRequests!: (hdid: string) => MedicationRequest[];

    @Getter("user", { namespace: "user" })
    user!: User;

    private logger!: ILogger;

    private readonly headerClass = "medication-request-report-table-header";

    private get isLoading(): boolean {
        return this.specialAuthorityRequestsAreLoading(this.user.hdid);
    }

    private get isEmpty(): boolean {
        return this.visibleRecords.length === 0;
    }

    private get visibleRecords(): MedicationRequest[] {
        let records = this.specialAuthorityRequests(this.user.hdid).filter(
            (record) => this.filter.allowsDate(record.requestedDate)
        );
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.requestedDate);
            const secondDate = new DateWrapper(b.requestedDate);

            if (firstDate.isBefore(secondDate)) {
                return 1;
            }

            if (firstDate.isAfter(secondDate)) {
                return -1;
            }

            return 0;
        });

        return records;
    }

    private get items(): MedicationRequestRow[] {
        return this.visibleRecords.map<MedicationRequestRow>((x) => ({
            date: DateWrapper.format(x.requestedDate),
            requested_drug_name: x.drugName || "",
            status: x.requestStatus || "",
            prescriber_name: this.prescriberName(x),
            effective_date:
                x.effectiveDate === undefined
                    ? ""
                    : DateWrapper.format(x.effectiveDate),
            expiry_date:
                x.expiryDate === undefined
                    ? ""
                    : DateWrapper.format(x.expiryDate),
            reference_number: x.referenceNumber,
        }));
    }

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged(): boolean {
        return this.isLoading;
    }

    @Watch("isEmpty")
    @Emit()
    private onIsEmptyChanged(): boolean {
        return this.isEmpty;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveSpecialAuthorityRequests({ hdid: this.user.hdid }).catch(
            (err) =>
                this.logger.error(
                    `Error loading Special Authority requests data: ${err}`
                )
        );
    }

    private mounted(): void {
        this.onIsEmptyChanged();
    }

    private prescriberName(medication: MedicationRequest): string {
        return (
            (medication.prescriberFirstName || " ") +
            " " +
            (medication.prescriberLastName || " ")
        );
    }

    public async generateReport(
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ): Promise<RequestResult<Report>> {
        const reportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );

        return reportService.generateReport({
            data: {
                header: headerData,
                records: this.items,
            },
            template: TemplateType.MedicationRequest,
            type: reportFormatType,
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            thStyle: { width: "10%" },
            tdAttr: { "data-testid": "medicationRequestDateItem" },
        },
        {
            key: "requested_drug_name",
            thClass: this.headerClass,
            thStyle: { width: "20%" },
        },
        {
            key: "status",
            thClass: this.headerClass,
            thStyle: { width: "9%" },
        },
        {
            key: "prescriber_name",
            thClass: this.headerClass,
            thStyle: { width: "15%" },
        },
        {
            key: "effective_date",
            thClass: this.headerClass,
            thStyle: { width: "15%" },
        },
        {
            key: "expiry_date",
            thClass: this.headerClass,
            thStyle: { width: "15%" },
        },
        {
            key: "reference_number",
            thClass: this.headerClass,
            thStyle: { width: "16%" },
        },
    ];
}
</script>

<template>
    <div>
        <section>
            <b-row v-if="isEmpty && !isLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-if="!isEmpty || isLoading"
                :striped="true"
                :fixed="true"
                :busy="isLoading"
                :items="items"
                :fields="fields"
                class="table-style"
            >
                <template #table-busy>
                    <content-placeholders>
                        <content-placeholders-text :lines="7" />
                    </content-placeholders>
                </template>
            </b-table>
        </section>
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.medication-request-report-table-header {
    color: $heading_color;
    font-size: 0.8rem;
}
</style>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.table-style {
    font-size: 0.6rem;
    text-align: center;
}
</style>
