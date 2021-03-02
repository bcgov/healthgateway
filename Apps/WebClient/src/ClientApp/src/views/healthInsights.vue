<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ErrorCardComponent from "@/components/errorCard.vue";
import LoadingComponent from "@/components/loading.vue";
import ProtectiveWordComponent from "@/components/modal/protectiveWord.vue";
import LineChartComponent from "@/components/plot/lineChart.vue";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        LoadingComponent,
        ProtectiveWordComponent,
        ErrorCard: ErrorCardComponent,
        LineChart: LineChartComponent,
    },
})
export default class HealthInsightsView extends Vue {
    @Action("retrieve", { namespace: "medication" })
    retrieveMedications!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<void>;

    @Getter("isLoading", { namespace: "medication" })
    isMedicationLoading!: boolean;

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    @Getter("user", { namespace: "user" }) user!: User;

    private logger!: ILogger;

    private startDate: DateWrapper | null = null;
    private endDate: DateWrapper | null = null;

    private timeChartData: Chart.ChartData = {};

    private readonly chartOptions: Chart.ChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
    };

    private get timelineEntries(): TimelineEntry[] {
        let timelineEntries: TimelineEntry[] = [];
        if (this.medicationStatements.length > 0) {
            // Add the medication entries to the timeline list
            for (let medication of this.medicationStatements) {
                timelineEntries.push(
                    new MedicationTimelineEntry(medication, [])
                );
            }

            timelineEntries = this.sortEntries(timelineEntries);
            timelineEntries = timelineEntries.reverse();

            this.startDate = timelineEntries[0].date;
            this.endDate = timelineEntries[timelineEntries.length - 1].date;

            this.prepareMonthlyChart(timelineEntries);
        }

        return timelineEntries;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.retrieveMedications({ hdid: this.user.hdid }).catch((err) => {
            this.logger.error(`Error loading Health Insights data: ${err}`);
        });
    }

    private get isLoading(): boolean {
        return this.isMedicationLoading;
    }

    private get visibleCount(): number {
        return this.timelineEntries.length;
    }

    private sortEntries(timelineEntries: TimelineEntry[]): TimelineEntry[] {
        return timelineEntries.sort((a, b) =>
            a.date.isAfter(b.date) ? -1 : a.date.isBefore(b.date) ? 1 : 0
        );
    }

    /**
     * Gets an array of months between two dates
     */
    private getMonthsBetweenDates(
        startDate: DateWrapper,
        endDate: DateWrapper
    ): string[] {
        if (endDate.isBefore(startDate)) {
            throw "End date must be greated than start date.";
        }

        var result: string[] = [];
        while (startDate.isBefore(endDate)) {
            result.push(startDate.format("yyyy-LL-01"));
            startDate = startDate.add({ months: 1 });
        }
        return result;
    }

    /**
     * Prepares the dataset based on the timeline entries
     */
    private prepareMonthlyChart(entries: TimelineEntry[]) {
        let months: string[] = this.getMonthsBetweenDates(
            this.startDate as DateWrapper,
            this.endDate as DateWrapper
        );

        let entryIndex = 0;
        let monthCounter: number[] = [];
        for (let monthIndex = 0; monthIndex < months.length; monthIndex++) {
            let currentMonth = months[monthIndex];
            monthCounter.push(0);

            while (entryIndex < entries.length) {
                let entry = entries[entryIndex];
                if (new DateWrapper(currentMonth).isSame(entry.date, "month")) {
                    monthCounter[monthIndex] += 1;
                    entryIndex++;
                } else {
                    break;
                }
            }
        }

        this.timeChartData.labels = [];
        this.timeChartData.datasets = [
            {
                label: "Monthly medications count",
                backgroundColor: "#ff6666",
                data: [],
            },
        ];
        this.timeChartData.datasets[0].data = [];

        for (let i = 0; i < months.length; i++) {
            this.timeChartData.labels.push(
                new DateWrapper(months[i]).format("LLL yyyy")
            );
            this.timeChartData.datasets[0].data.push(monthCounter[i]);
        }
    }

    private getReadableDate(date: DateWrapper | null): string {
        if (date === null) {
            return "N/A";
        }

        return date.format("LLL yyyy");
    }
}
</script>

<template>
    <div class="m-3">
        <LoadingComponent v-if="isLoading" :is-custom="true"></LoadingComponent>
        <b-row>
            <b-col
                id="healthInsights"
                class="col-12 col-md-10 col-lg-9 column-wrapper"
            >
                <div id="pageTitle">
                    <h1 id="subject">Health Insights</h1>
                    <hr />
                </div>
                <div>
                    <h3>Medication count over time</h3>
                    <b-row class="py-4">
                        <b-col cols="auto">
                            <strong>First month with data: </strong>
                            {{ getReadableDate(startDate) }}
                        </b-col>
                        <b-col cols="auto">
                            <strong>Last month with data: </strong
                            >{{ getReadableDate(endDate) }}
                        </b-col>
                        <b-col cols="auto">
                            <strong>Total records: </strong
                            ><span data-testid="totalRecordsText">{{
                                visibleCount
                            }}</span>
                        </b-col>
                    </b-row>
                    <b-row v-if="!isLoading && visibleCount > 0">
                        <b-col>
                            <LineChart
                                :chartdata="timeChartData"
                                :options="chartOptions"
                            />
                        </b-col>
                    </b-row>
                    <b-row
                        v-if="!isLoading && visibleCount === 0"
                        class="text-center pt-5"
                    >
                        <b-col> No medication records </b-col>
                    </b-row>
                    <b-row v-if="isLoading">
                        <b-col>
                            <content-placeholders>
                                <content-placeholders-img />
                                <content-placeholders-text :lines="1" />
                            </content-placeholders>
                        </b-col>
                    </b-row>
                </div>
            </b-col>
        </b-row>
        <ProtectiveWordComponent :is-loading="isLoading" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
