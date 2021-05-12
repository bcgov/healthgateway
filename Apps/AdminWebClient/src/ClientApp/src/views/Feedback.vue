<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
        ></BannerFeedbackComponent>
        <v-row justify="center">
            <v-col no-gutters>
                <v-data-table
                    :headers="tableHeaders"
                    :items="feedbackList"
                    :items-per-page="50"
                    :footer-props="{
                        'items-per-page-options': [25, 50, 100, -1],
                    }"
                >
                    <template #item.createdDateTime="{ item }">
                        <span>{{ formatDate(item.createdDateTime) }}</span>
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
                                :items="adminTags"
                                :filter="filter"
                                :loading="isLoadingTag"
                                item-text="name"
                                @input="onTagChange($event, feedback)"
                                @focus="onTagFocus(feedback)"
                            >
                                <template #selection="{ item }">
                                    <v-chip>
                                        {{ item.name }}
                                    </v-chip>
                                </template>
                                <template #item="{ item }">
                                    <v-chip>
                                        {{ item.name }}
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

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import LoadingComponent from "@/components/core/Loading.vue";
import { ResultType } from "@/constants/resulttype";
import BannerFeedback from "@/models/bannerFeedback";
import UserFeedback, { AdminTag } from "@/models/userFeedback";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserFeedbackService } from "@/services/interfaces";

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

    private feedbackList: UserFeedback[] = [];

    private userFeedbackService!: IUserFeedbackService;

    private focusedTags: AdminTag[] = [];

    private isLoadingTag = false;

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

    private formatDate(date: Date): string {
        return new Date(Date.parse(date + "Z")).toLocaleString();
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

    private onTagFocus(feedback: UserFeedback) {
        // Necessary to keep the feedback tags from being updated by the component.
        this.focusedTags = feedback.tags;
    }

    private onTagChange(
        input: (string | AdminTag)[],
        feedbackItem: UserFeedback
    ) {
        // Needs to be executed on the next render cycle to avoid racing conditions on vuetify components.
        this.$nextTick(() => {
            // Reset the feedback tags until the backend updates it
            feedbackItem.tags = this.focusedTags;

            // Last entry on the input is the most recently selected item (could be text)
            const lastTag = input[input.length - 1];

            var newTag: string | AdminTag = lastTag;

            // Look for the existing tags for a name match
            if (typeof lastTag === "string") {
                var foundIndex = this.adminTags.findIndex(
                    (x) => x.name === lastTag
                );
                if (foundIndex > 0) {
                    newTag = this.adminTags[foundIndex];
                }
            }

            this.isLoadingTag = true;
            if (typeof newTag === "string") {
                this.createNewTag(feedbackItem, newTag);
            } else {
                this.associateTag(feedbackItem, newTag);
            }
        });
    }

    private createNewTag(feedbackItem: UserFeedback, newTag: string) {
        this.userFeedbackService
            .createTag(feedbackItem.id, newTag)
            .then((newTag) => {
                feedbackItem.tags.push(newTag);
                this.adminTags.push(newTag);
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

    private associateTag(feedbackItem: UserFeedback, newTag: AdminTag) {
        this.userFeedbackService
            .associateTag(feedbackItem.id, newTag)
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

    private filter(item: AdminTag, queryText: string): boolean {
        return item.name.toLowerCase().includes(queryText.toLowerCase());
    }
}
</script>
