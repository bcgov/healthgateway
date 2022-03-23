<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { ResultType } from "@/constants/resulttype";
import { UserRoles } from "@/constants/userRoles";
import BannerFeedback from "@/models/bannerFeedback";
import { DateWrapper, StringISODateTime } from "@/models/dateWrapper";
import UserFeedback, { AdminTag, UserFeedbackTag } from "@/models/userFeedback";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserFeedbackService } from "@/services/interfaces";
import store from "@/store/store";

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
    },
})
export default class FeedbackView extends Vue {
    private isLoading = true;
    private showFeedback = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: "",
    };

    private tableHeaders = [
        {
            text: "Date",
            value: "createdDateTime",
            width: "20%",
        },
        {
            text: "Email",
            value: "email",
            width: "20%",
        },
        {
            text: "Comments",
            value: "comment",
            width: "35%",
        },
        {
            text: "Tags",
            value: "tags",
            width: "20%",
        },
        {
            text: "Reviewed?",
            value: "isReviewed",
            width: "5%",
        },
    ];

    private adminTags: AdminTag[] = [];

    private selectedAdminTagIds: string[] = [];

    private feedbackList: UserFeedback[] = [];

    private userFeedbackService!: IUserFeedbackService;

    private focusedTags: UserFeedbackTag[] = [];

    private isLoadingTag = false;

    private get availableTags(): string[] {
        return this.adminTags.map<string>((x) => x.name);
    }

    private get filterIsActive(): boolean {
        return this.selectedAdminTagIds.length > 0;
    }

    private get filteredFeedbackList(): UserFeedback[] {
        if (this.selectedAdminTagIds.length === 0) {
            return this.feedbackList;
        }

        return this.feedbackList.filter(
            (userFeedback) =>
                userFeedback.tags.filter((feedbackTag) =>
                    this.selectedAdminTagIds.includes(feedbackTag.tag.id)
                ).length > 0
        );
    }

    private get canAccessSupport() {
        const userRoles: string[] = store.getters["auth/roles"];
        return userRoles.some((userRole) => userRole === UserRoles.Admin);
    }

    private mounted() {
        this.userFeedbackService = container.get(
            SERVICE_IDENTIFIER.UserFeedbackService
        );

        this.loadFeedbackList();
        this.loadAdminTags();
    }

    private loadFeedbackList() {
        this.userFeedbackService
            .getFeedbackList()
            .then((userFeedbacks) => {
                this.feedbackList = [];
                this.feedbackList.push(...userFeedbacks);
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error loading user feedbacks",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private loadAdminTags() {
        this.userFeedbackService
            .getAllTags()
            .then((adminTags) => {
                this.adminTags = adminTags;
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error loading admin tags",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private clearFilter(): void {
        this.selectedAdminTagIds = [];
    }

    private formatDateTime(date: StringISODateTime): string {
        if (!date) {
            return "";
        }
        return new DateWrapper(date, { isUtc: true }).format(
            DateWrapper.defaultDateTimeFormat
        );
    }

    private toggleReviewed(feedback: UserFeedback): void {
        this.isLoading = true;
        feedback.isReviewed = !feedback.isReviewed;
        this.userFeedbackService
            .toggleReviewed(feedback)
            .then(() => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Success,
                    title: "Feedback Reviewed",
                    message: "Successfully Reviewed User Feedback.",
                };
                this.loadFeedbackList();
            })
            .catch((err) => {
                // revert feedback item
                feedback.isReviewed = !feedback.isReviewed;
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Reviewing feedback failed, please try again.",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private createNewTag(feedbackItem: UserFeedback, tagName: string) {
        this.userFeedbackService
            .createTag(feedbackItem.id, tagName)
            .then((newTag) => {
                feedbackItem.tags.push(newTag);
                this.adminTags.push(newTag.tag);
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error creating tag",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoadingTag = false;
            });
    }

    private associateTag(feedbackItem: UserFeedback, tag: AdminTag) {
        this.userFeedbackService
            .associateTag(feedbackItem.id, tag)
            .then((newTag) => {
                feedbackItem.tags.push(newTag);
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error associating tag",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoadingTag = false;
            });
    }

    private removeTag(feedbackItem: UserFeedback, tag: UserFeedbackTag) {
        this.userFeedbackService
            .removeTag(feedbackItem.id, tag)
            .then((result) => {
                if (result) {
                    const feedbackIndex = feedbackItem.tags.findIndex(
                        (x) => x.id === tag.id
                    );
                    feedbackItem.tags.splice(feedbackIndex, 1);
                } else {
                    this.showFeedback = true;
                    this.bannerFeedback = {
                        type: ResultType.Error,
                        title: "Error",
                        message: "Error removing tag",
                    };
                }
            })
            .catch((err) => {
                this.showFeedback = true;
                this.bannerFeedback = {
                    type: ResultType.Error,
                    title: "Error",
                    message: "Error removing tag",
                };
                console.log(err);
            })
            .finally(() => {
                this.isLoadingTag = false;
            });
    }

    private isString(item: unknown): item is string {
        return typeof item === "string" || item instanceof String;
    }

    private onTagFocus(feedback: UserFeedback) {
        // Necessary to keep the feedback tags from being updated by the component.
        this.focusedTags = feedback.tags;
    }

    private onTagChange(
        input: (string | AdminTag)[],
        feedbackItem: UserFeedback
    ) {
        // Needs to be executed on the next render cycle to clear the selected tags
        // to avoid racing conditions on vuetify components.
        this.$nextTick(() => {
            // Reset the feedback tags until the backend updates it
            feedbackItem.tags = this.focusedTags;

            // Last entry on the input is the most recently selected item (guaranteed to be a string)
            const lastInputTag = input[input.length - 1] as string;

            // Look in the existing tags for a name match
            var foundIndex = this.adminTags.findIndex(
                (x) => x.name === lastInputTag
            );
            if (foundIndex >= 0) {
                const adminTag = this.adminTags[foundIndex];
                this.associateTag(feedbackItem, adminTag);
            } else {
                const newTag = lastInputTag;
                this.createNewTag(feedbackItem, newTag);
            }
        });
    }

    private getItemText(
        selected: UserFeedbackTag | string,
        item: UserFeedbackTag | string
    ) {
        if (selected === undefined || item === undefined) {
            return undefined;
        }

        if (this.isString(item)) {
            return item;
        }

        return item.tag.name;
    }

    private filter(item: string, queryText: string): boolean {
        return item.toLowerCase().includes(queryText.toLowerCase());
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
        <v-row justify="center">
            <v-col>
                <v-card>
                    <v-card-title>Filter</v-card-title>
                    <v-card-text>
                        <v-row align="center">
                            <v-col class="flex-grow-1 flex-shrink-0">
                                <v-select
                                    v-model="selectedAdminTagIds"
                                    :items="adminTags"
                                    item-text="name"
                                    item-value="id"
                                    label="Tags"
                                    multiple
                                />
                            </v-col>
                            <v-col class="flex-grow-0 flex-shrink-1">
                                <v-btn
                                    color="accent"
                                    :disabled="!filterIsActive"
                                    @click="clearFilter"
                                >
                                    Clear
                                </v-btn>
                            </v-col>
                        </v-row>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
        <v-row justify="center">
            <v-col no-gutters>
                <v-data-table
                    :headers="tableHeaders"
                    :items="filteredFeedbackList"
                    :items-per-page="50"
                    :footer-props="{
                        'items-per-page-options': [25, 50, 100, -1],
                    }"
                >
                    <template #item.createdDateTime="{ item }">
                        <span>{{ formatDateTime(item.createdDateTime) }}</span>
                    </template>
                    <template #item.email="{ item }">
                        <td>
                            <button
                                v-if="canAccessSupport"
                                class="mr-1"
                                @click="
                                    $router.push({
                                        path: '/support',
                                        query: { hdid: item.userProfileId },
                                    })
                                "
                            >
                                <v-icon>mdi-account-search</v-icon>
                            </button>
                            <span>{{ item.email }}</span>
                        </td>
                    </template>
                    <template #item.isReviewed="{ item }">
                        <td>
                            <v-btn
                                class="mx-2"
                                dark
                                small
                                icon
                                @click="toggleReviewed(item)"
                            >
                                <v-icon
                                    v-if="item.isReviewed"
                                    color="green"
                                    dark
                                    >fa-check</v-icon
                                >
                                <v-icon v-if="!item.isReviewed" color="red" dark
                                    >fa-times</v-icon
                                >
                            </v-btn>
                        </td>
                    </template>
                    <template #item.tags="{ item: feedback }">
                        <td>
                            <v-combobox
                                v-model="feedback.tags"
                                multiple
                                hide-selected
                                :items="availableTags"
                                :filter="filter"
                                :loading="isLoadingTag"
                                :item-text="getItemText"
                                @input="onTagChange($event, feedback)"
                                @focus="onTagFocus(feedback)"
                            >
                                <template #selection="{ item }">
                                    <template v-if="isString(item)">
                                        Loading...
                                    </template>
                                    <template v-else>
                                        <v-chip
                                            close
                                            @click:close="
                                                removeTag(feedback, item)
                                            "
                                        >
                                            {{ item.tag.name }}
                                        </v-chip>
                                    </template>
                                </template>
                                <template #item="{ item }">
                                    <v-chip>
                                        {{ item }}
                                    </v-chip>
                                </template>
                            </v-combobox>
                        </td>
                    </template>
                </v-data-table>
            </v-col>
        </v-row>
    </v-container>
</template>
