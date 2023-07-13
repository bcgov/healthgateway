<script setup lang="ts">
import { ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import { ErrorSourceType } from "@/constants/errorType";
import { Loader } from "@/constants/loader";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const showCloseWarning = ref(false);

function closeAccount(): void {
    loadingStore.applyLoader(
        Loader.UserProfile,
        "closeAccount",
        userStore
            .closeUserAccount()
            .then(() => (showCloseWarning.value = false))
            .catch((err: ResultError) => {
                logger.error(err.resultMessage);
                if (err.statusCode === 429) {
                    errorStore.setTooManyRequestsError("page");
                } else {
                    errorStore.addCustomError(
                        `Unable to close ${ErrorSourceType.Profile}`,
                        ErrorSourceType.Profile,
                        undefined
                    );
                }
            })
    );
}
</script>

<template>
    <SectionHeaderComponent title="Manage Account" />
    <HgButtonComponent
        v-if="!showCloseWarning"
        id="recoverAccountShowCloseWarningBtn"
        data-testid="recoverAccountShowCloseWarningBtn"
        variant="link"
        color="error"
        text="Delete My Account"
        @click="showCloseWarning = true"
    />
    <template v-else>
        <v-card variant="text">
            <template #text>
                <v-alert
                    class="pa-0"
                    type="error"
                    variant="text"
                    icon="exclamation-triangle"
                    text="Your account will be marked for removal, preventing
                        you from accessing your information on the Health
                        Gateway. After a set period of time it will be
                        removed permanently."
                />
            </template>
            <template #actions>
                <v-spacer />
                <HgButtonComponent
                    id="closeAccountCancelBtn"
                    data-testid="closeAccountCancelBtn"
                    class="mr-2"
                    variant="secondary"
                    text="Cancel"
                    @click="showCloseWarning = false"
                />
                <HgButtonComponent
                    id="closeAccountBtn"
                    data-testid="closeAccountBtn"
                    color="error"
                    text="Delete Account"
                    @click="closeAccount"
                />
            </template>
        </v-card>
    </template>
</template>
