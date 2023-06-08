<script setup lang="ts">
import VueCountdown from "@chenfengyuan/vue-countdown";
import { computed, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import type { WebClientConfiguration } from "@/models/configData";
import { DateWrapper } from "@/models/dateWrapper";
import { isTooManyRequestsError, ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

interface Props {
    smsNumber: string;
}
const props = defineProps<Props>();

const emit = defineEmits<{
    (e: "submit"): void;
    (e: "cancel"): void;
}>();

defineExpose({ showModal, hideModal });

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const store = useStore();

const tooManyRetries = ref(false);
const allowRetry = ref(false);
const smsVerificationSent = ref(false);
const smsVerificationCode = ref("");
const isVisible = ref(false);
const isLoading = ref(false);
const validationError = ref(false);
const error = ref(false);

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const smsResendDateTime = computed<DateWrapper>(
    () => store.getters["user/smsResendDateTime"]
);
const user = computed<User>(() => store.getters["user/user"]);

function addError(
    errorType: ErrorType,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addError", { errorType, source, traceId });
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function setTooManyRequestsWarning(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsWarning", { key });
}

function updateSMSResendDateTime(hdid: string, dateTime: DateWrapper): void {
    store.dispatch("user/updateSMSResendDateTime", { hdid, dateTime });
}

function showModal(): void {
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
        minutes: config.value.timeouts.resendSMS,
    });

    const now = new DateWrapper();
    allowRetry.value = smsTimeWhenEnabled.isBefore(now);
    if (!allowRetry.value) {
        const millisecondsToExpire = smsTimeWhenEnabled.diff(now).milliseconds;
        setTimeout(() => (allowRetry.value = true), millisecondsToExpire);
    }
}

function verifySMS(): void {
    smsVerificationCode.value = smsVerificationCode.value.replace(/\D/g, "");
    isLoading.value = true;
    userProfileService
        .validateSMS(user.value.hdid, smsVerificationCode.value)
        .then((result) => {
            validationError.value = !result;
            if (!validationError.value) {
                hideModal();
                emit("submit");
            }
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                setTooManyRequestsError("verifySmsModal");
            } else {
                error.value = true;
            }
        })
        .finally(() => {
            smsVerificationCode.value = "";
            isLoading.value = false;
        });
}

function sendUserSMSUpdate(): void {
    smsVerificationSent.value = true;
    updateSMSResendDateTime(user.value.hdid, new DateWrapper());
    userProfileService
        .updateSMSNumber(user.value.hdid, props.smsNumber)
        .then(() => setTimeout(() => (smsVerificationSent.value = false), 5000))
        .catch((error) => {
            if (isTooManyRequestsError(error)) {
                setTooManyRequestsWarning("page");
            } else {
                addError(ErrorType.Update, ErrorSourceType.Profile, undefined);
            }
        });
}

function getTimeout(): number {
    let resendTime: DateWrapper;
    if (!smsResendDateTime.value) {
        const now = new DateWrapper();
        updateSMSResendDateTime(user.value.hdid, now);
        resendTime = now;
    } else {
        resendTime = smsResendDateTime.value;
    }
    resendTime = resendTime.add(config.value.timeouts.resendSMS * 60 * 1000);
    return resendTime.diff(new DateWrapper()).milliseconds;
}

function onVerificationChange(): void {
    if (smsVerificationCode.value.length >= 6) {
        verifySMS();
    }
}

function formatPhoneNumber(phoneNumber: string): string | null {
    const cleaned = ("" + phoneNumber).replace(/\D/g, "");
    const match = cleaned.match(/^(\d{3})(\d{3})(\d{4})$/);
    if (match) {
        return "(" + match[1] + ") " + match[2] + "-" + match[3];
    }
    return null;
}

watch(smsResendDateTime, () => setResendTimeout());

setResendTimeout();
</script>

<template>
    <b-modal
        id="verify-sms-modal"
        v-model="isVisible"
        data-testid="verifySMSModal"
        title="Phone Verification"
        size="sm"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
        ok-only
        @show="getTimeout"
    >
        <b-row>
            <b-col>
                <form>
                    <TooManyRequestsComponent location="verifySmsModal" />
                    <b-row
                        v-if="error"
                        data-testid="verifySMSModalUnexpectedErrorText"
                    >
                        <b-col class="text-center">
                            An unexpected error has occurred. Please try
                            refreshing your browser or try again later.
                        </b-col>
                    </b-row>
                    <b-row
                        v-else-if="tooManyRetries"
                        data-testid="verifySMSModalErrorAttemptsText"
                    >
                        <b-col class="text-center">
                            Too many failed attempts.
                        </b-col>
                    </b-row>
                    <b-row v-else data-testid="verifySMSModalText">
                        <b-col>
                            <label
                                for="verificationCode-input"
                                class="text-center w-100"
                            >
                                Enter the verification code sent to <br />
                                <strong>{{
                                    formatPhoneNumber(smsNumber)
                                }}</strong>
                            </label>
                            <b-form-input
                                id="verificationCode-input"
                                v-model="smsVerificationCode"
                                data-testid="verifySMSModalCodeInput"
                                size="lg"
                                :autofocus="true"
                                class="text-center"
                                :state="validationError ? false : undefined"
                                max-length="6"
                                :disabled="isLoading"
                                required
                                @update="onVerificationChange"
                            />
                        </b-col>
                    </b-row>
                    <b-row v-if="!error && !tooManyRetries && validationError">
                        <b-col>
                            <span
                                class="text-danger"
                                data-testid="verifySMSModalErrorInvalidText"
                                >Invalid verification code. Try again.</span
                            >
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template v-if="!error" #modal-footer>
            <b-row class="w-100">
                <b-col v-if="!tooManyRetries && allowRetry">
                    Didn't receive a code?
                    <hg-button
                        id="resendSMSVerification"
                        variant="link"
                        class="m-0 p-0"
                        :disabled="smsVerificationSent"
                        @click="sendUserSMSUpdate()"
                    >
                        Resend
                    </hg-button>
                </b-col>
                <b-col v-if="!allowRetry">
                    <VueCountdown
                        v-slot="{ minutes, seconds }"
                        :time="getTimeout()"
                    >
                        Your code has been sent. You can resend after
                        <span data-testid="countdownText"
                            >{{ minutes > 0 ? minutes + "m" : "" }}
                            {{ seconds }}s</span
                        >
                    </VueCountdown>
                </b-col>
                <b-col v-if="tooManyRetries">
                    <hg-button
                        id="resendSMSVerification"
                        variant="link"
                        class="text-center w-100"
                        :disabled="smsVerificationSent"
                        @click="sendUserSMSUpdate()"
                    >
                        Send New Code
                    </hg-button>
                </b-col>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading" />
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.modal-footer {
    justify-content: flex-start;

    button {
        padding: 5px 20px 5px 20px;
    }
}
</style>
