<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faCopy as farCopy } from "@fortawesome/free-regular-svg-icons";
import { faChevronDown, faChevronUp } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import MessageModalComponent from "@/components/modal/MessageModalComponent.vue";
import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { BannerError } from "@/models/errors";
import User from "@/models/user";

library.add(faChevronDown, faChevronUp, farCopy);

const today = new DateWrapper().format("yyyy-MMM-dd");

const store = useStore();

const copySuccessModal = ref<InstanceType<typeof MessageModalComponent>>();

const user = computed<User>(() => store.getters["user/user"]);
const isShowing = computed<boolean>(
    () => store.getters["errorBanner/isShowing"]
);
const errors = computed<BannerError[]>(
    () => store.getters["errorBanner/errors"]
);

const hasMultipleErrors = computed(() => errorDetails.value.length > 1);
const errorTitle = computed(() =>
    errors.value.length === 1 ? errors.value[0].title : ""
);
const errorDetails = computed(() =>
    errors.value.map((error) => {
        const source = error.source?.trim() ?? "";
        const hdid = user.value.hdid;
        const traceId = error.traceId ? `:${error.traceId.trim()}` : "";
        return `${source}/${today}/${hdid}${traceId}`;
    })
);
const formattedErrorDetails = computed(() => errorDetails.value.join("\n"));

function clearErrors(): void {
    store.dispatch("errorBanner/clearErrors");
}

function copyToClipboard() {
    navigator.clipboard
        .writeText(formattedErrorDetails.value)
        .then(() => copySuccessModal.value?.showModal());
}
</script>

<template>
    <div>
        <TooManyRequestsComponent />
        <b-alert
            :show="isShowing"
            data-testid="errorBanner"
            variant="danger"
            dismissible
            class="no-print"
            @dismissed="clearErrors"
        >
            <div>
                <div
                    v-if="hasMultipleErrors"
                    data-testid="multipleErrorsHeader"
                >
                    <h4>Multiple errors have occurred</h4>
                </div>
                <div v-else data-testid="singleErrorHeader">
                    <h4>{{ errorTitle }}</h4>
                </div>
                <hg-button
                    v-b-toggle.errorDetails
                    data-testid="errorDetailsBtn"
                    variant="link"
                    class="detailsButton"
                >
                    <hg-icon
                        icon="chevron-down"
                        size="medium"
                        aria-hidden="true"
                        class="when-closed mr-2"
                        data-testid="viewDetailsIcon"
                    />
                    <span class="when-closed">View Details</span>
                    <hg-icon
                        icon="chevron-up"
                        size="medium"
                        aria-hidden="true"
                        class="when-opened mr-2"
                        data-testid="hideDetailsIcon"
                    />
                    <span class="when-opened">Hide Details</span>
                </hg-button>
                <b-collapse id="errorDetails">
                    <div class="py-2">
                        <p>
                            Try refreshing the page. If this issue persists,
                            contact
                            <a href="mailto:HealthGateway@gov.bc.ca"
                                >HealthGateway@gov.bc.ca</a
                            >
                            and provide
                            <hg-button
                                variant="secondary"
                                data-testid="copyToClipBoardBtn"
                                @click="copyToClipboard"
                            >
                                <hg-icon :icon="['far', 'copy']" size="small" />
                                Copy
                            </hg-button>
                        </p>
                        <p
                            v-for="(error, index) in errorDetails"
                            :key="index"
                            class="text-break"
                            :data-testid="'error-details-span-' + (index + 1)"
                        >
                            {{ error }}
                        </p>
                    </div>
                </b-collapse>
                <MessageModalComponent
                    ref="copySuccessModal"
                    title="Copy to Clipboard"
                    message="Copied Successfully"
                    :ok-only="true"
                />
            </div>
        </b-alert>
    </div>
</template>

<style lang="scss" scoped>
.collapsed > .when-opened,
:not(.collapsed) > .when-closed {
    display: none;
}
</style>
