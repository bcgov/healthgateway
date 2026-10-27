<script setup lang="ts">
import { computed, onBeforeMount, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { Loader } from "@/constants/loader";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";
import PromiseUtility from "@/utility/promiseUtility";

interface Props {
    inviteKey: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userStore = useUserStore();
const loadingStore = useLoadingStore();

const isLoading = computed(() => loadingStore.isLoading(Loader.ValidateEmail));
const isVerified = ref<boolean>();
const isAlreadyVerified = ref<boolean>();

const isSuccess = computed(() => isAlreadyVerified.value || isVerified.value);
const validationFailed = computed(() => isVerified.value === false);

onBeforeMount(async () => {
    const loadingReason = "verifyingEmailAddress";
    loadingStore.setIsLoading(true, Loader.ValidateEmail, loadingReason);

    await PromiseUtility.runWithHandlers(
        () => userStore.validateEmail(props.inviteKey),
        (result) => {
            isVerified.value = result;
        },
        (error) => {
            if (error instanceof ResultError && error.statusCode === 409) {
                isAlreadyVerified.value = true;
            } else {
                logger.error("Error while validating email.");
            }
        },
        () => {
            loadingStore.setIsLoading(
                false,
                Loader.ValidateEmail,
                loadingReason
            );
        }
    );
});
</script>

<template>
    <div class="text-center pa-4">
        <h4
            v-if="isLoading"
            class="text-headline-small"
            data-testid="verifyingInvite"
        >
            We are verifying your email...
        </h4>
        <template v-else>
            <v-icon
                :icon="isSuccess ? 'check-circle' : 'times-circle'"
                size="48"
                aria-hidden="true"
                :color="isSuccess ? 'success' : 'error'"
            />
            <h4 class="text-title-large my-4 pa-4">
                <span v-if="isVerified" data-testid="verifiedInvite">
                    Your email address has been verified.
                    <br />
                    Health Gateway may now send you notifications.
                    <br />
                    You can change your preferences on your
                    <router-link
                        class="text-link"
                        data-testid="verified-profile-page-link"
                        to="/profile"
                        >Profile Page</router-link
                    >
                    at any time.
                </span>
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
