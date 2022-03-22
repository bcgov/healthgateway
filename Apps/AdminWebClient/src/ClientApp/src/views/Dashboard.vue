<script lang="ts">
import { DateTime } from "luxon";
import { Component, Vue, Watch } from "vue-property-decorator";

import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IDashboardService } from "@/services/interfaces";

interface DailyData {
    date: DateTime;
    registered?: number;
    loggedIn?: number;
    dependents?: number;
}

@Component
export default class Dashboard extends Vue {
    private totalRegisteredUserCount = 0;
    private totalDependentCount = 0;

    private isLoadingRegistered = true;
    private isLoadingLoggedIn = true;
    private isLoadingDependent = true;
    private isLoadingRecurrentCount = true;
    private isLoadingRatings = true;

    private periodPickerModal = false;

    private uniqueDays = 3;
    private uniquePeriodDates: string[] = [
        DateTime.local().minus({ days: 30 }).toISO().substring(0, 10),
        DateTime.local().toISO().substring(0, 10),
    ];
    private uniqueUsers = 0;

    private ratingPickerModal = false;
    private ratingPeriodDates: string[] = [
        DateTime.local().minus({ days: 30 }).toISO().substring(0, 10),
        DateTime.local().toISO().substring(0, 10),
    ];

    private debounceTimer: NodeJS.Timeout | null = null;

    private recurringRules = {
        required: (value: number) => !!value || "Required.",
        valid: (value: number) => value >= 0 || "Invalid value",
    };

    private today = DateTime.local();

    private dashboardService!: IDashboardService;

    private dailyDataDatesModal = false;
    private selectedDates: string[] = [
        DateTime.local().minus({ days: 10 }).toISO().substring(0, 10),
        DateTime.local().toISO().substring(0, 10),
    ];

    private tableData: DailyData[] = [];

    @Watch("isLoading")
    private onIsLoading(newVal: boolean, oldVal: boolean) {
        if (oldVal && !newVal) {
            this.tableData.sort((a, b) => {
                if (a.date < b.date) {
                    return 1;
                }
                if (a.date > b.date) {
                    return -1;
                }
                return 0;
            });
        }
    }

    @Watch("uniqueDays")
    @Watch("periodPickerModal")
    private onRecurringInputChange() {
        if (this.uniqueDays >= 0 && !this.periodPickerModal) {
            this.getRecurringUsersDebounced();
        }
    }

    @Watch("ratingPickerModal")
    private onRatingsInputChange(newVal: boolean, oldVal: boolean) {
        if (!newVal && oldVal) {
            this.getRatings();
        }
    }

    private get visibleTableData(): DailyData[] {
        let visible: DailyData[] = [];
        let startDate = DateTime.fromISO(this.selectedDates[0]);
        let endDate = DateTime.fromISO(this.selectedDates[1]);
        this.tableData.forEach((element) => {
            if (startDate <= element.date && element.date <= endDate) {
                visible.push(element);
            }
        });
        return visible;
    }

    private get isLoading(): boolean {
        return (
            this.isLoadingRegistered ||
            this.isLoadingLoggedIn ||
            this.isLoadingDependent ||
            this.isLoadingRecurrentCount ||
            this.isLoadingRatings
        );
    }

    private tableHeaders = [
        {
            text: "Date",
            value: "date",
        },
        { text: "Registered", value: "registered" },
        { text: "Logged In", value: "loggedIn" },
        { text: "Dependents", value: "dependents" },
    ];

    private ratingSummary: { [key: string]: number } = {};

    private get ratingCount(): number {
        let totalCount = 0;
        for (let key in this.ratingSummary) {
            totalCount += this.ratingSummary[key];
        }

        return totalCount;
    }

    private get ratingAverage(): string {
        if (this.ratingCount === 0) {
            return "N/A";
        }

        var totalScore = 0;
        for (let key in this.ratingSummary) {
            totalScore += Number(key) * this.ratingSummary[key];
        }

        return (totalScore / this.ratingCount).toFixed(1);
    }

    private get ratingBars(): { [key: string]: number } {
        var bars: { [key: string]: number } = {};
        for (var starRating = 5; starRating >= 1; starRating--) {
            if (this.ratingSummary[starRating] !== undefined) {
                bars[starRating] =
                    (this.ratingSummary[starRating] / this.ratingCount) * 100;
            } else {
                bars[starRating] = 0;
            }
        }

        return bars;
    }

    private mounted() {
        this.dashboardService = container.get(
            SERVICE_IDENTIFIER.DashboardService
        );
        this.getAllData();
    }

    private getAllData() {
        this.tableData = [];
        this.totalRegisteredUserCount = 0;
        this.totalDependentCount = 0;
        this.getRegisteredUserCount();
        this.getLoggedInUsersCount();
        this.getDependentCount();
        this.getRecurringUsers();
        this.getRatings();
    }

    private getRegisteredUserCount() {
        this.isLoadingRegistered = true;
        this.dashboardService
            .getRegisteredUsersCount()
            .then((count) => {
                for (let key in count) {
                    var countDate = DateTime.fromISO(key);
                    var dateValue = count[key];
                    var dayData = this.tableData.find(
                        (x) => x.date.toMillis() === countDate.toMillis()
                    );
                    if (dayData !== undefined) {
                        dayData.registered = dateValue;
                    } else {
                        this.tableData.push({
                            date: countDate,
                            registered: dateValue,
                        });
                    }
                    this.totalRegisteredUserCount += dateValue;
                }
            })
            .finally(() => {
                this.isLoadingRegistered = false;
            });
    }

    private getLoggedInUsersCount() {
        this.isLoadingLoggedIn = true;
        this.dashboardService
            .getLoggedInUsersCount()
            .then((count) => {
                for (let key in count) {
                    var countDate = DateTime.fromISO(key);
                    var dateValue = count[key];
                    var dayData = this.tableData.find(
                        (x) => x.date.toMillis() === countDate.toMillis()
                    );
                    if (dayData !== undefined) {
                        dayData.loggedIn = dateValue;
                    } else {
                        this.tableData.push({
                            date: countDate,
                            loggedIn: dateValue,
                        });
                    }
                }
            })
            .finally(() => {
                this.isLoadingLoggedIn = false;
            });
    }

    private getDependentCount() {
        this.isLoadingDependent = true;
        this.dashboardService
            .getDependentCount()
            .then((count) => {
                for (let key in count) {
                    var countDate = DateTime.fromISO(key);
                    var dateValue = count[key];
                    var dayData = this.tableData.find(
                        (x) => x.date.toMillis() === countDate.toMillis()
                    );
                    if (dayData !== undefined) {
                        dayData.dependents = dateValue;
                    } else {
                        this.tableData.push({
                            date: countDate,
                            dependents: dateValue,
                        });
                    }

                    this.totalDependentCount += dateValue;
                }
            })
            .finally(() => {
                this.isLoadingDependent = false;
            });
    }

    private getRecurringUsers() {
        this.isLoadingRecurrentCount = true;
        this.dashboardService
            .getRecurrentUserCount(
                this.uniqueDays,
                this.uniquePeriodDates[0],
                this.uniquePeriodDates[1]
            )
            .then((count) => {
                this.uniqueUsers = count;
            })
            .finally(() => {
                this.isLoadingRecurrentCount = false;
            });
    }

    private getRecurringUsersDebounced() {
        // cancel pending call
        if (this.debounceTimer !== null) {
            clearTimeout(this.debounceTimer);
        }

        // delay new call 500ms
        this.debounceTimer = setTimeout(() => {
            this.getRecurringUsers();
        }, 250);
    }

    private getRatings() {
        this.isLoadingRatings = true;
        var endDate = DateTime.fromISO(this.ratingPeriodDates[1])
            .toISO()
            .substring(0, 10);
        this.dashboardService
            .getRatings(this.ratingPeriodDates[0], endDate)
            .then((ratings) => {
                this.ratingSummary = ratings;
            })
            .finally(() => {
                this.isLoadingRatings = false;
            });
    }

    private formatDate(date: DateTime): string {
        return date.toFormat("dd/MM/yyyy");
    }
}
</script>

<template>
    <v-container>
        <v-btn
            fab
            color="secondary"
            dark
            class="refresh-button"
            :loading="isLoading"
            @click="getAllData"
        >
            <v-icon>refresh</v-icon>
        </v-btn>
        <h2 class="mt-6 mb-4">Totals</h2>
        <v-row class="px-2">
            <v-col cols="12" sm="6" md="5" offset-md="1">
                <v-card v-if="!isLoading" class="text-center">
                    <h3>Registered Users</h3>
                    <h1>
                        {{ totalRegisteredUserCount }}
                    </h1>
                </v-card>
                <v-skeleton-loader v-else type="image" max-height="76px" />
            </v-col>
            <v-col cols="12" sm="6" md="5">
                <v-card v-if="!isLoading" class="text-center">
                    <h3>Dependents</h3>
                    <h1>
                        {{ totalDependentCount }}
                    </h1>
                </v-card>
                <v-skeleton-loader v-else type="image" max-height="76px" />
            </v-col>
        </v-row>

        <h2 class="mt-6 mb-4">Recurring Users</h2>
        <v-row class="px-2">
            <v-col cols="12" sm="6" md="5" offset-md="1">
                <v-dialog
                    ref="periodDialog"
                    v-model="periodPickerModal"
                    :return-value.sync="uniquePeriodDates"
                    persistent
                    :disabled="isLoading"
                    width="290px"
                >
                    <template #activator="{ on, attrs }">
                        <v-row>
                            <v-col>
                                <v-text-field
                                    v-model="uniquePeriodDates[0]"
                                    label="Start Date"
                                    prepend-icon="mdi-calendar"
                                    readonly
                                    v-bind="attrs"
                                    v-on="on"
                                ></v-text-field>
                            </v-col>
                            <v-col>
                                <v-text-field
                                    v-model="uniquePeriodDates[1]"
                                    label="End Date"
                                    readonly
                                    v-bind="attrs"
                                    v-on="on"
                                ></v-text-field>
                            </v-col>
                        </v-row>
                    </template>
                    <v-date-picker
                        v-model="uniquePeriodDates"
                        :max="today.toISO()"
                        min="2019-06-01"
                        range
                        scrollable
                        no-title
                    >
                        <v-spacer></v-spacer>
                        <v-btn
                            text
                            color="primary"
                            @click="periodPickerModal = false"
                        >
                            Cancel
                        </v-btn>
                        <v-btn
                            text
                            :disabled="uniquePeriodDates.length !== 2"
                            color="primary"
                            @click="
                                uniquePeriodDates;
                                $refs.periodDialog.save(uniquePeriodDates);
                            "
                        >
                            OK
                        </v-btn>
                    </v-date-picker>
                </v-dialog>
            </v-col>
            <v-col cols="6" sm="3" md="2">
                <v-text-field
                    v-model="uniqueDays"
                    type="number"
                    label="Unique days"
                    :rules="[recurringRules.required, recurringRules.valid]"
                />
            </v-col>
            <v-col cols="6" sm="3">
                <v-card v-if="!isLoadingRecurrentCount" class="text-center">
                    <h3>User Count</h3>
                    <h1>
                        {{ uniqueUsers }}
                    </h1>
                </v-card>
                <v-skeleton-loader v-else max-height="76px" type="card" />
            </v-col>
        </v-row>

        <h2 class="mt-6 mb-4">Rating</h2>
        <v-row class="px-2">
            <v-col cols="12" sm="6" md="5" offset-md="1">
                <v-dialog
                    ref="ratingDialog"
                    v-model="ratingPickerModal"
                    :return-value.sync="ratingPeriodDates"
                    persistent
                    :disabled="isLoading"
                    width="290px"
                >
                    <template #activator="{ on, attrs }">
                        <v-row>
                            <v-col>
                                <v-text-field
                                    v-model="ratingPeriodDates[0]"
                                    label="Start Date"
                                    prepend-icon="mdi-calendar"
                                    readonly
                                    v-bind="attrs"
                                    v-on="on"
                                ></v-text-field>
                            </v-col>
                            <v-col>
                                <v-text-field
                                    v-model="ratingPeriodDates[1]"
                                    label="End Date"
                                    readonly
                                    v-bind="attrs"
                                    v-on="on"
                                ></v-text-field>
                            </v-col>
                        </v-row>
                    </template>
                    <v-date-picker
                        v-model="ratingPeriodDates"
                        :max="today.toISO()"
                        min="2019-06-01"
                        range
                        scrollable
                        no-title
                    >
                        <v-spacer></v-spacer>
                        <v-btn
                            text
                            color="primary"
                            @click="ratingPickerModal = false"
                        >
                            Cancel
                        </v-btn>
                        <v-btn
                            text
                            :disabled="ratingPeriodDates.length !== 2"
                            color="primary"
                            @click="
                                ratingPeriodDates;
                                $refs.ratingDialog.save(ratingPeriodDates);
                            "
                        >
                            OK
                        </v-btn>
                    </v-date-picker>
                </v-dialog>
            </v-col>
            <v-col cols="12" sm="6" md="5">
                <v-card
                    v-if="!isLoadingRatings"
                    class="text-center dashboard-rating-card"
                >
                    <h3>Rating Summary</h3>
                    <v-row class="px-0 mx-0">
                        <v-col cols="3" sm="4" class="px-0 mx-0"
                            ><h1>{{ ratingAverage }}</h1>
                            <span class="text-caption">Out of 5</span></v-col
                        >
                        <v-col>
                            <v-row
                                v-for="i in 5"
                                :key="i"
                                class="ma-0 pa-0"
                                align="center"
                                justify="center"
                            >
                                <v-col
                                    class="ma-0 pa-0 mr-1 text-right"
                                    cols="3"
                                >
                                    <v-rating
                                        readonly
                                        :length="Number(6 - i)"
                                        size="10"
                                        :value="5"
                                        class="ma-0 pa-0"
                                    ></v-rating>
                                </v-col>
                                <v-col class="ma-0 pa-0">
                                    <v-progress-linear
                                        class="ma-0 pa-0"
                                        :value="ratingBars[6 - i]"
                                    ></v-progress-linear>
                                </v-col>
                                <v-col
                                    class="ma-0 pa-0 text-right text-caption"
                                    cols="3"
                                >
                                    {{
                                        ratingSummary[6 - i] === undefined
                                            ? 0
                                            : ratingSummary[6 - i]
                                    }}
                                </v-col>
                            </v-row>
                        </v-col>
                    </v-row>
                    <div class="text-right text-caption pa-2">
                        {{ ratingCount }} Ratings
                    </div>
                </v-card>
                <v-skeleton-loader v-else max-height="164px" type="card" />
            </v-col>
        </v-row>

        <h2 class="mt-6 mb-4">Daily Data</h2>
        <v-row>
            <v-col cols="12" sm="6" md="5" offset-md="1">
                <v-dialog
                    ref="dailyDialog"
                    v-model="dailyDataDatesModal"
                    :return-value.sync="selectedDates"
                    persistent
                    :disabled="isLoading"
                    width="290px"
                >
                    <template #activator="{ on, attrs }">
                        <v-row>
                            <v-col>
                                <v-text-field
                                    v-model="selectedDates[0]"
                                    label="Start Date"
                                    prepend-icon="mdi-calendar"
                                    readonly
                                    v-bind="attrs"
                                    v-on="on"
                                ></v-text-field>
                            </v-col>
                            <v-col>
                                <v-text-field
                                    v-model="selectedDates[1]"
                                    label="End Date"
                                    readonly
                                    v-bind="attrs"
                                    v-on="on"
                                ></v-text-field>
                            </v-col>
                        </v-row>
                    </template>
                    <v-date-picker
                        v-model="selectedDates"
                        :max="today.toISO()"
                        min="2019-06-01"
                        range
                        scrollable
                        no-title
                    >
                        <v-spacer></v-spacer>
                        <v-btn
                            text
                            color="primary"
                            @click="dailyDataDatesModal = false"
                        >
                            Cancel
                        </v-btn>
                        <v-btn
                            text
                            color="primary"
                            :disabled="selectedDates.length !== 2"
                            @click="$refs.dailyDialog.save(selectedDates)"
                        >
                            OK
                        </v-btn>
                    </v-date-picker>
                </v-dialog>
            </v-col>
        </v-row>
        <v-row>
            <v-col cols="12" md="10" offset-md="1">
                <v-data-table
                    :headers="tableHeaders"
                    :items="visibleTableData"
                    :items-per-page="30"
                    :loading="isLoading"
                    loading-text="Loading... Please wait"
                    :footer-props="{
                        itemsPerPageOptions: [30, 60, 90, -1],
                    }"
                >
                    <template #[`item.date`]="{ item }">
                        <span>{{ formatDate(item.date) }}</span>
                    </template>
                </v-data-table>
            </v-col>
        </v-row>
    </v-container>
</template>

<style lang="scss">
.dashboard-rating-card {
    .v-rating {
        font-size: 10px;
        padding: 0px !important;
        margin-top: 0px !important;
        margin-bottom: 0px !important;
    }
    .v-rating .v-icon {
        padding: 0px;
    }
}

.refresh-button {
    position: fixed;
    right: 16px;
    top: 80px;
    z-index: 1000;
}
</style>
