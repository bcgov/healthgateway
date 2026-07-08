<script setup lang="ts">
import { computed, ref } from "vue";
import useClipboard from "vue-clipboard3";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import MessageModalComponent from "@/components/common/MessageModalComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { BannerError } from "@/models/errors";
import { ILogger } from "@/services/interfaces";
import { useErrorStore } from "@/stores/error";
import { useUserStore } from "@/stores/user";

const today = DateWrapper.today().format();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const { toClipboard } = useClipboard();
const errorStore = useErrorStore();
const userStore = useUserStore();

const detailsExpanded = ref(false);

const copySuccessModal = ref<InstanceType<typeof MessageModalComponent>>();

const isShowing = computed<boolean>(
    () => errorStore.genericErrorBanner.isShowing
);
const errors = computed<BannerError[]>(
    () => errorStore.genericErrorBanner.errors
);

const errorTitle = computed(() =>
    errors.value.length > 1
        ? "Multiple errors have occurred"
        : errors.value[0]?.title
);
const errorDetails = computed(() =>
    errors.value.map((error) => {
        const source = error.source?.trim() ?? "";
        const hdid = userStore.hdid;
        const traceId = error.traceId ? `:${error.traceId.trim()}` : "";
        return `${source}/${today}/${hdid}${traceId}`;
    })
);
const formattedErrorDetails = computed(() => errorDetails.value.join("\n"));

function clearErrors(): void {
    errorStore.clearErrors();
}

async function copyToClipboard() {
    try {
        await toClipboard(formattedErrorDetails.value);
        copySuccessModal.value?.showModal();
    } catch (e) {
        logger.error(JSON.stringify(e));
    }
}
</script>

<template>
    <TooManyRequestsComponent />
    <HgAlertComponent
        :model-value="isShowing"
        :title="errorTitle"
        type="error"
        data-testid="errorBanner"
        closable
        variant="outlined"
        @click:close="clearErrors"
    >
        <v-expansion-panels
            variant="accordion"
            class="hg-error-details mt-4"
            rounded="6"
        >
            <v-expansion-panel
                v-model="detailsExpanded"
                data-testid="errorDetailsBtn"
                :title="detailsExpanded ? 'Hide Details' : 'View Details'"
                elevation="0"
                bg-color="error-background-lighten-1"
            >
                <template #text>
                    <p>
                        Try refreshing the page. If this issue persists, contact
                        <a
                            href="mailto:HealthGateway@gov.bc.ca"
                            class="text-link"
                            >HealthGateway@gov.bc.ca</a
                        >
                        and provide
                        <HgButtonComponent
                            variant="secondary"
                            data-testid="copyToClipBoardBtn"
                            prepend-icon="copy"
                            text="Copy"
                            class="bg-surface"
                            @click="copyToClipboard"
                        />
                    </p>
                    <p
                        v-for="(error, index) in errorDetails"
                        :key="index"
                        class="text-break"
                        :data-testid="'error-details-span-' + (index + 1)"
                    >
                        {{ error }}
                    </p>
                </template>
            </v-expansion-panel>
        </v-expansion-panels>
    </HgAlertComponent>
    <MessageModalComponent
        ref="copySuccessModal"
        title="Copy to Clipboard"
        message="Copied Successfully"
        :ok-only="true"
    />
</template>

<style lang="scss" scoped>
// style expansion panels inside alert
.hg-error-details.v-expansion-panels {
    border: 2px solid rgb(var(--v-theme-error-background-darken-1));
}

.hg-error-details::v-deep(.v-expansion-panel-text) {
    border-top: 1px solid rgb(var(--v-theme-error-background-darken-1));
}

.hg-error-details::v-deep(.v-expansion-panel-text__wrapper) {
    padding-top: 1rem;
}

.hg-error-details::v-deep(button.v-expansion-panel-title) {
    font-weight: bold;
    color: rgb(var(--v-theme-error-darken-1));
}

.hg-error-details::v-deep(.v-expansion-panel-title) {
    min-height: auto !important;
}
</style>
