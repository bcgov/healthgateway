<script setup lang="ts">
import { ref } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import SectionHeadingComponent from "@/components/common/SectionHeadingComponent.vue";
import { ErrorSourceType } from "@/constants/errorType";
import { Loader } from "@/constants/loader";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { ResultError } from "@/models/errors";
import { Action, Text, Type } from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useLoadingStore } from "@/stores/loading";
import { useUserStore } from "@/stores/user";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const errorStore = useErrorStore();
const loadingStore = useLoadingStore();
const userStore = useUserStore();

const showCloseWarning = ref(false);
const isClosingAccount = ref(false);

function closeAccount(): void {
    if (isClosingAccount.value) {
        return;
    }

    isClosingAccount.value = true;

    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.DeleteAccount,
        type: Type.Profile,
    });
    loadingStore.applyLoader(
        Loader.UserProfile,
        "closeAccount",
        userStore
            .closeUserAccount()
            .then(() => (showCloseWarning.value = false))
            .catch((err: ResultError) => {
                logger.error(err.message);
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
            .finally(() => {
                isClosingAccount.value = false;
            })
    );
}
</script>

<template>
    <SectionHeadingComponent title="Manage Account" include-divider />
    <HgButtonComponent
        v-if="!showCloseWarning"
        id="recoverAccountShowCloseWarningBtn"
        data-testid="recoverAccountShowCloseWarningBtn"
        variant="primary"
        text="Delete My Account"
        @click="showCloseWarning = true"
    />
    <template v-else>
        <v-card variant="text" class="mt-n4">
            <template #text>
                <HgAlertComponent
                    type="error"
                    variant="text"
                    text="Your account will be marked for removal, preventing
                        you from accessing your information on the Health
                        Gateway. After a set period of time it will be
                        removed permanently."
                    class="pa-0"
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
                    variant="error"
                    text="Delete Account"
                    :disabled="isClosingAccount"
                    @click="closeAccount"
                />
            </template>
        </v-card>
    </template>
</template>
