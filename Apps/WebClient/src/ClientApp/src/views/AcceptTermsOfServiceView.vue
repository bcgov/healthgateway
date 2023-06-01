<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import { useVuelidate } from "@vuelidate/core";
import { sameAs } from "@vuelidate/validators";
import { computed, ref } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

library.add(faExclamationTriangle);

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const userProfileService = container.get<IUserProfileService>(
    SERVICE_IDENTIFIER.UserProfileService
);
const router = useRouter();
const store = useStore();

const isLoading = ref(true);
const termsOfServiceId = ref("");
const termsOfService = ref("");
const accepted = ref(false);

const isValid = computed(() => {
    const param = v$.value.accepted;
    return param.$dirty ? !param.$invalid : undefined;
});
const validations = computed(() => ({
    accepted: { isChecked: sameAs(() => true) },
}));

const v$ = useVuelidate(validations, { accepted });

function updateAcceptedTerms(termsOfServiceId: string): Promise<void> {
    return store.dispatch("user/updateAcceptedTerms", { termsOfServiceId });
}

function addError(
    errorType: ErrorType,
    source: ErrorSourceType,
    traceId: string | undefined
): Promise<void> {
    return store.dispatch("errorBanner/addError", {
        errorType,
        source,
        traceId,
    });
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function setTooManyRequestsWarning(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsWarning", { key });
}

function loadTermsOfService(): void {
    isLoading.value = true;
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
                setTooManyRequestsWarning("page");
            } else {
                addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.TermsOfService,
                    undefined
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });
}

function onSubmit(event: Event): void {
    v$.value.$touch();
    if (v$.value.$invalid) {
        event.preventDefault();
        return;
    }

    isLoading.value = true;

    updateAcceptedTerms(termsOfServiceId.value)
        .then(() => redirect())
        .catch((err: ResultError) => {
            logger.error(
                `Error updating accepted terms of service: ${JSON.stringify(
                    err
                )}`
            );
            if (err.statusCode === 429) {
                setTooManyRequestsError("page");
            } else {
                addError(
                    ErrorType.Update,
                    ErrorSourceType.Profile,
                    err.traceId
                );
            }
        })
        .finally(() => {
            isLoading.value = false;
        });

    event.preventDefault();
}

function redirect(): void {
    logger.debug("Redirecting to /home");
    router.push({ path: "/home" });
}

loadTermsOfService();
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading" />
        <b-container v-if="!isLoading && termsOfService !== ''">
            <div>
                <b-form ref="termsOfServiceForm" @submit.prevent="onSubmit">
                    <page-title
                        title="Update to our Terms of Service"
                        data-testid="tos-page-title"
                    />
                    <HtmlTextAreaComponent
                        class="termsOfService mb-3"
                        :input="termsOfService"
                        data-testid="tos-text-area-component"
                    />
                    <div class="mb-3">
                        <b-form-checkbox
                            id="accept-tos-checkbox"
                            v-model="accepted"
                            data-testid="accept-tos-checkbox"
                            class="accept"
                            :state="isValid"
                        >
                            I agree to the terms of service above
                        </b-form-checkbox>
                        <b-form-invalid-feedback :state="isValid">
                            You must accept the terms of service.
                        </b-form-invalid-feedback>
                    </div>
                    <div class="mb-3 text-right">
                        <hg-button
                            class="px-5"
                            type="submit"
                            data-testid="continue-btn"
                            variant="primary"
                            :disabled="!accepted"
                            >Continue</hg-button
                        >
                    </div>
                </b-form>
            </div>
        </b-container>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.accept label {
    color: $primary;
}

.termsOfService {
    max-height: 460px;
    overflow-y: scroll;
    box-shadow: 0 0 2px #00000070;
    border: none;
}
</style>
