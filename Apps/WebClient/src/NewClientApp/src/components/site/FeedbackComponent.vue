<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { maxLength, minLength, required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import { IUserFeedbackService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";

defineExpose({
    showDialog,
});

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

const user = computed<User>(() => userStore.user);
const hasEmail = computed<boolean>(
    () => user.value.verifiedEmail && user.value.hasEmail
);
const isInvalid = computed<boolean>(
    () => !v$.value.$dirty || (v$.value.$dirty && v$.value.$invalid)
);
const resultTitle = computed<string>(() => {
    if (hasSubmitted.value) {
        return isSuccess.value ? "Received" : "Sorry!";
    }

    return "";
});
const resultDescription = computed<string>(() => {
    if (hasSubmitted.value) {
        return isSuccess.value
            ? "Your message has been sent successfully!"
            : "Your message could not be sent out!";
    }

    return "";
});

function showDialog(): void {
    visible.value = true;
}

function hideDialog(): void {
    visible.value = false;
}

function onSubmit(): void {
    isLoading.value = true;
    if (isInvalid.value) {
        v$.value.$touch();
        isLoading.value = false;
        return;
    }

    userFeedbackService
        .submitFeedback(user.value.hdid, {
            comment: comment.value,
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

const commentErrors = computed(() =>
    v$.value.comment.$errors.map((e) =>
        typeof e.$message === "string" ? e.$message : e.$message.value
    )
);

const isSuccessWithoutEmail = computed<boolean>(
    () => isSuccess.value && !hasEmail.value
);

const isSuccessWithEmail = computed<boolean>(
    () => isSuccess.value && hasEmail.value
);

function resetFeedback(): void {
    visible.value = false;
    // begin closing before changing state
    setTimeout(() => {
        hasSubmitted.value = false;
        isSuccess.value = false;
        comment.value = "";
        v$.value.$reset();
    }, 500);
}
</script>

<template>
    <v-dialog
        id="feedback-container"
        v-model="visible"
        data-testid="feedbackContainer"
        persistent
        no-click-animation
    >
        <v-form
            v-model="isInvalid"
            class="d-flex justify-center"
            @submit.prevent="onSubmit"
        >
            <v-card max-width="700px">
                <v-card-title class="px-0">
                    <v-toolbar title="Feedback" density="compact" color="white">
                        <HgIconButtonComponent
                            data-testid="messageModalCloseButton"
                            icon="close"
                            @click="hideDialog"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text v-if="!hasSubmitted" class="text-body-1">
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
                        data-testid="feedbackCommentInput"
                        placeholder="Describe your suggestion or idea..."
                        :error-messages="commentErrors"
                        :disabled="isSuccess || isLoading"
                        @input="v$.comment.$touch()"
                    />
                </v-card-text>
                <v-card-text v-else>
                    <div class="text-center text-body-1">
                        <v-icon
                            v-if="isSuccess"
                            color="success"
                            icon="check-circle"
                            size="48"
                            data-testid="feedbackSuccessIcon"
                        />
                        <v-icon
                            v-else
                            color="error"
                            icon="circle-xmark"
                            size="48"
                            data-testid="feedbackErrorIcon"
                        />
                        <h3 class="text-h6 font-weight-bold pt-3">
                            {{ resultTitle }}
                        </h3>
                        <p class="py-3">
                            {{ resultDescription }}
                        </p>
                    </div>
                    <div>
                        <v-alert
                            v-if="isSuccessWithoutEmail"
                            color="white"
                            class="text-body-2"
                        >
                            <template #prepend>
                                <v-icon
                                    color="warning"
                                    icon="circle-exclamation"
                                    size="small"
                                />
                            </template>
                            <template #text>
                                <p>
                                    We won't be able to respond to your message
                                    unless you have a verified email address in
                                    your profile.
                                </p>
                                <p>
                                    If the problem persists please send your
                                    feedback to
                                    <a href="mailto:HealthGateway@gov.bc.ca">
                                        HealthGateway@gov.bc.ca
                                    </a>
                                </p>
                            </template>
                        </v-alert>
                    </div>
                </v-card-text>
                <v-card-actions class="border-t-sm pa-4 d-flex justify-end">
                    <HgButtonComponent
                        v-if="!isSuccess && !isLoading && !hasSubmitted"
                        data-testid="sendFeedbackMessageBtn"
                        :disabled="isInvalid"
                        :loading="isLoading"
                        text="Send Message"
                        type="submit"
                    />
                    <HgButtonComponent
                        v-if="isSuccessWithoutEmail"
                        data-testid="noNeedBtn"
                        variant="link"
                        text="No Need!"
                        @click="resetFeedback"
                    />
                    <HgButtonComponent
                        v-if="isSuccessWithoutEmail"
                        data-testid="updateMyEmailButton"
                        variant="primary"
                        text="Update my email"
                        to="/profile"
                        @click="resetFeedback"
                    />
                    <HgButtonComponent
                        v-if="isSuccessWithEmail"
                        data-testid="hasEmailResetFeedbackBtn"
                        text="Got it!"
                        variant="primary"
                        :loading="isLoading"
                        :disabled="isLoading"
                        @click="resetFeedback"
                    />
                    <HgButtonComponent
                        v-if="!isSuccess && hasSubmitted"
                        data-testid="tryAgainBtn"
                        text="Try Again"
                        variant="primary"
                        :loading="isLoading"
                        :disabled="isLoading || isInvalid"
                        type="submit"
                    />
                </v-card-actions>
            </v-card>
        </v-form>
    </v-dialog>
</template>
