<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { Loader } from "@/constants/loader";
import { ResultType } from "@/constants/resulttype";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";

interface Props {
    inviteKey: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userStore = useUserStore();
const loadingStore = useLoadingStore();

const isLoading = computed(() => loadingStore.isLoading(Loader.ValidateEmail));
const validationPayload = ref<boolean>();
const resultStatus = ref<ResultType>();

const isVerified = computed(
    () =>
        resultStatus.value === ResultType.Success &&
        validationPayload.value === true
);
const isAlreadyVerified = computed(
    () =>
        resultStatus.value === ResultType.Error &&
        validationPayload.value === true
);
const isSuccess = computed(() => isAlreadyVerified.value || isVerified.value);
const validationFailed = computed(() => validationPayload.value === false);

// Created hook
loadingStore.applyLoader(
    Loader.ValidateEmail,
    "verifyingEmailAddress",
    userStore
        .validateEmail(props.inviteKey)
        .then((result) => {
            validationPayload.value = result.resourcePayload;
            resultStatus.value = result.resultStatus;
        })
        .catch(() => logger.error("Error while validating email."))
);
</script>

<template>
    <div class="text-center pa-4">
        <h4 v-if="isLoading" class="text-h5" data-testid="verifyingInvite">
            We are verifying your email...
        </h4>
        <template v-else>
            <v-icon
                :icon="isSuccess ? 'check-circle' : 'times-circle'"
                size="48"
                aria-hidden="true"
                :color="isSuccess ? 'success' : 'error'"
            />
            <h4 class="text-h6 my-4 pa-4">
                <span v-if="isVerified" data-testid="verifiedInvite"
                    >Your email address has been verified</span
                >
                <span
                    v-if="isAlreadyVerified"
                    data-testid="alreadyVerifiedInvite"
                    >Your email address is already verified</span
                >
                <span v-if="validationFailed" data-testid="expiredInvite">
                    Your link is expired or incorrect. Please resend
                    verification email from your profile page
                </span>
            </h4>
            <HgButtonComponent
                data-testid="continueButton"
                variant="primary"
                text="Continue"
                :to="isSuccess ? '/home' : '/profile'"
            />
        </template>
    </div>
</template>
