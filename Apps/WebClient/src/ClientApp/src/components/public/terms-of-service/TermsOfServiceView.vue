<script setup lang="ts">
import { onBeforeMount, ref } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
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
import PromiseUtility from "@/utility/promiseUtility";

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

onBeforeMount(async () => {
    await PromiseUtility.runWithHandlers(
        () => userProfileService.getTermsOfService(),
        (result) => {
            logger.debug(
                "Terms Of Service retrieved: " + JSON.stringify(result)
            );
            termsOfService.value = result.content;
        },
        (error) => {
            const resultError =
                error instanceof ResultError ? error : undefined;
            logger.error(
                resultError?.message ??
                    (error instanceof Error ? error.message : String(error))
            );
            if (resultError?.statusCode === 429) {
                errorStore.setTooManyRequestsWarning("page");
            } else {
                hasErrors.value = true;
            }
        },
        () => {
            isLoading.value = false;
        }
    );
});
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <PageTitleComponent title="Terms of Service" />
        <HgAlertComponent
            v-if="hasErrors"
            variant="outlined"
            closable
            type="error"
            title="Error"
        >
            <template #text>
                <p class="text-body-large">
                    An unexpected error occurred while processing the request.
                    Please refresh your browser.
                </p>
            </template>
        </HgAlertComponent>
        <HtmlTextAreaComponent v-if="!isLoading" :input="termsOfService" />
    </div>
</template>
