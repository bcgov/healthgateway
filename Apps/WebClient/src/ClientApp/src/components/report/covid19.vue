<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder, LaboratoryUtil } from "@/models/laboratory";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { TemplateType } from "@/models/reportRequest";
import { ReportFormatType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IReportService } from "@/services/interfaces";

interface LaboratoryRow {
    date: string;
    test_type: string;
    test_location: string;
    result: string;
}

@Component
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

    private logger!: ILogger;

    private readonly headerClass = "laboratory-report-table-header";

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

            return firstDate.isBefore(secondDate)
                ? 1
                : firstDate.isAfter(secondDate)
                ? -1
                : 0;
        });

        return records;
    }

    private get isEmpty() {
        return this.visibleRecords.length === 0;
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

    @Watch("isLaboratoryLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLaboratoryLoading;
    }

    @Watch("isEmpty")
    @Emit()
    private onIsEmptyChanged() {
        return this.isEmpty;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveLaboratory({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading Covid19 data: ${err}`);
        });
    }

    private mounted() {
        this.onIsEmptyChanged();
    }

    private checkResultReady(testStatus: string | null): boolean {
        return LaboratoryUtil.isTestResultReady(testStatus);
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
            template: TemplateType.COVID,
            type: reportFormatType,
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19DateItem" },
        },
        {
            key: "test_type",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19TestTypeItem" },
        },
        {
            key: "test_location",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19LocationItem" },
        },
        {
            key: "result",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "covid19ResultItem" },
        },
    ];
}
</script>

<template>
    <div>
        <section>
            <b-row v-if="isEmpty && !isLaboratoryLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-if="!isEmpty || isLaboratoryLoading"
                :striped="true"
                :fixed="true"
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
