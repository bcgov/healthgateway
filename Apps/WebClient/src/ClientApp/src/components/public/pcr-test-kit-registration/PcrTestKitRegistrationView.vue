<script setup lang="ts">
import { computed, ref, watch } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PcrTestKitRegistrationForm from "@/components/public/pcr-test-kit-registration/PcrTestKitRegistrationForm.vue";
import PcrTestKitRegistrationMethodPrompt from "@/components/public/pcr-test-kit-registration/PcrTestKitRegistrationMethodPrompt.vue";
import PcrTestKitRegistrationSuccessfulDisplay from "@/components/public/pcr-test-kit-registration/PcrTestKitRegistrationSuccessfulDisplay.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { PcrDataSource } from "@/constants/pcrTestDataSource";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ILogger } from "@/services/interfaces";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { useErrorStore } from "@/stores/error";

interface Props {
    serialNumber?: string;
}
const props = defineProps<Props>();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const configStore = useConfigStore();
const authStore = useAuthStore();
const errorStore = useErrorStore();

const pcrTestKitRegistrationForm =
    ref<InstanceType<typeof PcrTestKitRegistrationForm>>();

const registrationComplete = ref(false);
const isLoading = ref(false);

const dataSource = ref(PcrDataSource.None);

const oidcIsAuthenticated = computed(() => authStore.oidcIsAuthenticated);

function setDataSource(value: PcrDataSource): void {
    if (value === PcrDataSource.Keycloak && !oidcIsAuthenticated.value) {
        isLoading.value = true;
        const redirectPath = props.serialNumber
            ? `/pcrtest/${props.serialNumber}`
            : "/pcrtest";
        authStore
            .signIn(redirectPath, configStore.identityProviders[0].hint)
            .catch((err) => {
                logger.error(`oidcLogin Error: ${err}`);
                errorStore.addError(
                    ErrorType.Retrieve,
                    ErrorSourceType.User,
                    undefined
                );
            })
            .finally(() => (isLoading.value = false));
    }
    pcrTestKitRegistrationForm.value?.resetForm();
    dataSource.value = value;
}

function onFormSuccess(): void {
    pcrTestKitRegistrationForm.value?.resetForm();
    isLoading.value = false;
    registrationComplete.value = true;
    window.scrollTo({ top: 0, behavior: "smooth" });
}

function onFormCancel(): void {
    dataSource.value = PcrDataSource.None;
    window.scrollTo({ top: 0, behavior: "smooth" });
}

watch(oidcIsAuthenticated, (value) => {
    if (value) {
        dataSource.value = PcrDataSource.Keycloak;
    }
});

if (oidcIsAuthenticated.value) {
    dataSource.value = PcrDataSource.Keycloak;
}
</script>

<template>
    <LoadingComponent :is-loading="isLoading" />
    <template v-if="!isLoading">
        <PcrTestKitRegistrationMethodPrompt
            v-if="!registrationComplete && dataSource === PcrDataSource.None"
            @on-data-source-selected="setDataSource"
        />
        <PcrTestKitRegistrationForm
            v-else-if="
                !registrationComplete && dataSource !== PcrDataSource.None
            "
            ref="pcrTestKitRegistrationForm"
            :data-source="dataSource"
            :serial-number="serialNumber"
            @on-success="onFormSuccess"
            @on-cancel="onFormCancel"
        />
        <PcrTestKitRegistrationSuccessfulDisplay
            v-else-if="registrationComplete"
            :oidc-is-authenticated="oidcIsAuthenticated"
        />
    </template>
</template>
