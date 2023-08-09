<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HtmlTextAreaComponent from "@/components/common/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const router = useRouter();
const userStore = useUserStore();
const errorStore = useErrorStore();

const isLoading = ref(true);
const termsOfServiceId = ref("");
const termsOfService = ref("");
const accepted = ref(false);

function onSubmit(): void {
    if (accepted.value !== true) {
        return;
    }

    isLoading.value = true;

    userStore
        .updateAcceptedTerms(termsOfServiceId.value)
        .then(() => redirect())
        .catch((err: ResultError) => {
            logger.error(
                `Error updating accepted terms of service: ${JSON.stringify(
                    err
                )}`
            );
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsError("page");
            } else {
                errorStore.addError(
                    ErrorType.Update,
                    ErrorSourceType.Profile,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

function redirect(): void {
    logger.debug("Redirecting to /home");
    router.push({ path: "/home" });
}

// Created hook
userProfileService
    .getTermsOfService()
    .then((result) => {
        logger.debug(`getTermsOfService result: ${JSON.stringify(result)}`);
        termsOfServiceId.value = result.id;
        termsOfService.value = result.content;
    })
    .catch((err: ResultError) => {
        logger.error(err.resultMessage);
        if (err.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            errorStore.addError(
                ErrorType.Retrieve,
                ErrorSourceType.TermsOfService,
                undefined
            );
        }
    })
    .finally(() => {
        isLoading.value = false;
    });
</script>

<template>
    <LoadingComponent :is-loading="isLoading" />
    <PageTitleComponent
        title="Update to our Terms of Service"
        data-testid="tos-page-title"
    />
    <template v-if="!isLoading && termsOfService !== ''">
        <HtmlTextAreaComponent
            class="termsOfService mb-4"
            :input="termsOfService"
            data-testid="tos-text-area-component"
        />
        <div class="mb-4">
            <v-checkbox
                id="accept-tos-checkbox"
                v-model="accepted"
                color="primary"
                data-testid="accept-tos-checkbox"
                label="I agree to the terms of service above"
                :error-messages="
                    accepted ? [] : ['You must accept the terms of service.']
                "
            />
        </div>
        <div class="text-right">
            <HgButtonComponent
                data-testid="continue-btn"
                variant="primary"
                :disabled="!accepted"
                text="Continue"
                @click="onSubmit"
            />
        </div>
    </template>
</template>

<style scoped lang="scss">
.termsOfService {
    overflow-y: scroll;
    max-height: 460px;
}
</style>
