<script lang="ts">
import moment from "moment";
import { Component, Prop, Vue } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import CommunicationTable from "@/components/core/CommunicationTable.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { ResultType } from "@/constants/resulttype";
import BannerFeedback from "@/models/bannerFeedback";
import MessageVerification, {
    VerificationType,
} from "@/models/messageVerification";
import { QueryType } from "@/models/userQuery";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ISupportService } from "@/services/interfaces";
import PHNValidator from "@/utility/phnValidator";

interface UserSearchRow {
    hdid: string;
    email: string;
    emailVerified: string;
    emailVerificationDate: string;
    sms: string;
    smsVerified: string;
    smsVerificationCode: string;
    smsVerificationDate: string;
}

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
        CommunicationTable,
    },
})
export default class SupportView extends Vue {
    @Prop({ default: null, required: false }) hdid!: string;

    private isLoading = false;
    private showFeedback = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };

    private searchText = "";
    private selectedQueryType: QueryType | null = null;

    private tableHeaders = [
        {
            text: "HDID",
            value: "hdid",
        },
        {
            text: "Email",
            value: "email",
        },
        {
            text: "Email Verified",
            value: "emailVerified",
        },
        {
            text: "Email Verification Date",
            value: "emailVerificationDate",
        },
        {
            text: "SMS",
            value: "sms",
        },
        {
            text: "SMS Verified",
            value: "smsVerified",
        },
        {
            text: "SMS Verification Code",
            value: "smsVerificationCode",
        },
        {
            text: "SMS Verification Date",
            value: "smsVerificationDate",
        },
    ];

    private emailList: MessageVerification[] = [];

    private supportService!: ISupportService;

    private get queryTypes(): string[] {
        return Object.keys(QueryType).filter((x) => isNaN(Number(x)) !== false);
    }

    private get userInfo(): UserSearchRow[] {
        return this.emailList.map<UserSearchRow>((x) => {
            if (x.verificationType === VerificationType.Email) {
                return {
                    hdid: x.userProfileId,
                    email: x.email !== null ? x.email.to : "N/A",
                    emailVerified: x.validated ? "true" : "false",
                    emailVerificationDate: this.formatDate(x.updatedDateTime),
                    sms: "-",
                    smsVerified: "-",
                    smsVerificationCode: "-",
                    smsVerificationDate: "-",
                };
            } else {
                return {
                    hdid: x.userProfileId,
                    email: "-",
                    emailVerified: "-",
                    emailVerificationDate: "-",
                    sms: x.smsNumber !== null ? x.smsNumber : "N/A",
                    smsVerified: x.validated ? "true" : "false",
                    smsVerificationCode: x.smsValidationCode,
                    smsVerificationDate: this.formatDate(x.updatedDateTime),
                };
            }
        });
    }

    private mounted() {
        this.supportService = container.get(SERVICE_IDENTIFIER.SupportService);
        if (this.hdid) {
            this.selectedQueryType = QueryType.HDID;
            this.searchText = this.hdid;
            this.handleSearch();
            this.$router.replace({ path: "/support" });
        }
    }

    private formatDate(date: string): string {
        return moment(date).format("l LT");
    }

    private handleSearch() {
        if (this.selectedQueryType === null || this.searchText.length === 0) {
            this.emailList = [];
            return;
        }

        if (this.selectedQueryType === QueryType.PHN) {
            var isValid = PHNValidator.IsValid(this.searchText);

            if (!isValid) {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Validation error",
                    message: "Invalid PHN",
                };
                return;
            }
        }

        this.supportService
            .getMessageVerifications(this.selectedQueryType, this.searchText)
            .then((result) => {
                this.emailList = result;
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error:" + err.errorCode,
                    message: "Message: " + err.resultMessage,
                };
                console.log(err);
            });
    }
}
</script>

<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading" />
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
        />
        <form @submit.prevent="handleSearch()">
            <v-row align="center">
                <v-col cols="4" sm="3" lg="2">
                    <v-combobox
                        v-model="selectedQueryType"
                        :items="queryTypes"
                        label="Query Type"
                        outlined
                    />
                </v-col>
                <v-col>
                    <v-text-field v-model="searchText" label="Search Query" />
                </v-col>
                <v-col cols="auto">
                    <v-btn type="submit" class="mt-2">
                        Search
                        <v-icon class="ml-2" size="sm">fas fa-search</v-icon>
                    </v-btn>
                </v-col>
            </v-row>
        </form>
        <v-row justify="center">
            <v-col>
                <v-row>
                    <v-col no-gutters>
                        <v-data-table
                            :headers="tableHeaders"
                            :items="userInfo"
                            :items-per-page="5"
                        >
                            <template #:item.sentDateTime="{ item }">
                                <span>{{ formatDate(item.sentDateTime) }}</span>
                            </template>
                        </v-data-table>
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>
