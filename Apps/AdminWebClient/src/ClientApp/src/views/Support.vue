<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import CommunicationTable from "@/components/core/CommunicationTable.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { ResultType } from "@/constants/resulttype";
import BannerFeedback from "@/models/bannerFeedback";
import Email from "@/models/email";
import { QueryType } from "@/models/userQuery";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ISupportService } from "@/services/interfaces";

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
        CommunicationTable,
    },
})
export default class SupportView extends Vue {
    private isLoading = false;
    private showFeedback = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };

    private tableHeaders = [
        {
            text: "Subject",
            value: "subject",
        },
        {
            text: "Status",
            value: "emailStatusCode",
        },
        {
            text: "Date",
            value: "sentDateTime",
        },
        { text: "Email", value: "to" },
        { text: "Is Invited?", value: "userInviteStatus" },
    ];

    private selectedEmails: Email[] = [];
    private emailList: Email[] = [];

    private filterText = "";

    private supportService!: ISupportService;

    private mounted() {
        this.supportService = container.get(SERVICE_IDENTIFIER.SupportService);

        this.supportService.getMessageVerifications(QueryType.Email, "marobej");
    }

    private shouldShowFeedback(show: boolean) {
        this.showFeedback = show;
    }

    private isFinishedLoading(loading: boolean) {
        this.isLoading = loading;
    }

    private bannerFeedbackInfo(banner: BannerFeedback) {
        this.bannerFeedback = banner;
    }
}
</script>

<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
        ></BannerFeedbackComponent>
        <v-row justify="center">
            <v-col md="9">
                <v-text-field
                    v-model="filterText"
                    label="Filter"
                    hide-details="auto"
                >
                    <v-icon slot="append">fas fa-search</v-icon>
                </v-text-field>
            </v-col>
        </v-row>
        <v-row justify="center">
            <v-col md="9">
                <v-row>
                    <v-col no-gutters>
                        <v-data-table
                            v-model="selectedEmails"
                            :headers="tableHeaders"
                            :items="emailList"
                            :items-per-page="5"
                            show-select
                            :search="filterText"
                        >
                            <template #:item.sentDateTime="{ item }">
                                <span>{{ formatDate(item.sentDateTime) }}</span>
                            </template>
                        </v-data-table>
                    </v-col>
                </v-row>
                <v-row justify="end" no-gutters>
                    <v-btn
                        :disabled="selectedEmails.length === 0"
                        @click="resendEmails()"
                        >Resend Emails</v-btn
                    >
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>
