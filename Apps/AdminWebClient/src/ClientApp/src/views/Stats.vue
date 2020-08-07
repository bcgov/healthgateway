<style scoped></style>
<template>
    <v-container fill-height fluid grid-list-xl>
        <v-layout justify-center wrap>
            <v-flex xs6 md4>
                <v-card dark>
                    <v-card-title>
                        <span class="headline">System Stats Download</span>
                    </v-card-title>
                    <v-form>
                        <v-container>
                            <v-row>
                                <v-col align="center" justify="center">
                                    <h3>User Profile</h3>
                                    <v-btn
                                        class="info"
                                        @click="downloadUserInfoCSV()"
                                        ><v-icon>fa-download</v-icon></v-btn
                                    >
                                </v-col>
                                <v-col align="center" justify="center">
                                    <h3>User Notes</h3>
                                    <v-btn
                                        class="info"
                                        @click="downloadUserNotesCSV()"
                                        ><v-icon>fa-download</v-icon></v-btn
                                    >
                                </v-col>
                                <v-col align="center" justify="center">
                                    <h3>User Comments</h3>
                                    <v-btn
                                        class="info"
                                        @click="downloadUserCommentsCSV()"
                                        ><v-icon>fa-download</v-icon></v-btn
                                    >
                                </v-col>
                            </v-row>
                        </v-container>
                    </v-form>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>
<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ICsvExportService } from "@/services/interfaces";
@Component
export default class StatsView extends Vue {
    private csvExportService!: ICsvExportService;
    private mounted() {
        this.csvExportService = container.get(
            SERVICE_IDENTIFIER.CsvExportService
        );
    }
    private downloadUserInfoCSV() {
        this.csvExportService
            .getUserProfiles()
            .then((csvData: any) => {
                console.log("user profiles retrieved.");
                // Todo: not sure if which task or if we have a task to present this export to admin user here?
            })
            .catch((err: any) => {
                console.error("Error when retrieving csv data " + err);
            });
    }
    private downloadUserNotesCSV() {
        this.csvExportService
            .getNotes()
            .then((csvData: any) => {
                console.log("user notes data retrieved.");
                // Todo: not sure if which task or if we have a task to present this export to admin user here?
            })
            .catch((err: any) => {
                console.error("Error when retrieving user notes " + err);
            });
    }
    private downloadUserCommentsCSV() {
        this.csvExportService
            .getComments()
            .then((csvData: any) => {
                console.log("user comments retrieved...");
                // Todo: not sure if which task or if we have a task to present this export to admin user here?
            })
            .catch((err: any) => {
                console.error("Error when retrieving user comments " + err);
            });
    }
}
</script>
