<script setup lang="ts">
import { onMounted, ref } from "vue";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { ResultError } from "@/models/errors";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER, STORE_IDENTIFIER } from "@/plugins/inversify";
import {
    ILogger,
    IStoreProvider,
    IUserProfileService,
} from "@/services/interfaces";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const storeProvider = container.get<IStoreProvider>(
    STORE_IDENTIFIER.StoreProvider
);
const store = storeProvider.getStore();
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Terms of Service",
        to: "/termsOfService",
        active: true,
        dataTestId: "breadcrumb-terms-of-service",
    },
];

const isLoading = ref(true);
const hasErrors = ref(false);
const errorMessage = ref("");
const termsOfService = ref("");

onMounted(() => {
    loadTermsOfService();
});

function setTooManyRequestsWarning(params: { key: string }): void {
    store.dispatch("errorBanner/setTooManyRequestsWarning", params);
}

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
                setTooManyRequestsWarning({ key: "page" });
            } else {
                hasErrors.value = true;
                errorMessage.value = "Please refresh your browser.";
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <b-alert :show="hasErrors" dismissible variant="danger">
            <h4>Error</h4>
            <p>An unexpected error occured while processing the request:</p>
            <span>{{ errorMessage }}</span>
        </b-alert>
        <page-title title="Terms of Service" />
        <HtmlTextAreaComponent v-if="!isLoading" :input="termsOfService" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#termsOfService {
    background-color: $light_background;
    color: $soft_text;
}
</style>
