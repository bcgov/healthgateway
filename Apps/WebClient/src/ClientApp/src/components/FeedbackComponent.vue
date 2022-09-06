<script lang="ts">
import { IconDefinition, library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle as farCheckCircle,
    faTimesCircle as farTimesCircle,
} from "@fortawesome/free-regular-svg-icons";
import {
    faAngleDown,
    faComments,
    faExclamationCircle,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IUserFeedbackService } from "@/services/interfaces";

library.add(faAngleDown, faComments, faExclamationCircle);

const navbar = "navbar";
const user = "user";

@Component
export default class FeedbackComponent extends Vue {
    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    @Action("toggleSidebar", { namespace: navbar })
    toggleSidebar!: () => void;

    @Getter("isSidebarOpen", { namespace: navbar })
    isSidebarOpen!: boolean;

    @Getter("isSidebarAnimating", { namespace: "navbar" })
    isSidebarAnimating!: boolean;

    @Getter("user", { namespace: user })
    user!: User;

    private comment = "";

    private visible = false;
    private hasSubmitted = false;
    private isSuccess = false;
    private isLoading = false;
    private userFeedbackService!: IUserFeedbackService;

    private get isSidebarFullyOpen(): boolean {
        return this.isSidebarOpen && !this.isSidebarAnimating;
    }

    private get feedbackMaximized(): boolean {
        return this.isSidebarFullyOpen && this.visible;
    }

    private get isValid(): boolean {
        return this.comment.length > 1;
    }

    private get resultTitle(): string {
        if (this.hasSubmitted) {
            return this.isSuccess ? "Awesome!" : "Oh no!";
        }

        return "";
    }

    private get resultIcon(): IconDefinition {
        return this.isSuccess ? farCheckCircle : farTimesCircle;
    }

    private get resultDescription(): string {
        if (this.hasSubmitted) {
            return this.isSuccess
                ? "Your message has been sent successfully!"
                : "Your Message could not be sent out!";
        }

        return "";
    }

    private get hasEmail(): boolean {
        return this.user.verifiedEmail && this.user.hasEmail;
    }

    @Watch("isSidebarOpen")
    private onIsSidebarOpen(newValue: boolean): void {
        // Make sure it closes if the sidebar is closing and reset state
        if (!newValue) {
            this.resetFeedback();
        }
    }

    private created(): void {
        this.userFeedbackService = container.get(
            SERVICE_IDENTIFIER.UserFeedbackService
        );
    }

    private toggleExpanded(): void {
        this.visible = !this.visible;
        if (!this.isSidebarOpen) {
            this.toggleSidebar();
        }
    }

    private onSubmit(): void {
        this.isLoading = true;

        this.userFeedbackService
            .submitFeedback(this.user.hdid, {
                comment: this.comment,
            })
            .then((result) => {
                this.isSuccess = result;
            })
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                }
                this.isSuccess = false;
            })
            .finally(() => {
                this.hasSubmitted = true;
                this.isLoading = false;
            });
    }

    private resetFeedback(): void {
        this.visible = false;
        this.hasSubmitted = false;
        this.isSuccess = false;
        this.comment = "";
    }
}
</script>

<template>
    <div
        id="feedback-container"
        class="d-flex flex-column text-dark"
        data-testid="feedbackContainer"
    >
        <b-button
            data-testid="expandFeedbackBtn"
            class="justify-content-center feedback px-0 py-2 rounded-0"
            :class="{
                'bg-danger': hasSubmitted && !isSuccess,
                'bg-success': hasSubmitted && isSuccess,
            }"
            :aria-expanded="visible ? 'true' : 'false'"
            aria-controls="collapse"
            size="sm"
            @click="toggleExpanded"
        >
            <b-row v-show="!hasSubmitted" no-gutters class="px-2 px-md-3">
                <b-col class="text-nowrap">
                    <hg-icon
                        icon="comments"
                        size="large"
                        square
                        aria-hidden="true"
                    />
                    <span
                        v-show="isSidebarFullyOpen"
                        class="px-2 px-md-3"
                        :class="{
                            'mr-3': !visible,
                        }"
                        >Feedback</span
                    >
                </b-col>
                <b-col
                    cols="auto"
                    class="align-items-center"
                    :class="{
                        'd-flex': feedbackMaximized,
                        'd-none': !feedbackMaximized,
                    }"
                >
                    <hg-icon
                        icon="angle-down"
                        size="medium"
                        square
                        aria-hidden="true"
                    />
                </b-col>
            </b-row>
            <b-row v-show="hasSubmitted" no-gutters>
                <b-col>
                    <hg-icon
                        :icon="resultIcon"
                        size="large"
                        aria-hidden="true"
                    />
                </b-col>
            </b-row>
        </b-button>
        <b-collapse id="collapse" v-model="visible" class="input-container">
            <div v-show="isSidebarOpen" class="p-3">
                <div v-if="!hasSubmitted">
                    <b-row no-gutters class="description-container">
                        <p v-show="isSidebarFullyOpen" class="small">
                            Do you have a suggestion or idea? Let us know in the
                            field below.
                        </p>
                    </b-row>
                    <b-form ref="feedbackForm" @submit.prevent="onSubmit">
                        <b-row no-gutters class="mb-3">
                            <b-col>
                                <b-form-textarea
                                    id="comment"
                                    v-model="comment"
                                    data-testid="feedbackCommentInput"
                                    size="sm"
                                    placeholder="Describe your suggestion or idea..."
                                    rows="5"
                                    max-rows="3"
                                    maxlength="500"
                                    class="border-0 p-2"
                                    :disabled="isSuccess || isLoading"
                                />
                            </b-col>
                        </b-row>
                        <b-row no-gutters class="submit-button-container">
                            <b-col class="d-flex justify-content-center">
                                <b-button
                                    v-if="!isSuccess && !isLoading"
                                    data-testid="sendFeedbackMessageBtn"
                                    size="md"
                                    block
                                    class="action-button aqua-button border-0 px-3"
                                    type="submit"
                                    :disabled="!isValid"
                                >
                                    Send Message
                                </b-button>
                                <b-spinner v-if="isLoading"></b-spinner>
                            </b-col>
                        </b-row>
                    </b-form>
                </div>
                <!-- Request result section -->
                <div v-else v-show="isSidebarFullyOpen">
                    <div class="text-center">
                        <p class="font-weight-bold">
                            {{ resultTitle }}
                        </p>
                        <p class="small">
                            {{ resultDescription }}
                        </p>
                    </div>
                    <b-row
                        v-if="isSuccess && !hasEmail"
                        no-gutters
                        class="mt-4 mb-3 mb-md-4 px-md-3 small"
                    >
                        <b-col cols="auto">
                            <hg-icon
                                icon="exclamation-circle"
                                size="medium"
                                aria-hidden="true"
                                class="text-warning mt-1 mr-2 mr-md-3"
                            />
                        </b-col>
                        <b-col>
                            <span class="text-left">
                                We won't be able to respond to your message
                                unless you have a verified email address in your
                                profile.
                            </span>
                        </b-col>
                    </b-row>
                    <!-- Result buttons -->
                    <b-row
                        v-if="isSuccess && !hasEmail"
                        no-gutters
                        class="justify-content-center"
                    >
                        <b-col class="mx-2 mb-2" cols="auto">
                            <b-button
                                data-testid="noNeedBtn"
                                variant="link"
                                size="sm"
                                @click="resetFeedback"
                            >
                                No Need!
                            </b-button>
                        </b-col>
                        <b-col class="mx-2 mb-2" cols="auto">
                            <router-link
                                id="menuBtnProfile"
                                data-testid="menuBtnProfileLink"
                                to="/profile"
                            >
                                <b-button
                                    data-testid="updateMyEmailButton"
                                    size="sm"
                                    class="aqua-button border-0"
                                >
                                    Update my email
                                </b-button>
                            </router-link>
                        </b-col>
                    </b-row>
                    <div v-else class="mb-2 d-flex justify-content-center">
                        <b-button
                            v-if="isSuccess && hasEmail"
                            data-testid="hasEmailResetFeedbackBtn"
                            size="md"
                            block
                            class="action-button bg-success px-3 border-0"
                            :disabled="isLoading"
                            @click="resetFeedback"
                        >
                            Got it!
                        </b-button>
                        <b-button
                            v-if="!isSuccess"
                            data-testid="tryAgainBtn"
                            size="md"
                            block
                            class="action-button bg-danger px-3 border-0"
                            :disabled="isLoading"
                            @click="onSubmit"
                        >
                            Try Again
                        </b-button>
                    </div>
                </div>
            </div>
        </b-collapse>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.feedback {
    background-color: $aquaBlue;
    display: inline;
    font-size: 1.3em;
    border: 0px;
    transition: all 0.3s;
}

.input-container {
    background-color: $lightGrey;
    border: 0px;
    transition: all 0.3s;
}

.aqua-button {
    background-color: $aquaBlue;
}

.action-button {
    max-width: 160px;
}

.description-container {
    min-height: 58px;
}

.submit-button-container {
    height: 36px;
    overflow: hidden;
}
</style>
