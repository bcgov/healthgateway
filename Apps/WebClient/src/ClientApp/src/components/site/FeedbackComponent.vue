<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { maxLength, minLength, required } from "@vuelidate/validators";
import { computed, ref, unref } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { ClientType } from "@/constants/clientType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { IUserFeedbackService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";
import ValidationUtil from "@/utility/validationUtil";

defineExpose({ showDialog });

const userFeedbackService = container.get<IUserFeedbackService>(
    SERVICE_IDENTIFIER.UserFeedbackService
);
const userStore = useUserStore();
const errorStore = useErrorStore();

const comment = ref("");
const visible = ref(false);
const hasSubmitted = ref(false);
const isSuccess = ref(false);
const isLoading = ref(false);

const validations = computed(() => ({
    comment: {
        required,
        minLength: minLength(5),
        maxLength: maxLength(500),
    },
}));

const v$ = useVuelidate(validations, { comment });

const user = computed(() => userStore.user);
const hasEmail = computed(
    () => user.value.verifiedEmail && user.value.hasEmail
);
const isValid = computed(() => ValidationUtil.isValid(v$.value));
const resultTitle = computed(() => {
    if (hasSubmitted.value) {
        return isSuccess.value ? "Received" : "Sorry";
    }

    return "";
});
const resultDescription = computed(() => {
    if (hasSubmitted.value) {
        return isSuccess.value
            ? "Your message has been sent successfully!"
            : "Your message could not be sent out!";
    }

    return "";
});
const commentErrors = computed(() =>
    v$.value.comment.$errors.map((e) => unref(e.$message))
);
const isSuccessWithoutEmail = computed(
    () => isSuccess.value && !hasEmail.value
);
const isSuccessWithEmail = computed(() => isSuccess.value && hasEmail.value);
const hasFailed = computed(() => !isSuccess.value && hasSubmitted.value);

function showDialog(): void {
    resetFeedback();
    visible.value = true;
}

function hideDialog(): void {
    visible.value = false;
}

function onSubmit(): void {
    isLoading.value = true;
    if (isValid.value !== true) {
        v$.value.$touch();
        isLoading.value = false;
        return;
    }

    userFeedbackService
        .submitFeedback(user.value.hdid, {
            comment: comment.value,
            clientType: ClientType.Web,
        })
        .then((result) => {
            isSuccess.value = result;
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            }
            isSuccess.value = false;
        })
        .finally(() => {
            hasSubmitted.value = true;
            isLoading.value = false;
        });
}

function resetFeedback(): void {
    hasSubmitted.value = false;
    isSuccess.value = false;
    comment.value = "";
    v$.value.$reset();
}
</script>

<template>
    <v-dialog
        id="feedback-container"
        v-model="visible"
        persistent
        no-click-animation
    >
        <form class="d-flex justify-center" @submit.prevent="onSubmit">
            <v-card max-width="700px">
                <v-card-title class="px-0">
                    <v-toolbar title="Feedback" density="compact" color="white">
                        <HgIconButtonComponent
                            data-testid="messageModalCloseButton"
                            icon="close"
                            aria-label="Close"
                            @click="hideDialog"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text v-if="!hasSubmitted" class="pa-4 text-body-1">
                    <p>
                        Do you have a suggestion or idea? Let us know in the
                        field below.
                    </p>
                    <v-textarea
                        id="comment"
                        v-model="comment"
                        counter
                        auto-grow
                        rows="5"
                        data-testid="feedback-comment-input"
                        placeholder="Describe your suggestion or idea..."
                        :error-messages="commentErrors"
                        :disabled="isSuccess || isLoading"
                        @input="v$.comment.$touch()"
                    />
                </v-card-text>
                <v-card-text v-else class="pa-4">
                    <div class="text-center text-body-1">
                        <v-icon
                            v-if="isSuccess"
                            color="success"
                            icon="check-circle"
                            size="48"
                        />
                        <v-icon
                            v-else
                            color="error"
                            icon="circle-xmark"
                            size="48"
                        />
                        <h3 class="text-h6 font-weight-bold pt-3">
                            {{ resultTitle }}
                        </h3>
                        <p class="py-3">
                            {{ resultDescription }}
                        </p>
                    </div>
                    <HgAlertComponent
                        v-if="isSuccessWithoutEmail"
                        type="warning"
                        variant="text"
                        :center-content="true"
                    >
                        <template #text>
                            We won't be able to respond to your message unless
                            you have a verified email address in your profile.
                        </template>
                    </HgAlertComponent>
                    <HgAlertComponent
                        v-if="hasFailed"
                        type="error"
                        variant="text"
                    >
                        <template #text>
                            If the problem persists please send your feedback to
                            <a
                                href="mailto:HealthGateway@gov.bc.ca"
                                class="text-link"
                            >
                                HealthGateway@gov.bc.ca
                            </a>
                        </template>
                    </HgAlertComponent>
                </v-card-text>
                <v-card-actions class="pa-4">
                    <v-spacer />
                    <template v-if="!isSuccess && !hasSubmitted">
                        <HgButtonComponent
                            data-testid="feedback-no-need-btn"
                            variant="secondary"
                            text="Cancel"
                            @click="hideDialog"
                        />
                        <HgButtonComponent
                            data-testid="send-feedback-message-btn"
                            :disabled="isValid !== true"
                            :loading="isLoading"
                            text="Send Message"
                            type="submit"
                        />
                    </template>
                    <template v-else-if="isSuccessWithoutEmail">
                        <HgButtonComponent
                            data-testid="feedback-no-need-btn"
                            variant="secondary"
                            text="No Need!"
                            @click="hideDialog"
                        />
                        <HgButtonComponent
                            data-testid="feedback-update-my-email-btn"
                            variant="primary"
                            text="Update my email"
                            to="/profile"
                            @click="hideDialog"
                        />
                    </template>
                    <!-- Single button States (centered) -->
                    <template v-else>
                        <HgButtonComponent
                            v-if="isSuccessWithEmail"
                            data-testid="feedback-got-it-btn"
                            text="Got it!"
                            variant="primary"
                            :loading="isLoading"
                            @click="hideDialog"
                        />
                        <HgButtonComponent
                            v-if="hasFailed"
                            text="Try Again"
                            variant="primary"
                            :loading="isLoading"
                            :disabled="isValid !== true"
                            type="submit"
                        />
                        <v-spacer />
                    </template>
                </v-card-actions>
            </v-card>
        </form>
    </v-dialog>
</template>
