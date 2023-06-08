<script setup lang="ts">
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
import { computed, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { IUserFeedbackService } from "@/services/interfaces";

library.add(faAngleDown, faComments, faExclamationCircle);

const userFeedbackService = container.get<IUserFeedbackService>(
    SERVICE_IDENTIFIER.UserFeedbackService
);
const store = useStore();

const comment = ref("");
const visible = ref(false);
const hasSubmitted = ref(false);
const isSuccess = ref(false);
const isLoading = ref(false);

const isSidebarOpen = computed<boolean>(
    () => store.getters["navbar/isSidebarOpen"]
);
const isSidebarAnimating = computed<boolean>(
    () => store.getters["navbar/isSidebarAnimating"]
);
const user = computed<User>(() => store.getters["user/user"]);

const isSidebarFullyOpen = computed<boolean>(
    () => isSidebarOpen.value && !isSidebarAnimating.value
);
const feedbackMaximized = computed<boolean>(
    () => isSidebarFullyOpen.value && visible.value
);
const hasEmail = computed<boolean>(
    () => user.value.verifiedEmail && user.value.hasEmail
);
const isValid = computed<boolean>(() => comment.value.length > 1);
const resultTitle = computed<string>(() => {
    if (hasSubmitted.value) {
        return isSuccess.value ? "Awesome!" : "Oh no!";
    }

    return "";
});
const resultIcon = computed<IconDefinition>(() =>
    isSuccess.value ? farCheckCircle : farTimesCircle
);
const resultDescription = computed<string>(() => {
    if (hasSubmitted.value) {
        return isSuccess.value
            ? "Your message has been sent successfully!"
            : "Your message could not be sent out!";
    }

    return "";
});

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function toggleExpanded(): void {
    visible.value = !visible.value;
    if (!isSidebarOpen.value) {
        store.dispatch("navbar/toggleSidebar");
    }
}

function onSubmit(): void {
    isLoading.value = true;

    userFeedbackService
        .submitFeedback(user.value.hdid, {
            comment: comment.value,
        })
        .then((result) => {
            isSuccess.value = result;
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            }
            isSuccess.value = false;
        })
        .finally(() => {
            hasSubmitted.value = true;
            isLoading.value = false;
        });
}

function resetFeedback(): void {
    visible.value = false;
    hasSubmitted.value = false;
    isSuccess.value = false;
    comment.value = "";
}

watch(isSidebarOpen, (newValue: boolean) => {
    // Make sure it closes if the sidebar is closing and reset state
    if (!newValue) {
        resetFeedback();
    }
});
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
