<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";
import { DataTableHeader } from "vuetify";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { FeedbackType } from "@/constants/feedbacktype";
import { SnackbarPosition } from "@/constants/snackbarPosition";
import BannerFeedback from "@/models/bannerFeedback";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import { instanceOfResultError } from "@/models/errors";
import MessagingVerification, {
    VerificationType,
} from "@/models/messagingVerification";
import User from "@/models/user";
import { QueryType } from "@/models/userQuery";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ISupportService } from "@/services/interfaces";
import { Mask, phnMaskTemplate } from "@/utility/masks";
import PHNValidator from "@/utility/phnValidator";

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
    },
})
export default class SupportView extends Vue {
    @Prop({ default: null, required: false })
    hdid!: string;

    public supportService!: ISupportService;
    public usersLoading = false;
    public showFeedback = false;
    public bannerFeedback: BannerFeedback = {
        type: FeedbackType.NONE,
        title: "",
        message: "",
    };

    public searchPhn = "";
    public searchText = "";
    public selectedQueryType: QueryType | null = null;
    public users: User[] = [];
    public expandedUsers: User[] = [];
    public verifications = new Map();
    public componentKey = 0;

    public userTableHeaders: DataTableHeader[] = [
        {
            text: "HDID",
            value: "hdid",
        },
        {
            text: "PHN",
            value: "personalHealthNumber",
        },
        {
            text: "Last Login",
            value: "lastLoginDateTime",
            sort: (first: StringISODateTime, second: StringISODateTime) => {
                const firstDateTime = new DateWrapper(first, { isUtc: true });
                const secondDateTime = new DateWrapper(second, { isUtc: true });
                return secondDateTime.diff(firstDateTime).as("milliseconds");
            },
        },
        {
            text: "",
            value: "data-table-expand",
            sortable: false,
        },
    ];

    public verificationsTableHeaders: DataTableHeader[] = [
        {
            text: "Email/SMS",
            value: "emailOrSms",
            sortable: false,
        },
        {
            text: "Verified",
            value: "validated",
            sort: (first: boolean, second: boolean) => {
                return first === second ? 0 : first ? -1 : 1;
            },
        },
        {
            text: "Verification Date",
            value: "updatedDateTime",
            sort: (first: StringISODateTime, second: StringISODateTime) => {
                const firstDateTime = new DateWrapper(first, { isUtc: true });
                const secondDateTime = new DateWrapper(second, { isUtc: true });
                return secondDateTime.diff(firstDateTime).as("milliseconds");
            },
        },
        {
            text: "Verification Code",
            value: "smsValidationCode",
        },
    ];

    public get queryTypes(): string[] {
        return Object.keys(QueryType).filter((x) => isNaN(Number(x)) !== false);
    }

    public get phnSelected(): boolean {
        return this.selectedQueryType === QueryType.PHN;
    }

    public get phnMask(): Mask {
        return phnMaskTemplate;
    }

    public get snackbarPositionBottom(): string {
        return SnackbarPosition.Bottom;
    }

    public mounted() {
        this.supportService = container.get(SERVICE_IDENTIFIER.SupportService);
        if (this.hdid) {
            this.selectedQueryType = QueryType.HDID;
            this.searchText = this.hdid;
            this.handleSearch();
            this.$router.replace({ path: "/support" });
        }
    }

    public userVerifications(hdid: string): MessagingVerification[] {
        return this.verifications.get(hdid) ?? [];
    }

    public verificationsLoading(hdid: string): boolean {
        return !this.verifications.has(hdid);
    }

    public forceRerender(): void {
        this.componentKey++;
    }

    public clearSearch(): void {
        this.searchText = "";
        this.searchPhn = "";
        this.users = [];
        this.showFeedback = false;
    }

    public showValidationError(message: string): void {
        this.showFeedback = true;
        this.bannerFeedback = {
            type: FeedbackType.Error,
            title: "Validation error",
            message,
        };
    }

    public validate(queryType: QueryType | null, searchText: string): boolean {
        if (queryType === null || searchText.length === 0) {
            this.showValidationError("Query fields cannot be empty");
            return false;
        }

        if (
            searchText.length < 5 &&
            (queryType === QueryType.SMS || queryType === QueryType.Email)
        ) {
            this.showValidationError(
                `${queryType} must be minimum 5 characters`
            );
            return false;
        }

        if (queryType === QueryType.SMS) {
            const smsDigits = searchText.replace(/[^0-9 ()-]/g, "");
            if (searchText.length > smsDigits.length) {
                this.showValidationError("SMS must contain digits only");
                return false;
            }
        }

        if (queryType === QueryType.PHN) {
            const phnDigits = searchText.replace(/\D/g, "");
            var isValid = PHNValidator.IsValid(phnDigits);

            if (!isValid) {
                this.showValidationError("PHN is invalid");
                return false;
            }
        }

        return true;
    }

    public async handleSearch(): Promise<void> {
        this.showFeedback = false;
        this.users = [];
        this.expandedUsers = [];
        this.verifications = new Map();

        const queryType = this.selectedQueryType;
        let searchText =
            this.selectedQueryType === QueryType.PHN
                ? this.searchPhn
                : this.searchText;

        if (!this.validate(queryType, searchText) || queryType === null) {
            return;
        }

        if (queryType === QueryType.SMS) {
            searchText = searchText.replace(/[() -]/g, "");
        }

        try {
            this.usersLoading = true;
            const result = await this.supportService.getUsers(
                queryType,
                searchText
            );
            this.users = result.resourcePayload;

            // display error banner when status is ResultType.ActionRequired
            const error = result.resultError;
            if (error) {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: FeedbackType.Error,
                    title: "Error: " + error.errorCode,
                    message: "Message: " + error.resultMessage,
                };
            }
        } catch (error: unknown) {
            if (instanceOfResultError(error)) {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: FeedbackType.Error,
                    title: "Error: " + error.errorCode,
                    message: "Message: " + error.resultMessage,
                };
            }
        } finally {
            this.usersLoading = false;
        }
    }

    // eslint-disable-next-line @typescript-eslint/no-unused-vars
    public async handleUserRowExpanded(event: {
        item: User;
        value: boolean;
    }): Promise<void> {
        const { item: user, value: expanded } = event;
        if (expanded && !this.verifications.has(user.hdid)) {
            try {
                const result = await this.supportService.getVerifications(
                    user.hdid
                );
                this.verifications.set(user.hdid, result.resourcePayload);
                this.forceRerender();
            } catch (error: unknown) {
                if (instanceOfResultError(error)) {
                    this.showFeedback = true;
                    this.bannerFeedback = {
                        type: FeedbackType.Error,
                        title: "Error: " + error.errorCode,
                        message: "Message: " + error.resultMessage,
                    };
                }
            }
        }
    }

    public formatDateTime(date: StringISODateTime): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date, { isUtc: true }).format(
            DateWrapper.defaultDateTimeFormat
        );
    }

    public formatOptionalField(value: string | null | undefined) {
        return value ? value : "-";
    }

    public formatEmailOrSms(verification: MessagingVerification): string {
        if (verification.verificationType === VerificationType.Email) {
            return this.formatOptionalField(verification.email);
        }
        return this.formatOptionalField(verification.smsNumber);
    }
}
</script>

<template>
    <v-container>
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
            :position="snackbarPositionBottom"
        />
        <form @submit.prevent="handleSearch()">
            <v-row align="center">
                <v-col cols="4" sm="3" lg="2">
                    <v-select
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
        <v-data-table
            :headers="userTableHeaders"
            :items="users"
            :expanded.sync="expandedUsers"
            :loading="usersLoading"
            show-expand
            item-key="hdid"
            sort-by="lastLoginDateTime"
            must-sort
            :items-per-page="10"
            @item-expanded="handleUserRowExpanded"
        >
            <template #item.personalHealthNumber="{ item }">
                {{ formatOptionalField(item.personalHealthNumber) }}
            </template>
            <template #item.lastLoginDateTime="{ item }">
                {{ formatDateTime(item.lastLoginDateTime) }}
            </template>
            <template #expanded-item="{ headers, item }">
                <td :colspan="headers.length" class="py-3">
                    <h3 class="ma-4">Verification</h3>
                    <v-data-table
                        :key="componentKey"
                        :headers="verificationsTableHeaders"
                        :items="userVerifications(item.hdid)"
                        :items-per-page="10"
                        :loading="verificationsLoading(item.hdid)"
                        sort-by="updatedDateTime"
                        must-sort
                        class="ma-4"
                    >
                        <template #item.emailOrSms="{ item: v }">
                            {{ formatEmailOrSms(v) }}
                        </template>
                        <template #item.validated="{ item: v }">
                            {{ v.validated ? "Yes" : "No" }}
                        </template>
                        <template #item.updatedDateTime="{ item: v }">
                            {{ formatDateTime(v.updatedDateTime) }}
                        </template>
                        <template #item.smsValidationCode="{ item: v }">
                            {{ formatOptionalField(v.smsValidationCode) }}
                        </template>
                    </v-data-table>
                </td>
            </template>
        </v-data-table>
    </v-container>
</template>
