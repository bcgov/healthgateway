<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { ResultType } from "@/constants/resulttype";
import BannerFeedback from "@/models/bannerFeedback";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import MessageVerification, { Email } from "@/models/messageVerification";
import { QueryType } from "@/models/userQuery";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ISupportService } from "@/services/interfaces";
import { Mask, phnMaskTemplate } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";

interface UserSearchRow {
    hdid: string;
    personalHealthNumber: string;
    emailOrSms: string;
    verificationCode: string;
    verificationDate: string;
}

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
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

    private searchPhn = "";
    private searchText = "";
    private selectedQueryType: QueryType | null = null;

    private tableHeaders = [
        {
            text: "HDID",
            value: "hdid",
        },
        {
            text: "PHN",
            value: "personalHealthNumber",
        },
        {
            text: "Email/SMS",
            value: "emailOrSms",
        },
        {
            text: "Verification Code",
            value: "verificationCode",
        },
        {
            text: "Verification Date",
            value: "verificationDate",
        },
    ];

    private emailList: MessageVerification[] = [];

    private supportService!: ISupportService;

    private get queryTypes(): string[] {
        return Object.keys(QueryType).filter((x) => isNaN(Number(x)) !== false);
    }

    private getEmailOrSms(email: Email | null, sms: string | null): string {
        let smsValue = sms !== null ? sms : "";
        return email !== null ? email.to : smsValue;
    }

    private getVerificationDate(updatedDateTime: string): string {
        return this.formatDateTime(updatedDateTime);
    }

    private getPhn(phn: string): string {
        return phn !== null ? phn : "-";
    }

    private get phnSelected(): boolean {
        return this.selectedQueryType === QueryType.PHN;
    }

    private get phnMask(): Mask {
        return phnMaskTemplate;
    }

    private get userInfo(): UserSearchRow[] {
        return this.emailList.map<UserSearchRow>((x) => {
            return {
                hdid: x.userProfileId,
                personalHealthNumber: this.getPhn(x.personalHealthNumber),
                emailOrSms: this.getEmailOrSms(x.email, x.smsNumber),
                verificationDate: this.getVerificationDate(x.updatedDateTime),
                verificationCode: x.smsValidationCode,
            };
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

    private clearSearch(): void {
        this.searchText = "";
        this.searchPhn = "";
        this.emailList = [];
        this.showFeedback = false;
    }

    private formatDateTime(date: StringISODateTime): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date, { isUtc: true }).format(
            DateWrapper.defaultDateTimeFormat
        );
    }

    private handleSearch() {
        this.showFeedback = false;
        if (
            this.selectedQueryType === null ||
            (this.selectedQueryType !== QueryType.PHN &&
                this.searchText.length === 0) ||
            (this.selectedQueryType === QueryType.PHN &&
                this.searchPhn.length === 0)
        ) {
            this.emailList = [];
            return;
        }

        let searchText =
            this.selectedQueryType !== QueryType.PHN ? this.searchText : "";

        if (this.selectedQueryType === QueryType.PHN) {
            const phnDigits = this.searchPhn.replace(/[^0-9]/g, "");
            var isValid = PHNValidator.IsValid(phnDigits);

            if (!isValid) {
                this.emailList = [];
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Validation error",
                    message: "Invalid PHN",
                };
                return;
            }
            searchText = phnDigits;
        }

        this.supportService
            .getMessageVerifications(this.selectedQueryType, searchText)
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
                        @change="clearSearch"
                    />
                </v-col>
                <v-col>
                    <v-text-field
                        v-show="!phnSelected"
                        v-model="searchText"
                        label="Search Query"
                    />
                    <v-text-field
                        v-show="phnSelected"
                        v-model="searchPhn"
                        v-mask="phnMask"
                        label="Search Query"
                    />
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
                                <span>{{
                                    formatDateTime(item.sentDateTime)
                                }}</span>
                            </template>
                        </v-data-table>
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>
