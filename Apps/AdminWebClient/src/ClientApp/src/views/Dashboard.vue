<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { IDashboardService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { DateTime } from "luxon";

interface DailyData {
    date: DateTime;
    registered?: number;
    loggedIn?: number;
    notes?: number;
    dependents?: number;
}

@Component
export default class Dashboard extends Vue {
    private registeredUserCount = 0;
    private loggedInUsersCount: { [key: string]: number } = {};
    private usersWithNotesCount = 0;
    private dependentCount = 0;
    private modal = false;

    private today = DateTime.local();

    private dashboardService!: IDashboardService;

    private selectedDates: string[] = [
        DateTime.local()
            .minus({ days: 10 })
            .toISO()
            .substr(0, 10),
        DateTime.local()
            .toISO()
            .substr(0, 10)
    ];

    private tableData: DailyData[] = [];

    private get visibleTableData(): DailyData[] {
        console.log("efsefsfd");
        let visible: DailyData[] = [];
        let startDate = DateTime.fromISO(this.selectedDates[0]);
        let endDate = DateTime.fromISO(this.selectedDates[1]);
        this.tableData.forEach(element => {
            if (startDate <= element.date && element.date <= endDate) {
                visible.push(element);
            }
        });
        return visible;
    }

    private tableHeaders = [
        {
            text: "Date",
            value: "date"
        },
        { text: "Registered", value: "registered" },
        { text: "Logged In", value: "loggedIn" },
        { text: "Notes", value: "notes" },
        { text: "Dependents", value: "dependents" }
    ];

    private mounted() {
        this.dashboardService = container.get(
            SERVICE_IDENTIFIER.DashboardService
        );
        this.getRegisteredUserCount();
        this.getLoggedInUsersCount();
        this.getUsersWithNotesCount();
        this.getDependentCount();
    }

    private getRegisteredUserCount() {
        this.dashboardService.getRegisteredUsersCount().then(count => {
            this.registeredUserCount = count;
        });
    }

    private getLoggedInUsersCount() {
        this.dashboardService.getLoggedInUsersCount().then(count => {
            this.loggedInUsersCount = count;
            for (let key in count) {
                var leDate = DateTime.fromISO(key);
                var leValue = count[key];
                console.log(leDate);
                var index = this.tableData.findIndex(x => x.date === leDate);
                if (index > 0) {
                    this.tableData[index].loggedIn = leValue;
                } else {
                    this.tableData.push({ date: leDate, loggedIn: leValue });
                }
            }
        });
    }

    private getUsersWithNotesCount() {
        this.dashboardService.getUsersWithNotesCount().then(count => {
            this.usersWithNotesCount = count;
        });
    }

    private getDependentCount() {
        this.dashboardService.getDependentCount().then(count => {
            this.dependentCount = count;
        });
    }

    private formatDate(date: DateTime): string {
        return date.toFormat("dd/MM/yyyy");
    }
}
</script>

<template>
    <v-container>
        <h2>Totals</h2>
        <v-row class="px-2">
            <v-col class="col-lg-3 col-md-6 col-sm-12">
                <v-card class="text-center">
                    <h3>Registered Users</h3>
                    <h1>
                        {{ registeredUserCount }}
                    </h1>
                </v-card>
            </v-col>
            <v-col class="col-lg-3 col-md-6 col-sm-12">
                <v-card class="text-center">
                    <h3>Users With Notes</h3>
                    <h1>
                        {{ usersWithNotesCount }}
                    </h1>
                </v-card>
            </v-col>
            <v-col class="col-lg-3 col-md-6 col-sm-12">
                <v-card class="text-center">
                    <h3>Dependents</h3>
                    <h1>
                        {{ dependentCount }}
                    </h1>
                </v-card>
            </v-col>
        </v-row>
        <br />
        <h2>Daily Data</h2>
        <v-row>
            <v-col cols="12" sm="6" md="4">
                <v-dialog
                    ref="dialog"
                    v-model="modal"
                    :return-value.sync="selectedDates"
                    persistent
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
                        <v-btn text color="primary" @click="modal = false">
                            Cancel
                        </v-btn>
                        <v-btn
                            text
                            color="primary"
                            @click="$refs.dialog.save(selectedDates)"
                        >
                            OK
                        </v-btn>
                    </v-date-picker>
                </v-dialog>
            </v-col>
        </v-row>
        <v-row>
            <v-col>
                <v-data-table
                    :headers="tableHeaders"
                    :items="visibleTableData"
                    :items-per-page="50"
                >
                    <template #[`item.date`]="{ item }">
                        <span>{{ formatDate(item.date) }}</span>
                    </template>
                </v-data-table>
            </v-col>
        </v-row>
    </v-container>
</template>

<style scoped></style>
