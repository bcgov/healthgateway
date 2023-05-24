<script setup lang="ts">
import { computed, watch } from "vue";
import { useRoute, useRouter, useStore } from "vue-composition-wrapper";

import type { WebClientConfiguration } from "@/models/configData";
import type { Ticket } from "@/models/ticket";

const store = useStore();

const webClient = computed<WebClientConfiguration>(() => {
    return store.getters["config/webClient"];
});

const ticket = computed<Ticket>(() => {
    return store.getters["waitlist/ticket"];
});

const ticketIsProcessed = computed<boolean>(() => {
    return store.getters["waitlist/ticketIsProcessed"];
});

const queuePosition = computed<number | undefined>(
    () => ticket?.value.queuePosition
);

const route = useRoute();
const router = useRouter();
function redirect(): void {
    const path = route.value.query.redirect
        ? route.value.query.redirect.toString()
        : "/";
    router.push({ path });
}

watch(ticketIsProcessed, (value: boolean) => {
    if (value) {
        redirect();
    }
});

if (
    !webClient.value.featureToggleConfiguration.waitingQueue.enabled ||
    !ticket.value ||
    ticketIsProcessed.value
) {
    redirect();
}
</script>

<template>
    <div
        class="queue flex-grow-1 d-flex flex-column align-items-center justify-content-center p-3 p-md-4 text-center"
    >
        <img
            src="@/assets/images/queue/waiting.png"
            class="img-fluid p-3"
            data-testid="queue-image"
            alt=""
        />
        <p>
            Health Gateway is busier than normal at this moment. We have placed
            you in a queue.
        </p>
        <h2 v-if="queuePosition !== undefined" class="mt-4">
            <div class="mb-3">Your position in the queue:</div>
            <div data-testid="queue-position">{{ queuePosition }}</div>
        </h2>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.queue {
    color: $hg-brand-primary;
}
</style>
