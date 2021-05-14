<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

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

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveMedications({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading medication data: ${err}`);
        });
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Medication History PDF...");
        this.totalFiles = Math.ceil(
            this.visibleRecords.length / this.fileMaxRecords
        );
        this.isPreview = false;

        return this.generatePdfFile().then(() => {
            if (this.fileIndex + 1 < this.totalFiles) {
                this.fileIndex++;
                return this.generatePdfFile();
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
    color: $primary;
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
