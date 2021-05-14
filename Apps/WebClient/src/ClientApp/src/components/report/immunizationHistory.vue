<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import {
    ImmunizationAgent,
    ImmunizationEvent,
    Recommendation,
} from "@/models/immunizationModel";
import ReportField from "@/models/reportField";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

interface ImmunizationRow {
    date: string;
    immunization: string;
    agents: ImmunizationAgent[];
    provider_clinic: string;
}

interface RecomendationRow {
    immunization: string;
    date: string;
    status: string;
}

@Component({
    components: {
        ReportHeaderComponent,
    },
})
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

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;
    private isPreview = true;

    private readonly headerClass = "immunization-report-table-header";

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private get isLoading(): boolean {
        return this.immunizationIsDeferred || this.isImmunizationLoading;
    }

    private get isEmpty() {
        return this.visibleImmunizations.length === 0;
    }

    private get isRecommendationEmpty() {
        return this.patientRecommendations.length === 0;
    }

    private get visibleImmunizations(): ImmunizationEvent[] {
        let records = this.patientImmunizations.filter((record) => {
            return this.filter.allowsDate(record.dateOfImmunization);
        });

        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.dateOfImmunization);
            const secondDate = new DateWrapper(b.dateOfImmunization);

            return firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;
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

    private get recomendationItems(): RecomendationRow[] {
        return this.patientRecommendations.map<RecomendationRow>((x) => {
            return {
                immunization: x.immunization.name,
                date:
                    x.agentDueDate === undefined
                        ? ""
                        : DateWrapper.format(x.agentDueDate),
                status: x.status || "",
            };
        });
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveImmunizations({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading immunization data: ${err}`);
        });
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Immunization History PDF...");
        this.isPreview = false;

        return PDFUtil.generatePdf(
            "HealthGateway_ImmunizationHistory.pdf",
            this.report
        ).then(() => {
            this.isPreview = true;
        });
    }

    private immunizationFields: ReportField[] = [
        {
            key: "date",
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
        {
            key: "immunization",
            thStyle: { width: "25%" },
            thClass: this.headerClass,
        },
        {
            key: "agents",
            thStyle: { width: "45%" },
            thClass: this.headerClass,
        },
        {
            key: "provider_clinic",
            label: "Provider / Clinic",
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
    ];

    private recomendationFields: ReportField[] = [
        {
            key: "immunization",
            thStyle: { width: "50%" },
            thClass: this.headerClass,
        },
        {
            key: "date",
            thStyle: { width: "25%" },
            thClass: this.headerClass,
        },
        {
            key: "status",
            thStyle: { width: "25%" },
            thClass: this.headerClass,
        },
    ];
}
</script>

<template>
    <div>
        <div ref="report">
            <section class="pdf-item">
                <div>
                    <ReportHeaderComponent
                        v-show="!isPreview"
                        :filter="filter"
                        title="Health Gateway Immunization Record"
                    />
                    <hr />
                </div>
                <b-row>
                    <b-col>
                        <h4>Immunization History</h4>
                    </b-col>
                </b-row>
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
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
                                    record only. Immunization history displayed
                                    may not portray the client’s complete
                                    immunization history and may impact
                                    forecasted vaccines. For information on
                                    recommended immunizations, please visit
                                    <a>https://www.immunizebc.ca</a> or contact
                                    your local Public Health Unit.
                                </div>
                            </b-col>
                        </b-row>
                        <b-row
                            v-if="
                                isRecommendationEmpty &&
                                (!isLoading || !isPreview)
                            "
                            class="mt-2"
                        >
                            <b-col>No recommendations found.</b-col>
                        </b-row>
                        <b-table
                            v-if="!isRecommendationEmpty || isLoading"
                            :busy="isLoading"
                            striped
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
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.immunization-report-table-header {
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

h4 {
    color: $primary;
}

#disclaimer {
    font-size: 0.7em;
    font-weight: bold !important;
}
</style>
