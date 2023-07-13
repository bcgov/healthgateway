<script setup lang="ts">
import { onMounted, ref } from "vue";

import HtmlTextAreaComponent from "@/components/common/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
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
const errorMessage = ref("");
const termsOfService = ref("");

function loadTermsOfService(): void {
    isLoading.value = true;
    userProfileService
        .getTermsOfService()
        .then((result) => {
            logger.debug(
                "Terms Of Service retrieved: " + JSON.stringify(result)
            );
            termsOfService.value = result.content;
        })
        .catch((err: ResultError) => {
            logger.error(err.resultMessage);
            if (err.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                hasErrors.value = true;
                errorMessage.value = "Please refresh your browser.";
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

onMounted(() => {
    loadTermsOfService();
});
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <PageTitleComponent title="Terms of Service" />
        <v-alert v-if="hasErrors" closable type="error" title="Error">
            <template #text>
                <p class="text-body-1">
                    An unexpected error occured while processing the request:
                </p>
                <p class="text-body-1">{{ errorMessage }}</p>
            </template>
        </v-alert>
        <HtmlTextAreaComponent v-if="!isLoading" :input="termsOfService" />
    </div>
</template>
