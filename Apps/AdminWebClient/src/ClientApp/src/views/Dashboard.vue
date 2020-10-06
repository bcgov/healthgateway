<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { IDashboardService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
@Component
export default class Dashboard extends Vue {
    private registeredUserCount = 0;
    private unregisteredInvitedUserCount = 0;
    private loggedInUsersCount = 0;
    private waitlistedUserCount = 0;
    private usersWithNotesCount = 0;
    private dashboardService!: IDashboardService;

    private mounted() {
        this.dashboardService = container.get(
            SERVICE_IDENTIFIER.DashboardService
        );
        this.getRegisteredUserCount();
        this.getLoggedInUsersCount();
        this.getUnregisteredInvitedUserCount();
        this.getWaitlistedUserCount();
        this.getUsersWithNotesCount();
    }

    private getRegisteredUserCount() {
        this.dashboardService.getRegisteredUsersCount().then(count => {
            this.registeredUserCount = count;
        });
    }

    private getLoggedInUsersCount() {
        this.dashboardService.getLoggedInUsersCount().then(count => {
            this.loggedInUsersCount = count;
        });
    }

    private getWaitlistedUserCount() {
        this.dashboardService.getWaitlistedUsersCount().then(count => {
            this.waitlistedUserCount = count;
        });
    }

    private getUnregisteredInvitedUserCount() {
        this.dashboardService.getUnregisteredInvitedUsersCount().then(count => {
            this.unregisteredInvitedUserCount = count;
        });
    }

    private getUsersWithNotesCount() {
        this.dashboardService.getUsersWithNotesCount().then(count => {
            this.usersWithNotesCount = count;
        });
    }
}
</script>

<template>
    <v-layout>
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
                    <h3>Waitlisted Users</h3>
                    <h1>
                        {{ waitlistedUserCount }}
                    </h1>
                </v-card>
            </v-col>
            <v-col class="col-lg-3 col-md-6 col-sm-12">
                <v-card class="text-center">
                    <h3>Users Logged In Today</h3>
                    <h1>
                        {{ loggedInUsersCount }}
                    </h1>
                </v-card>
            </v-col>
            <v-col class="col-lg-3 col-md-6 col-sm-12">
                <v-card class="text-center">
                    <h3>Invited Unregistered Users</h3>
                    <h1>
                        {{ unregisteredInvitedUserCount }}
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
        </v-row>
    </v-layout>
</template>

<style scoped>
.v-btn {
    background-size: 100%;
    width: 386px;
    height: 96px;
    display: block;
    margin: 10px auto 50px;
    border: none;
}
.v-card {
    height: 100px;
    padding: 15px;
}
</style>
