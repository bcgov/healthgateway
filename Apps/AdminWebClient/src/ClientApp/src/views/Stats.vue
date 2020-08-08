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
                                        @click="downloadUserProfileCSV()"
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
import container from "@/plugins/inversify.config";
import { Getter } from "vuex-class";

@Component
export default class StatsView extends Vue {
    @Getter("serviceEndpoints", { namespace: "config" })
    private serviceEndpoints!: { [id: string]: string };
    private downloadUserProfileCSV(): void {
        window.open(
            `${this.serviceEndpoints.CsvExportBaseUri}/GetUserProfiles`
        );
    }
    private downloadUserNotesCSV(): void {
        window.open(`${this.serviceEndpoints.CsvExportBaseUri}/GetNotes`);
    }
    private downloadUserCommentsCSV(): void {
        window.open(`${this.serviceEndpoints.CsvExportBaseUri}/GetComments`);
    }
}
</script>
