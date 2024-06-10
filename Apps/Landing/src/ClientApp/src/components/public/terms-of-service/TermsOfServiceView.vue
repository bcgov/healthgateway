<script setup lang="ts">
import { ref } from "vue";

import HtmlTextAreaComponent from "@/components/common/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { ILogger, IUserProfileService } from "@/services/interfaces";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);

const isLoading = ref(true);
const hasTooManyRequestsError = ref(false);
const hasOtherError = ref(false);
const termsOfService = ref("");

userProfileService
    .getTermsOfService()
    .then((result) => {
        logger.debug("Terms Of Service retrieved: " + JSON.stringify(result));
        termsOfService.value = result.content;
    })
    .catch((err: ResultError) => {
        logger.error(err.message);
        if (err.statusCode === 429) {
            hasTooManyRequestsError.value = true;
        } else {
            hasOtherError.value = true;
        }
    })
    .finally(() => {
        isLoading.value = false;
    });
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading" />
        <PageTitleComponent title="Terms of Service" />
        <v-alert
            v-if="hasTooManyRequestsError || hasOtherError"
            variant="outlined"
            border
            type="error"
            title="Error"
        >
            <template #text>
                <p v-if="hasTooManyRequestsError" class="text-body-1">
                    Unable to complete action as the site is too busy. Please
                    try again later.
                </p>
                <p v-else class="text-body-1">
                    An unexpected error occured while processing the request.
                    Please refresh your browser.
                </p>
            </template>
        </v-alert>
        <HtmlTextAreaComponent
            v-if="!isLoading && termsOfService"
            :input="termsOfService"
        />
    </div>
</template>
