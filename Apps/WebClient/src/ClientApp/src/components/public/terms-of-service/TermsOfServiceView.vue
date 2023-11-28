<script setup lang="ts">
import { ref } from "vue";

import HtmlTextAreaComponent from "@/components/common/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import BreadcrumbComponent from "@/components/common/BreadcrumbComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { ResultError } from "@/models/errors";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Terms of Service",
        to: "/termsOfService",
        active: true,
        dataTestId: "breadcrumb-terms-of-service",
    },
];

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);

const errorStore = useErrorStore();

const isLoading = ref(true);
const hasErrors = ref(false);
const termsOfService = ref("");

userProfileService
    .getTermsOfService()
    .then((result) => {
        logger.debug("Terms Of Service retrieved: " + JSON.stringify(result));
        termsOfService.value = result.content;
    })
    .catch((err: ResultError) => {
        logger.error(err.resultMessage);
        if (err.statusCode === 429) {
            errorStore.setTooManyRequestsWarning("page");
        } else {
            hasErrors.value = true;
        }
    })
    .finally(() => {
        isLoading.value = false;
    });
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <PageTitleComponent title="Terms of Service" />
        <v-alert
            v-if="hasErrors"
            variant="outlined"
            border
            closable
            type="error"
            title="Error"
        >
            <template #text>
                <p class="text-body-1">
                    An unexpected error occured while processing the request.
                    Please refresh your browser.
                </p>
            </template>
        </v-alert>
        <HtmlTextAreaComponent v-if="!isLoading" :input="termsOfService" />
    </div>
</template>
