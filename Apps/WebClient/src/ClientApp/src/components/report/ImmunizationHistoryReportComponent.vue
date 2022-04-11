<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { DateWrapper } from "@/models/dateWrapper";
import {
    ImmunizationAgent,
    ImmunizationEvent,
    Recommendation,
} from "@/models/immunizationModel";
import Report from "@/models/report";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import ReportHeader from "@/models/reportHeader";
import { ReportFormatType, TemplateType } from "@/models/reportRequest";
import RequestResult from "@/models/requestResult";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IReportService } from "@/services/interfaces";

interface ImmunizationRow {
    date: string;
    immunization: string;
    agents: ImmunizationAgent[];
    provider_clinic: string;
}

interface RecomendationRow {
    immunization: string;
    due_date: string;
    status: string;
}

@Component
export default class ImmunizationHistoryReportComponent extends Vue {
    @Prop() private filter!: ReportFilter;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("isDeferredLoad", { namespace: "immunization" })
    immunizationIsDeferred!: boolean;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Getter("immunizations", { namespace: "immunization" })
    patientImmunizations!: ImmunizationEvent[];

    @Getter("recomendations", { namespace: "immunization" })
    patientRecommendations!: Recommendation[];

    private logger!: ILogger;

    private readonly headerClass = "immunization-report-table-header";

    private get isLoading(): boolean {
        return this.immunizationIsDeferred || this.isImmunizationLoading;
    }

    private get isEmpty() {
        return this.visibleImmunizations.length === 0;
    }

    private get isRecommendationEmpty() {
        return this.visibleRecomendations.length === 0;
    }

    private get visibleImmunizations(): ImmunizationEvent[] {
        let records = this.patientImmunizations.filter((record) => {
            return this.filter.allowsDate(record.dateOfImmunization);
        });

        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.dateOfImmunization);
            const secondDate = new DateWrapper(b.dateOfImmunization);

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

    private get immunizationItems(): ImmunizationRow[] {
        return this.visibleImmunizations.map<ImmunizationRow>((x) => {
            return {
                date: DateWrapper.format(x.dateOfImmunization),
                immunization: x.immunization.name,
                agents: x.immunization.immunizationAgents,
                provider_clinic: x.providerOrClinic,
            };
        });
    }

    private get visibleRecomendations(): Recommendation[] {
        let records = this.patientRecommendations.filter(
            (x) => x.immunization.name !== null && x.immunization.name !== ""
        );

        records.sort((a, b) => {
            const firstDateEmpty =
                a.diseaseDueDate === null || a.diseaseDueDate === undefined;
            const secondDateEmpty =
                b.diseaseDueDate === null || b.diseaseDueDate === undefined;

            if (firstDateEmpty && secondDateEmpty) {
                return 0;
            }

            if (firstDateEmpty) {
                return 1;
            }

            if (secondDateEmpty) {
                return -1;
            }

            const firstDate = new DateWrapper(a.diseaseDueDate);
            const secondDate = new DateWrapper(b.diseaseDueDate);

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

    private get recomendationItems(): RecomendationRow[] {
        return this.visibleRecomendations.map<RecomendationRow>((x) => {
            return {
                immunization: x.immunization.name,
                due_date:
                    x.diseaseDueDate === undefined || x.diseaseDueDate === null
                        ? ""
                        : DateWrapper.format(x.diseaseDueDate),
                status: x.status || "",
            };
        });
    }

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    @Watch("isEmpty")
    @Watch("isRecommendationEmpty")
    @Emit()
    private onIsEmptyChanged() {
        return this.isEmpty && this.isRecommendationEmpty;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveImmunizations({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading immunization data: ${err}`);
        });
    }

    private mounted() {
        this.$emit(
            "on-is-empty-changed",
            this.isEmpty && this.isRecommendationEmpty
        );
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
                records: this.immunizationItems,
                recommendations: this.recomendationItems,
            },
            template: TemplateType.Immunization,
            type: reportFormatType,
        });
    }

    private immunizationFields: ReportField[] = [
        {
            key: "date",
            thStyle: { width: "15%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "immunizationDateTitle" },
            tdAttr: { "data-testid": "immunizationDateItem" },
        },
        {
            key: "immunization",
            thStyle: { width: "25%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "immunizationNameTitle" },
            tdAttr: { "data-testid": "immunizationNameItem" },
        },
        {
            key: "agents",
            thStyle: { width: "45%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "immunizationAgentTitle" },
            tdAttr: { "data-testid": "immunizationAgentItem" },
        },
        {
            key: "provider_clinic",
            label: "Provider / Clinic",
            thStyle: { width: "15%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "immunizationProviderClinicTitle" },
            tdAttr: { "data-testid": "immunizationProviderClinicItem" },
        },
    ];

    private recomendationFields: ReportField[] = [
        {
            key: "immunization",
            thStyle: { width: "50%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "recommendationTitle" },
            tdAttr: { "data-testid": "recommendationItem" },
        },
        {
            key: "due_date",
            thStyle: { width: "25%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "recommendationDateTitle" },
            tdAttr: { "data-testid": "recommendationDateItem" },
        },
        {
            key: "status",
            thStyle: { width: "25%" },
            thClass: this.headerClass,
            thAttr: { "data-testid": "recommendationStatusTitle" },
            tdAttr: { "data-testid": "recommendationStatusItem" },
        },
    ];
}
</script>

<template>
    <div>
        <section>
            <b-row>
                <b-col>
                    <h4>Immunization History</h4>
                </b-col>
            </b-row>
            <b-row v-if="isEmpty && !isLoading">
                <b-col>No records found.</b-col>
            </b-row>
            <b-table
                v-if="!isEmpty || isLoading"
                striped
                :busy="isLoading"
                :items="immunizationItems"
                :fields="immunizationFields"
                class="table-style"
            >
                <!-- A custom formatted header cell for field 'name' -->
                <template #head(agents)>
                    <b-row>
                        <b-col>Agent</b-col>
                        <b-col>Product</b-col>
                        <b-col>Lot Number</b-col>
                    </b-row>
                </template>
                <template #cell(agents)="data">
                    <b-row
                        v-for="(agent, index) in data.item.agents"
                        :key="index"
                    >
                        <b-col> {{ agent.name }} </b-col>
                        <b-col> {{ agent.productName }} </b-col>
                        <b-col> {{ agent.lotNumber }} </b-col>
                    </b-row>
                </template>
                <template #table-busy>
                    <content-placeholders>
                        <content-placeholders-text :lines="7" />
                    </content-placeholders>
                </template>
            </b-table>
            <b-row class="mt-3">
                <b-col class="col-7">
                    <b-row>
                        <b-col>
                            <h4>Recommended Immunizations</h4>
                        </b-col>
                    </b-row>
                    <b-row>
                        <b-col>
                            <div id="disclaimer">
                                DISCLAIMER: Provincial Immunization Registry
                                record only. Immunization history displayed may
                                not portray the client’s complete immunization
                                history and may impact forecasted vaccines. For
                                information on recommended immunizations, please
                                visit
                                <a>https://www.immunizebc.ca</a> or contact your
                                local Public Health Unit.
                            </div>
                        </b-col>
                    </b-row>
                    <b-row
                        v-if="isRecommendationEmpty && !isLoading"
                        class="mt-2"
                    >
                        <b-col>No recommendations found.</b-col>
                    </b-row>
                    <b-table
                        v-if="!isRecommendationEmpty || isLoading"
                        :striped="true"
                        :fixed="true"
                        :busy="isLoading"
                        :items="recomendationItems"
                        :fields="recomendationFields"
                        class="mt-2 table-style"
                    >
                        <template #table-busy>
                            <content-placeholders>
                                <content-placeholders-text :lines="5" />
                            </content-placeholders>
                        </template>
                    </b-table>
                </b-col>
            </b-row>
        </section>
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.immunization-report-table-header {
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

h4 {
    color: $primary;
}

#disclaimer {
    font-size: 0.7em;
    font-weight: bold !important;
}
</style>
