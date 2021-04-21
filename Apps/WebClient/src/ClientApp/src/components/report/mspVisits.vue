<script lang="ts">
import Vue from "vue";
import { Component, Emit, Prop, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ReportHeaderComponent from "@/components/report/header.vue";
import { DateWrapper } from "@/models/dateWrapper";
import Encounter from "@/models/encounter";
import ReportFilter from "@/models/reportFilter";
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

    @Ref("report")
    readonly report!: HTMLElement;

    private logger!: ILogger;

    private isPreview = true;

    private get visibleRecords(): Encounter[] {
        let records = this.patientEncounters.filter((record) => {
            return this.filter.allowsDate(record.encounterDate);
        });
        records.sort((a, b) => {
            const firstDate = new DateWrapper(a.encounterDate);
            const secondDate = new DateWrapper(b.encounterDate);

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

    @Watch("isLoading")
    @Emit()
    private onIsLoadingChanged() {
        return this.isLoading;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveEncounters({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading encounter data: ${err}`);
        });
    }

    private formatDate(date: string): string {
        return new DateWrapper(date).format();
    }

    public async generatePdf(): Promise<void> {
        this.logger.debug("generating Health Visits PDF...");
        this.isPreview = false;

        PDFUtil.generatePdf("HealthGateway_HealthVisits.pdf", this.report).then(
            () => {
                this.isPreview = true;
            }
        );
    }
}
</script>

<template>
    <div>
        <div ref="report">
            <section class="pdf-item">
                <ReportHeaderComponent
                    v-show="!isPreview"
                    :filter="filter"
                    title="Health Gateway Health Visit History"
                />
                <b-row v-if="isEmpty && (!isLoading || !isPreview)">
                    <b-col>No records found.</b-col>
                </b-row>
                <b-row v-else-if="!isEmpty" class="py-3 header">
                    <b-col>Date</b-col>
                    <b-col>Specialty Description</b-col>
                    <b-col>Practitioner</b-col>
                    <b-col>Clinic/Practitioner</b-col>
                </b-row>
                <b-row
                    v-for="item in visibleRecords"
                    :key="item.id"
                    class="item py-1"
                >
                    <b-col class="my-auto text-nowrap">
                        {{ formatDate(item.encounterDate) }}
                    </b-col>
                    <b-col class="my-auto">
                        {{ item.specialtyDescription }}
                    </b-col>
                    <b-col class="my-auto">
                        {{ item.practitionerName }}
                    </b-col>
                    <b-col class="my-auto">
                        {{ item.clinic.name }}
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

.item {
    font-size: 0.6em;
    border-bottom: solid 1px $soft_background;
    page-break-inside: avoid;
    text-align: center;
}

.item:nth-child(odd) {
    background-color: $medium_background;
}
.item:nth-child(even) {
    background-color: $soft_background;
}
</style>
