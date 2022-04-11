<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import ErrorCardComponent from "@/components/ErrorCardComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import ProtectiveWordComponent from "@/components/modal/ProtectiveWordComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import LineChartComponent from "@/components/plot/LineChartComponent.vue";
import ResourceCentreComponent from "@/components/ResourceCentreComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { DateWrapper } from "@/models/dateWrapper";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import TimelineEntry from "@/models/timelineEntry";
import User from "@/models/user";
import { UserComment } from "@/models/userComment";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        ProtectiveWordComponent,
        ErrorCard: ErrorCardComponent,
        LineChart: LineChartComponent,
        "resource-centre": ResourceCentreComponent,
    },
})
export default class HealthInsightsView extends Vue {
    @Action("retrieveMedicationStatements", { namespace: "medication" })
    retrieveMedications!: (params: {
        hdid: string;
        protectiveWord?: string;
    }) => Promise<void>;

    @Getter("isMedicationStatementLoading", { namespace: "medication" })
    isMedicationLoading!: boolean;

    @Getter("medicationStatements", { namespace: "medication" })
    medicationStatements!: MedicationStatementHistory[];

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("getEntryComments", { namespace: "comment" })
    getEntryComments!: (entyId: string) => UserComment[] | null;

    private logger!: ILogger;

    private startDate: DateWrapper | null = null;
    private endDate: DateWrapper | null = null;

    private timeChartData: Chart.ChartData = {};

    private readonly chartOptions: Chart.ChartOptions = {
        responsive: true,
        maintainAspectRatio: false,
    };

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Health Insights",
            to: "/healthInsights",
            active: true,
            dataTestId: "breadcrumb-health-insights",
        },
    ];

    private get timelineEntries(): TimelineEntry[] {
        let timelineEntries: TimelineEntry[] = [];
        if (this.medicationStatements.length > 0) {
            // Add the medication entries to the timeline list
            for (let medication of this.medicationStatements) {
                timelineEntries.push(
                    new MedicationTimelineEntry(
                        medication,
                        this.getEntryComments
                    )
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
        return timelineEntries.sort((a, b) => {
            if (a.date.isBefore(b.date)) {
                return 1;
            }
            if (a.date.isAfter(b.date)) {
                return -1;
            }
            return 0;
        });
    }

    /**
     * Gets an array of months between two dates
     */
    private getMonthsBetweenDates(
        startDate: DateWrapper,
        endDate: DateWrapper
    ): string[] {
        if (endDate.isBefore(startDate)) {
            throw Error("End date must be greater than start date.");
        }

        var currentMonth = startDate.startOf("month");
        var result: string[] = [];
        while (currentMonth.isBeforeOrSame(endDate)) {
            result.push(currentMonth.format("yyyy-LL-01"));
            currentMonth = currentMonth.add({ months: 1 });
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
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" :is-custom="true" />
        <b-row>
            <b-col id="healthInsights" class="col-12 col-md-10 col-lg-9">
                <page-title title="Health Insights" />
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
        <resource-centre />
        <ProtectiveWordComponent :is-loading="isLoading" />
    </div>
</template>
