<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder, LaboratoryUtil } from "@/models/laboratory";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

interface LaboratoryRow {
    date: string;
    test_type: string;
    test_location: string;
    result: string;
}

@Component({
    components: {
        ReportHeaderComponent,
    },
})
export default class COVID19ReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Action("retrieve", { namespace: "laboratory" })
    retrieveLaboratory!: (params: { hdid: string }) => Promise<void>;

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: LaboratoryOrder[];

    @Getter("isLoading", { namespace: "laboratory" })
    isLaboratoryLoading!: boolean;

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private isPreview = true;

    private readonly headerClass = "laboratory-report-table-header";

    @Watch("isLaboratoryLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLaboratoryLoading;
    }

    private get visibleRecords(): LaboratoryOrder[] {
        let records = this.laboratoryOrders.filter((record) => {
            return this.filter.allowsDate(
                record.labResults[0].collectedDateTime
            );
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(
                a.labResults[0].collectedDateTime
            );
            const secondDate = new DateWrapper(
                b.labResults[0].collectedDateTime
            );

            return firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;
        });

        return records;
    }

    private get isEmpty() {
        return this.visibleRecords.length == 0;
    }

    private get items(): LaboratoryRow[] {
        return this.visibleRecords.map<LaboratoryRow>((x) => {
            const labResult = x.labResults[0];
            return {
                date: DateWrapper.format(labResult.collectedDateTime),
                test_type: labResult.testType || "",
                test_location: x.location || "",
                result: this.checkResultReady(labResult.testStatus)
                    ? labResult.labResultOutcome || ""
                    : "",
            };
        });
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveLaboratory({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading Covid19 data: ${err}`);
        });
    }

    private checkResultReady(testStatus: string | null): boolean {
        return LaboratoryUtil.isTestResultReady(testStatus);
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating COVID-19 PDF...");
        this.isPreview = false;

        return PDFUtil.generatePdf(
            "HealthGateway_COVID19.pdf",
            this.report
        ).then(() => {
            this.isPreview = true;
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19ItemDate" },
        },
        {
            key: "test_type",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19ItemTestType" },
        },
        {
            key: "test_location",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19ItemLocation" },
        },
        {
            key: "result",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19ItemResult" },
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
                    title="Health Gateway COVID-19 Test Result History"
                />
                <b-row v-if="isEmpty && (!isLaboratoryLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-table
                    v-if="!isEmpty || isLaboratoryLoading"
                    striped
                    :busy="isLaboratoryLoading"
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
.laboratory-report-table-header {
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
