<script lang="ts">
import { saveAs } from "file-saver";
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import PatientData from "@/models/patientData";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IReportService } from "@/services/interfaces";

import { ReportType, TemplateType } from "../../models/reportRequest";

interface MedicationRow {
    date: string;
    din_pin: string;
    brand: string;
    generic: string;
    practitioner: string;
    quantity: string;
    strength: string;
    form: string;
    manufacturer: string;
}

@Component({
    components: {
        ProtectiveWordComponent,
    },
})
export default class MedicationHistoryReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Getter("patientData", { namespace: "user" })
    patientData!: PatientData;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Action("retrieveMedicationStatements", { namespace: "medication" })
    private retrieveMedications!: (params: { hdid: string }) => Promise<void>;

    @Getter("isMedicationStatementLoading", { namespace: "medication" })
    isLoading!: boolean;

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    private logger!: ILogger;
    private notFoundText = "Not Found";

    private readonly headerClass = "medication-report-table-header";

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

        return records;
    }

    private get items(): MedicationRow[] {
        return this.visibleRecords.map<MedicationRow>((x) => {
            return {
                date: DateWrapper.format(x.dispensedDate),
                din_pin: x.medicationSummary.din,
                brand: x.medicationSummary.brandName,
                generic: x.medicationSummary.genericName || this.notFoundText,
                practitioner: x.practitionerSurname || "",
                quantity:
                    x.medicationSummary.quantity === undefined
                        ? ""
                        : x.medicationSummary.quantity.toString(),
                strength:
                    (x.medicationSummary.strength || "") +
                        (x.medicationSummary.strengthUnit || "") ||
                    this.notFoundText,
                form: x.medicationSummary.form || this.notFoundText,
                manufacturer:
                    x.medicationSummary.manufacturer || this.notFoundText,
            };
        });
    }

    private get filterText(): string {
        if (!this.filter.hasDateFilter()) {
            return "";
        }

        const start = this.filter.startDate
            ? ` since ${this.formatDate(this.filter.startDate)}`
            : "";
        const end = this.filter.endDate
            ? this.formatDate(this.filter.endDate)
            : this.formatDate(new DateWrapper().toISO());
        return `Displaying records${start} until ${end}`;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveMedications({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication data: ${err}`);
        });
    }

    public async generatePdf(): Promise<void> {
        const reportService: IReportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );

        return reportService
            .generateReport({
                data: {
                    patient: {
                        phn: this.patientData.personalhealthnumber,
                        dateOfBirth: this.formatDate(
                            this.patientData.birthdate || ""
                        ),
                        name: this.patientData
                            ? this.patientData.firstname +
                              " " +
                              this.patientData.lastname
                            : "",
                    },
                    records: this.items,
                    isRedacted: this.filter.hasMedicationsFilter(),
                    datePrinted: this.formatDate(new DateWrapper().toISO()),
                    filterText: this.filterText,
                },
                template: TemplateType.Medication,
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

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
        },
        {
            key: "din_pin",
            label: "DIN/PIN",
            thClass: this.headerClass,
        },
        {
            key: "brand",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "medicationReportBrandNameItem" },
        },
        {
            key: "generic",
            thClass: this.headerClass,
        },
        {
            key: "practitioner",
            thClass: this.headerClass,
        },
        {
            key: "quantity",
            thClass: this.headerClass,
        },
        {
            key: "strength",
            thClass: this.headerClass,
        },
        {
            key: "form",
            thClass: this.headerClass,
        },
        {
            key: "manufacturer",
            thClass: this.headerClass,
        },
    ];
}
</script>

<template>
    <div>
        <div>
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
        <ProtectiveWordComponent
            ref="protectiveWordModal"
            :is-loading="isLoading"
        />
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.medication-report-table-header {
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
