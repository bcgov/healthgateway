<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import MedicationRequest from "@/models/MedicationRequest";
import PatientData from "@/models/patientData";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportType, TemplateType } from "@/models/reportRequest";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
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
    @Prop() private filter!: ReportFilter;

    @Getter("patientData", { namespace: "user" })
    private patientData!: PatientData;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Action("retrieveMedicationRequests", { namespace: "medication" })
    private retrieve!: (params: { hdid: string }) => Promise<void>;

    @Getter("isMedicationRequestLoading", { namespace: "medication" })
    isLoading!: boolean;

    @Getter("medicationRequests", { namespace: "medication" })
    medicationRequests!: MedicationRequest[];

    private logger!: ILogger;

    private readonly headerClass = "medication-request-report-table-header";

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

            return firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;
        });

        return records;
    }

    private get headerData(): ReportHeader {
        return {
            phn: this.patientData.personalhealthnumber,
            dateOfBirth: this.formatDate(this.patientData.birthdate || ""),
            name: this.patientData
                ? this.patientData.firstname + " " + this.patientData.lastname
                : "",
            datePrinted: this.formatDate(new DateWrapper().toISO()),
            filterText: this.filterText,
        };
    }

    private get filterText(): string {
        if (!this.filter.hasDateFilter()) {
            return "";
        }

        const start = this.filter.startDate
            ? ` from ${this.formatDate(this.filter.startDate)}`
            : "";
        const end = this.filter.endDate
            ? this.formatDate(this.filter.endDate)
            : this.formatDate(new DateWrapper().toISO());
        return `Displaying records${start} up to ${end}`;
    }

    private get items(): MedicationRequestRow[] {
        return this.visibleRecords.map<MedicationRequestRow>((x) => {
            return {
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
            };
        });
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieve({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication requests data: ${err}`);
        });
    }

    private prescriberName(medication: MedicationRequest) {
        return (
            (medication.prescriberFirstName || " ") +
            " " +
            (medication.prescriberLastName || " ")
        );
    }

    public async generatePdf(): Promise<void> {
        const reportService: IReportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );
        return reportService
            .generateReport({
                data: {
                    header: this.headerData,
                    records: this.items,
                },
                template: TemplateType.MedicationRequest,
                type: ReportType.PDF,
            })
            .then((result) => {
                const downloadLink = `data:application/pdf;base64,${result.resourcePayload.data}`;
                fetch(downloadLink).then((res) => {
                    res.blob().then((blob) => {
                        saveAs(blob, result.resourcePayload.fileName);
                    });
                });
            });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            thStyle: { width: "10%" },
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
        <div ref="report">
            <section class="pdf-item">
                <b-row v-if="isEmpty && !isLoading">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-table
                    v-if="!isEmpty || isLoading"
                    striped
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
