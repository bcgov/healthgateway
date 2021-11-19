<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import Encounter from "@/models/encounter";
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

interface EncounterRow {
    date: string;
    specialty_description: string;
    practitioner: string;
    clinic_practitioner: string;
}

@Component
export default class MSPVisitsReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Action("retrieve", { namespace: "encounter" })
    retrieveEncounters!: (params: { hdid: string }) => Promise<void>;

    @Getter("isLoading", { namespace: "encounter" })
    isLoading!: boolean;

    @Getter("patientEncounters", { namespace: "encounter" })
    patientEncounters!: Encounter[];

    @Getter("user", { namespace: "user" })
    private user!: User;

    private logger!: ILogger;

    private readonly headerClass = "encounter-report-table-header";

    private get visibleRecords(): Encounter[] {
        let records = this.patientEncounters.filter((record) => {
            return this.filter.allowsDate(record.encounterDate);
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.encounterDate);
            const secondDate = new DateWrapper(b.encounterDate);

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

    private get isEmpty() {
        return this.visibleRecords.length === 0;
    }

    private get items(): EncounterRow[] {
        return this.visibleRecords.map<EncounterRow>((x) => {
            return {
                date: DateWrapper.format(x.encounterDate),
                specialty_description: x.specialtyDescription,
                practitioner: x.practitionerName,
                clinic_practitioner: x.clinic.name,
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
        this.retrieveEncounters({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading encounter data: ${err}`);
        });
    }

    private mounted() {
        this.onIsEmptyChanged();
    }

    public async generateReport(
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
            template: TemplateType.Encounter,
            type: reportFormatType,
        });
    }

    private fields: ReportField[] = [
        {
            key: "date",
            thClass: this.headerClass,
            tdAttr: { "data-testid": "mspVisitDateItem" },
        },
        {
            key: "specialty_description",
            thClass: this.headerClass,
        },
        {
            key: "practitioner",
            thClass: this.headerClass,
        },
        {
            key: "clinic_practitioner",
            label: "Clinic/Practitioner",
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
.encounter-report-table-header {
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
