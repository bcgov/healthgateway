<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent, Recommendation } from "@/models/immunizationModel";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import PDFUtil from "@/utility/pdfUtil";

@Component({
    components: {
        ReportHeaderComponent,
    },
})
export default class ImmunizationHistoryReportComponent extends Vue {
    @Prop() private startDate!: string | null;
    @Prop() private endDate!: string | null;

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
    private notFoundText = "Not Found";
    private isPreview = true;

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
            let filterStart = true;
            if (this.startDate !== null) {
                filterStart = new DateWrapper(
                    record.dateOfImmunization
                ).isAfterOrSame(new DateWrapper(this.startDate));
            }

            let filterEnd = true;
            if (this.endDate !== null) {
                filterEnd = new DateWrapper(
                    record.dateOfImmunization
                ).isBeforeOrSame(new DateWrapper(this.endDate));
            }
            return filterStart && filterEnd;
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.dateOfImmunization);
            const secondDate = new DateWrapper(b.dateOfImmunization);

            const value = firstDate.isAfter(secondDate)
                ? 1
                : firstDate.isBefore(secondDate)
                ? -1
                : 0;

            return value;
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
}
</script>

<template>
    <div>
        <div v-show="!isLoading" ref="report">
            <section class="pdf-item">
                <div>
                    <ReportHeaderComponent
                        v-show="!isPreview"
                        :start-date="startDate"
                        :end-date="endDate"
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
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col data-testid="immunizationDateTitle" class="col-2"
                        >Date</b-col
                    >
                    <b-col data-testid="immunizationItemTitle" class="col-3"
                        >Immunization</b-col
                    >
                    <b-col data-testid="immunizationAgentTitle" class="col-5">
                        <b-row>
                            <b-col>Agent</b-col>
                            <b-col>Product</b-col>
                            <b-col>Lot Number</b-col>
                        </b-row>
                    </b-col>
                    <b-col data-testid="immunizationProviderTitle" class="col-2"
                        >Provider / Clinic</b-col
                    >
                </b-row>
                <b-row
                    v-for="record in visibleImmunizations"
                    :key="record.id"
                    class="item py-1"
                >
                    <b-col
                        data-testid="immunizationItemDate"
                        class="col-2 text-nowrap"
                    >
                        {{ formatDate(record.dateOfImmunization) }}
                    </b-col>
                    <b-col data-testid="immunizationItemName" class="col-3">
                        {{ record.immunization.name }}
                    </b-col>
                    <b-col data-testid="immunizationItemAgent" class="col-5">
                        <b-row
                            v-for="agent in record.immunization
                                .immunizationAgents"
                            :key="agent.code"
                        >
                            <b-col>
                                {{ agent.name }}
                            </b-col>
                            <b-col>
                                {{ agent.productName }}
                            </b-col>
                            <b-col>
                                {{ agent.lotNumber }}
                            </b-col>
                        </b-row>
                    </b-col>
                    <b-col
                        data-testid="immunizationItemProviderClinic"
                        class="col-2"
                    >
                        {{ record.providerOrClinic }}
                    </b-col>
                </b-row>
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

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

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
    border-bottom: solid 1px $soft_background;
    page-break-inside: avoid;
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
