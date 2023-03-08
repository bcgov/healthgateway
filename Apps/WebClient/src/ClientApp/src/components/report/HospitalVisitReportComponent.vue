<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import { HospitalVisit } from "@/models/encounter";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

interface HospitalVisitRow {
    date: string;
    health_service: string;
    visit_type: string;
    location: string;
    provider: string;
}

@Component
export default class HospitalVisitReportComponent extends Vue {
    @Prop({ required: true })
    hdid!: string;

    @Prop() private filter!: ReportFilter;

    @Action("retrieveHospitalVisits", { namespace: "encounter" })
    retrieveHospitalVisits!: (params: { hdid: string }) => Promise<void>;

    @Getter("hospitalVisitsAreLoading", { namespace: "encounter" })
    hospitalVisitsAreLoading!: (hdid: string) => boolean;

    @Getter("hospitalVisits", { namespace: "encounter" })
    hospitalVisits!: (hdid: string) => HospitalVisit[];

    private logger!: ILogger;

    private readonly headerClass = "hospital-visit-report-table-header";

    private get isLoading(): boolean {
        return this.hospitalVisitsAreLoading(this.hdid);
    }

    private get visibleRecords(): HospitalVisit[] {
        let records = this.hospitalVisits(this.hdid).filter((record) =>
            this.filter.allowsDate(record.admitDateTime)
        );
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.admitDateTime);
            const secondDate = new DateWrapper(b.admitDateTime);

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

    private get items(): HospitalVisitRow[] {
        return this.visibleRecords.map<HospitalVisitRow>((x) => ({
            date: DateWrapper.format(x.admitDateTime),
            health_service: x.healthService,
            visit_type: x.visitType,
            location: x.facility,
            provider: x.provider,
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
        this.retrieveHospitalVisits({ hdid: this.hdid }).catch((err) =>
            this.logger.error(`Error loading hospital visit data: ${err}`)
        );
    }

    private mounted(): void {
        this.onIsEmptyChanged();
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
            template: TemplateType.HospitalVisit,
            type: reportFormatType,
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "hospital-visit-date" },
        },
        {
            key: "health_service",
            thClass: this.headerClass,
        },
        {
            key: "visit_type",
            thClass: this.headerClass,
        },
        {
            key: "location",
            thClass: this.headerClass,
        },
        {
            key: "provider",
            thClass: this.headerClass,
        },
    ];
}
</script>

<template>
    <div>
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
                    data-testid="hospital-visit-report-table"
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
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.hospital-visit-report-table-header {
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
