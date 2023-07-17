<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faTimesCircle,
} from "@fortawesome/free-solid-svg-icons";
import { computed, onMounted, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import { ResultType } from "@/constants/resulttype";
import RequestResult from "@/models/requestResult";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faCheckCircle, faTimesCircle);

interface Props {
    inviteKey: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const store = useStore();

const isLoading = ref(true);
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

const validationFailed = computed(() => validationPayload.value === false);

function validateEmail(inviteKey: string): Promise<RequestResult<boolean>> {
    return store.dispatch("user/validateEmail", { inviteKey });
}

function verifyEmail(): void {
    isLoading.value = true;
    validateEmail(props.inviteKey)
        .then((result) => {
            validationPayload.value = result.resourcePayload;
            resultStatus.value = result.resultStatus;
        })
        .catch(() => logger.error("Error while validating email."))
        .finally(() => (isLoading.value = false));
}

onMounted(() => {
    verifyEmail();
});
</script>

<template>
    <b-container class="text-center title pt-4">
        <h4 v-if="isLoading" data-testid="verifyingInvite">
            We are verifying your email...
        </h4>
        <div v-else-if="isVerified">
            <hg-icon
                icon="check-circle"
                size="extra-large"
                aria-hidden="true"
                class="text-success"
            />
            <h4 data-testid="verifiedInvite">
                Your email address has been verified
            </h4>
            <hg-button
                data-testid="continueButton"
                variant="primary"
                to="/home"
            >
                Continue
            </hg-button>
        </div>
        <div v-else-if="isAlreadyVerified">
            <hg-icon
                icon="check-circle"
                size="extra-large"
                aria-hidden="true"
                class="text-success"
            />
            <h4 data-testid="alreadyVerifiedInvite">
                Your email address is already verified
            </h4>
            <hg-button
                data-testid="continueButton"
                variant="primary"
                to="/home"
            >
                Continue
            </hg-button>
        </div>
        <div v-else-if="validationFailed">
            <hg-icon
                icon="times-circle"
                size="large"
                aria-hidden="true"
                class="text-danger"
            />
            <h4 data-testid="expiredInvite">
                Your link is expired or incorrect. Please resend verification
                email from your profile page
            </h4>
            <hg-button
                data-testid="continueButton"
                variant="primary"
                @click="$router.push({ path: '/profile' })"
            >
                Continue
            </hg-button>
        </div>
    </b-container>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.title {
    color: $primary;
    font-size: 2.1em;
}
</style>
