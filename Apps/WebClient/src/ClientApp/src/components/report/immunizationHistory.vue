<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import ReportFilter from "@/models/reportFilter";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

interface TableField {
    key: string;
    sortable: false;
    thClass: string;
    thStyle: { width: string };
}

interface TableItems {
    date: string;
    immunization: string;
    agent: string;
    product: string;
    lot_number: string;
    provider_or_clinic: string;
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

        /*records = records.map<ImmunizationEvent>((x) => {
            return {
                id: x.id,
                isSelfReported: x.isSelfReported,
                location: x.location,
                immunization:
                status: x.status,
                dateOfImmunization: x.dateOfImmunization,
                providerOrClinic: x.providerOrClinic,
                targetedDisease: x.targetedDisease,
            };
        });*/

        /*
        records.forEach((x, index) =>
            x.immunization.immunizationAgents.push({
                code: "A-code" + index,
                name: "SomeName " + index,
                lotNumber: "LOT-" + index,
                productName: "Product " + index,
            })
        );*/

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

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveImmunizations({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading immunization data: ${err}`);
        });
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Immunization History PDF...");
        this.isPreview = false;

        PDFUtil.generatePdf(
            "HealthGateway_ImmunizationHistory.pdf",
            this.report
        ).then(() => {
            this.isPreview = true;
        });
    }

    private fields: TableField[] = [
        {
            key: "date",
            sortable: false,
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
        {
            key: "immunization",
            sortable: false,
            thStyle: { width: "25%" },
            thClass: this.headerClass,
        },
        {
            key: "agent",
            sortable: false,
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
        {
            key: "product",
            sortable: false,
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
        {
            key: "lot_number",
            sortable: false,
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
        {
            key: "provider_or_clinic",
            sortable: false,
            thStyle: { width: "15%" },
            thClass: this.headerClass,
        },
    ];
    private get items(): TableItems[] {
        return this.visibleImmunizations.map<TableItems>((x) => {
            return {
                date: DateWrapper.format(x.dateOfImmunization),
                immunization: x.immunization.name,
                agent: x.immunization.immunizationAgents.map((a) => a.name),
                product: x.immunization.immunizationAgents.map(
                    (a) => a.productName
                ),
                lot_number: x.immunization.immunizationAgents.map(
                    (a) => a.lotNumber
                ),
                provider_or_clinic: x.providerOrClinic,
            };
        });
    }
}
</script>

<template>
    <div>
        <div v-show="!isLoading" ref="report">
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
                    v-else
                    striped
                    :busy="isLoading"
                    :items="items"
                    :fields="fields"
                    class="table-style"
                >
                    <template #cell(agent)="data" colsplan="3">
                        <b-row
                            v-for="(agent, index) in data.item.agent"
                            :key="index"
                            class="border"
                        >
                            <b-col>
                                {{ agent }}
                            </b-col>
                        </b-row>
                    </template>
                    <template #cell(product)="data">
                        <b-row
                            v-for="(agent, index) in data.item.product"
                            :key="index"
                            class="border"
                        >
                            <b-col>
                                {{ agent }}
                            </b-col>
                        </b-row>
                    </template>
                    <template #cell(lot_number)="data">
                        <b-row
                            v-for="(agent, index) in data.item.lot_number"
                            :key="index"
                        >
                            <b-col>
                                {{ agent }}
                            </b-col>
                            <hr />
                        </b-row>
                    </template>
                    <template #table-busy>
                        <div class="text-center text-danger my-2">
                            <b-spinner class="align-middle"></b-spinner>
                            <strong>Loading...</strong>
                        </div>
                    </template>
                </b-table>
                <b-row>
                    <b-col class="col-7">
                        <b-row class="mt-3">
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
                        <b-row
                            v-else-if="!isRecommendationEmpty"
                            class="py-3 mt-4 header"
                        >
                            <b-col
                                data-testid="recommendationTitle"
                                class="col-6"
                                >Immunization</b-col
                            >
                            <b-col
                                data-testid="recommendationDateTitle"
                                class="col-3"
                                >Date</b-col
                            >
                            <b-col
                                data-testid="recommendationStatusTitle"
                                class="col-3"
                                >Status</b-col
                            >
                        </b-row>
                        <b-row
                            v-for="recommendation in patientRecommendations"
                            :key="recommendation.recommendationId"
                            class="item py-1"
                        >
                            <b-col
                                data-testid="recommendation"
                                class="col-6 text-nowrap"
                            >
                                {{ recommendation.immunization.name }}
                            </b-col>
                            <b-col
                                data-testid="recommendationDate"
                                class="col-3"
                            >
                                {{ formatDate(recommendation.agentDueDate) }}
                            </b-col>
                            <b-col
                                data-testid="recommendationStatus"
                                class="col-3"
                            >
                                {{ recommendation.status }}
                            </b-col>
                        </b-row>
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

.header {
    color: $primary;
    background-color: $soft_background;
    font-weight: bold;
    font-size: 0.8em;
    text-align: center;
}

h4 {
    color: $primary;
}

.item {
    font-size: 0.6em;
    //border-bottom: solid 1px $soft_background;
    //page-break-inside: avoid;
    text-align: center;
}

.item:nth-child(odd) {
    background-color: $soft_background;
}
.item:nth-child(even) {
    background-color: $medium_background;
}

#disclaimer {
    font-size: 0.7em;
    font-weight: bold !important;
}
</style>
