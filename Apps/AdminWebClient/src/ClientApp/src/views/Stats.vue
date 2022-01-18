<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { Getter } from "vuex-class";

@Component
export default class StatsView extends Vue {
    @Getter("serviceEndpoints", { namespace: "config" })
    private serviceEndpoints!: { [id: string]: string };

    private inactiveDays = 90;

    private inactiveDaysRules = {
        required: (value: number) => !!value || "Required.",
        valid: (value: number) => value >= 0 || "Invalid value",
    };

    private downloadUserProfileCSV(): void {
        this.$nextTick(() => {
            (this.$refs.form as Vue & { reset: () => void }).reset();
            window.open(
                `${this.serviceEndpoints.CsvExportBaseUri}/GetUserProfiles`
            );
        });
    }
    private downloadUserNotesCSV(): void {
        this.$nextTick(() => {
            (this.$refs.form as Vue & { reset: () => void }).reset();
            window.open(`${this.serviceEndpoints.CsvExportBaseUri}/GetNotes`);
        });
    }
    private downloadUserCommentsCSV(): void {
        this.$nextTick(() => {
            (this.$refs.form as Vue & { reset: () => void }).reset();
            window.open(
                `${this.serviceEndpoints.CsvExportBaseUri}/GetComments`
            );
        });
    }
    private downloadUserRatingsCSV(): void {
        this.$nextTick(() => {
            (this.$refs.form as Vue & { reset: () => void }).reset();
            window.open(`${this.serviceEndpoints.CsvExportBaseUri}/GetRatings`);
        });
    }
    private downloadInactiveUsersCSV(): void {
        if ((this.$refs.form as Vue & { validate: () => boolean }).validate()) {
            window.open(
                `${
                    this.serviceEndpoints.CsvExportBaseUri
                }/GetInactiveUsers?inactiveDays=${
                    this.inactiveDays
                }&timeOffset=${new Date().getTimezoneOffset() * -1}`
            );
        }
    }
}
</script>

<template>
    <v-container fill-height fluid grid-list-xl>
        <v-layout justify-center wrap>
            <v-flex xs6 md4>
                <v-card dark>
                    <v-card-title>
                        <span class="headline">System Stats Download</span>
                    </v-card-title>
                    <v-form ref="form">
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
                                <v-col align="center" justify="center">
                                    <h3>User Ratings</h3>
                                    <v-btn
                                        class="info"
                                        @click="downloadUserRatingsCSV()"
                                        ><v-icon>fa-download</v-icon></v-btn
                                    >
                                </v-col>
                            </v-row>
                            <v-row
                                ><v-col
                                    cols
                                    sm="6"
                                    md="3"
                                    align="center"
                                    justify="center"
                                >
                                    <h3>Inactive Users</h3>
                                    <v-text-field
                                        v-model="inactiveDays"
                                        type="number"
                                        label="Inactive Days"
                                        :rules="[
                                            inactiveDaysRules.required,
                                            inactiveDaysRules.valid,
                                        ]"
                                    />
                                    <v-btn
                                        class="info"
                                        @click="downloadInactiveUsersCSV()"
                                        ><v-icon>fa-download</v-icon></v-btn
                                    >
                                </v-col></v-row
                            >
                        </v-container>
                    </v-form>
                </v-card>
            </v-flex>
        </v-layout>
    </v-container>
</template>
