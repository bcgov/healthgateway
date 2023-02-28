<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder } from "@/models/laboratory";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

interface LabTestRow {
    date: string;
    test: string;
    result: string;
    status: string;
}

@Component
export default class LaboratoryReportComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop() private filter!: ReportFilter;

    @Action("retrieveLaboratoryOrders", { namespace: "laboratory" })
    retrieveLaboratoryOrders!: (params: { hdid: string }) => Promise<void>;

    @Getter("laboratoryOrders", { namespace: "laboratory" })
    laboratoryOrders!: (hdid: string) => LaboratoryOrder[];

    @Getter("laboratoryOrdersAreLoading", { namespace: "laboratory" })
    laboratoryOrdersAreLoading!: (hdid: string) => boolean;

    private logger!: ILogger;

    private readonly headerClass = "laboratory-test-report-table-header";

    private get isLaboratoryLoading(): boolean {
        return this.laboratoryOrdersAreLoading(this.hdid);
    }

    private get visibleRecords(): LaboratoryOrder[] {
        let records = this.laboratoryOrders(this.hdid).filter((record) =>
            this.filter.allowsDate(record.timelineDateTime)
        );
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.timelineDateTime);
            const secondDate = new DateWrapper(b.timelineDateTime);

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

    private get isEmpty(): boolean {
        return this.visibleRecords.length === 0;
    }

    private get items(): LabTestRow[] {
        return this.visibleRecords.flatMap<LabTestRow>((x) => {
            const timelineDateTime = DateWrapper.format(x.timelineDateTime);
            return x.laboratoryTests.map<LabTestRow>((y) => ({
                date: timelineDateTime,
                test: y.batteryType || "",
                result: y.result,
                status: y.testStatus,
            }));
        });
    }

    @Watch("isLaboratoryLoading")
    @Emit()
    private onIsLoadingChanged(): boolean {
        return this.isLaboratoryLoading;
    }

    @Watch("isEmpty")
    @Emit()
    private onIsEmptyChanged(): boolean {
        return this.isEmpty;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveLaboratoryOrders({ hdid: this.hdid }).catch((err) =>
            this.logger.error(`Error loading Laboratory data: ${err}`)
        );
    }

    private mounted(): void {
        this.onIsEmptyChanged();
    }

    public generateReport(
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
            template: TemplateType.Laboratory,
            type: reportFormatType,
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "labResultDateItem" },
        },
        {
            key: "test",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "labResultTestTypeItem" },
        },
        {
            key: "result",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "labResultItem" },
        },
        {
            key: "status",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "labStatusItem" },
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

.laboratory-test-report-table-header {
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
