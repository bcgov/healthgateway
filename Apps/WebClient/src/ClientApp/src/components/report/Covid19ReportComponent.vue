<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { Covid19LaboratoryOrder } from "@/models/laboratory";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

interface Covid19LaboratoryOrderRow {
    date: string;
    test_type: string;
    test_location: string;
    result: string;
}

@Component
export default class Covid19ReportComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop() private filter!: ReportFilter;

    @Action("retrieveCovid19LaboratoryOrders", { namespace: "laboratory" })
    retrieveCovid19LaboratoryOrders!: (params: {
        hdid: string;
    }) => Promise<void>;

    @Getter("covid19LaboratoryOrders", { namespace: "laboratory" })
    covid19LaboratoryOrders!: (hdid: string) => Covid19LaboratoryOrder[];

    @Getter("covid19LaboratoryOrdersAreLoading", { namespace: "laboratory" })
    covid19LaboratoryOrdersAreLoading!: (hdid: string) => boolean;

    private logger!: ILogger;

    private readonly headerClass = "covid19-laboratory-report-table-header";

    private get isCovid19LaboratoryLoading(): boolean {
        return this.covid19LaboratoryOrdersAreLoading(this.hdid);
    }

    private get visibleRecords(): Covid19LaboratoryOrder[] {
        let records = this.covid19LaboratoryOrders(this.hdid).filter((record) =>
            this.filter.allowsDate(record.labResults[0].collectedDateTime)
        );
        records.sort((a, b) => {
            const firstDate = new DateWrapper(
                a.labResults[0].collectedDateTime
            );
            const secondDate = new DateWrapper(
                b.labResults[0].collectedDateTime
            );

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

    private get items(): Covid19LaboratoryOrderRow[] {
        return this.visibleRecords.map<Covid19LaboratoryOrderRow>((x) => {
            const labResult = x.labResults[0];
            return {
                date: DateWrapper.format(labResult.collectedDateTime),
                test_type: labResult.testType,
                test_location: x.location || "",
                result: labResult.filteredLabResultOutcome,
            };
        });
    }

    @Watch("isCovid19LaboratoryLoading")
    @Emit()
    private onIsLoadingChanged(): boolean {
        return this.isCovid19LaboratoryLoading;
    }

    @Watch("isEmpty")
    @Emit()
    private onIsEmptyChanged(): boolean {
        return this.isEmpty;
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveCovid19LaboratoryOrders({ hdid: this.hdid }).catch((err) =>
            this.logger.error(`Error loading Covid19 data: ${err}`)
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
            <b-row v-if="isEmpty && !isCovid19LaboratoryLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-if="!isEmpty || isCovid19LaboratoryLoading"
                :striped="true"
                :fixed="true"
                :busy="isCovid19LaboratoryLoading"
                :items="items"
                :fields="fields"
                class="table-style d-none d-md-table"
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

.covid19-laboratory-report-table-header {
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
