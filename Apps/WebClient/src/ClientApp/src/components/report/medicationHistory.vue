<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IReportService } from "@/services/interfaces";

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

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    @Watch("isEmpty")
    @Emit()
    private onIsEmptyChanged() {
        return this.isEmpty;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveMedications({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication data: ${err}`);
        });
    }

    private mounted() {
        this.onIsEmptyChanged();
    }

    public generateReport(
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ): Promise<RequestResult<Report>> {
        const reportService: IReportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );

        return reportService.generateReport({
            data: {
                header: headerData,
                records: this.items,
            },
            template: TemplateType.Medication,
            type: reportFormatType,
        });
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
