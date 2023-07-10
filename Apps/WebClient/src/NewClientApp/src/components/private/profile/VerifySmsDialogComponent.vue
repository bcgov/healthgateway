<script setup lang="ts">
import VueCountdown from "@chenfengyuan/vue-countdown";
import { Mask } from "maska";
import { computed, ComputedRef, ref, watch } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import { isTooManyRequestsError, ResultError } from "@/models/errors";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";

interface Props {
    smsNumber: string;
}
const props = defineProps<Props>();

const emit = defineEmits<{
    (e: "verified"): void;
}>();

defineExpose({ showModal });

const mask = new Mask({ mask: "(###) ###-####" });

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const configStore = useConfigStore();
const errorStore = useErrorStore();
const userStore = useUserStore();

const tooManyRetries = ref(false);
const allowRetry = ref(false);
const smsVerificationSent = ref(false);
const smsVerificationCode = ref("");
const isVisible = ref(false);
const isLoading = ref(false);
const validationError = ref(false);
const unexpectedError = ref(false);

const smsResendDateTime: ComputedRef<IDateWrapper | undefined> = computed(
    () => userStore.smsResendDateTime
);
const user = computed(() => userStore.user);
const maskedSmsNumber = computed(() => mask.masked(props.smsNumber));
const errorMessages = computed(() =>
    validationError.value ? ["Invalid verification code. Try again."] : []
);

function showModal(): void {
    getTimeout();
    isVisible.value = true;
}

function hideModal(): void {
    isVisible.value = false;
}

function setResendTimeout(): void {
    if (smsResendDateTime.value === undefined) {
        allowRetry.value = true;
        return;
    }

    const smsTimeWhenEnabled = smsResendDateTime.value.add({
        minutes: configStore.webConfig.timeouts.resendSMS,
    });

    const now = new DateWrapper();
    allowRetry.value = smsTimeWhenEnabled.isBefore(now);
    if (!allowRetry.value) {
        const millisecondsToExpire = smsTimeWhenEnabled.diff(now).milliseconds;
        setTimeout(() => (allowRetry.value = true), millisecondsToExpire);
    }
}

function verifySms(): void {
    smsVerificationCode.value = smsVerificationCode.value.replace(/\D/g, "");
    isLoading.value = true;
    userProfileService
        .validateSms(user.value.hdid, smsVerificationCode.value)
        .then((result) => {
            validationError.value = !result;
            if (!validationError.value) {
                hideModal();
                emit("verified");
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("verifySmsModal");
            } else {
                unexpectedError.value = true;
            }
        })
        .finally(() => {
            smsVerificationCode.value = "";
            isLoading.value = false;
        });
}

function sendUserSmsUpdate(): void {
    smsVerificationSent.value = true;
    userStore.updateSmsResendDateTime(new DateWrapper());
    userProfileService
        .updateSmsNumber(user.value.hdid, props.smsNumber)
        .then(() => setTimeout(() => (smsVerificationSent.value = false), 5000))
        .catch((error) => {
            if (isTooManyRequestsError(error)) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                errorStore.addError(
                    ErrorType.Update,
                    ErrorSourceType.Profile,
                    undefined
                );
            }
        });
}

function getTimeout(): number {
    let resendTime: IDateWrapper;
    if (!smsResendDateTime.value) {
        const now = new DateWrapper();
        userStore.updateSmsResendDateTime(now);
        resendTime = now;
    } else {
        resendTime = smsResendDateTime.value;
    }
    resendTime = resendTime.add(
        configStore.webConfig.timeouts.resendSMS * 60 * 1000
    );
    return resendTime.diff(new DateWrapper()).milliseconds;
}

function onVerificationChange(): void {
    if (smsVerificationCode.value.length >= 6) {
        verifySms();
    }
}

watch(smsResendDateTime, () => setResendTimeout());

setResendTimeout();
</script>

<template>
    <LoadingComponent :is-loading="isLoading" />
    <v-dialog
        v-model="isVisible"
        data-testid="verifySMSModal"
        persistent
        no-click-animation
    >
        <div class="d-flex justify-center">
            <v-card :loading="isLoading" max-width="300px">
                <template #loader="{ isActive }">
                    <v-progress-linear
                        :active="isActive"
                        color="accent"
                        indeterminate
                    />
                </template>
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        title="Phone Verification"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            data-testid="messageModalCloseButton"
                            icon="close"
                            @click="hideModal"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-4">
                    <TooManyRequestsComponent location="verifySmsModal" />
                    <v-alert
                        v-if="unexpectedError"
                        data-testid="verifySMSModalUnexpectedErrorText"
                        class="d-print-none pa-0"
                        variant="text"
                        type="error"
                    >
                        An unexpected error has occurred. Please try refreshing
                        your browser or try again later.
                    </v-alert>
                    <div
                        v-else-if="tooManyRetries"
                        data-testid="verifySMSModalErrorAttemptsText"
                        class="text-center"
                    >
                        Too many failed attempts.
                    </div>
                    <div
                        v-else
                        data-testid="verifySMSModalText"
                        class="text-center"
                    >
                        <p class="text-body-1">
                            Enter the verification code sent to
                            <span class="font-weight-bold">
                                {{ maskedSmsNumber }}
                            </span>
                        </p>
                        <v-text-field
                            v-model="smsVerificationCode"
                            data-testid="verifySMSModalCodeInput"
                            label="Verification Code"
                            maxlength="6"
                            :disabled="isLoading"
                            :error-messages="errorMessages"
                            @update:model-value="onVerificationChange"
                        />
                    </div>
                </v-card-text>
                <v-card-actions
                    class="border-t-sm pa-4"
                    :class="{
                        'justify-center': !unexpectedError,
                        'justify-end': unexpectedError,
                    }"
                >
                    <HgButtonComponent
                        v-if="unexpectedError"
                        variant="primary"
                        text="OK"
                        @click.prevent="hideModal"
                    />
                    <HgButtonComponent
                        v-else-if="tooManyRetries"
                        id="resendSMSVerification"
                        variant="link"
                        block
                        text="Send New Code"
                        :disabled="smsVerificationSent"
                        @click="sendUserSmsUpdate"
                    />
                    <VueCountdown
                        v-else-if="!allowRetry"
                        v-slot="{ minutes, seconds }"
                        :time="getTimeout()"
                    >
                        Your code has been sent. You can resend after
                        <span data-testid="countdownText">
                            {{ minutes > 0 ? minutes + "m" : "" }}
                            {{ seconds }}s.
                        </span>
                    </VueCountdown>
                    <div v-else>
                        <span>Didn't receive a code?</span>
                        <HgButtonComponent
                            id="resendSMSVerification"
                            class="ml-2"
                            variant="link"
                            text="Resend"
                            :disabled="smsVerificationSent"
                            @click="sendUserSmsUpdate"
                        />
                    </div>
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
