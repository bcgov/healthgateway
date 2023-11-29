<script setup lang="ts">
import { computed, watch } from "vue";
import { useRouter } from "vue-router";

import { useConfigStore } from "@/stores/config";
import { useWaitlistStore } from "@/stores/waitlist";

interface Props {
    redirectPath: string;
}
const props = defineProps<Props>();

const router = useRouter();
const configStore = useConfigStore();
const waitlistStore = useWaitlistStore();

const ticketIsProcessed = computed(() => waitlistStore.ticketIsProcessed);
const queuePosition = computed(() => waitlistStore.ticket?.queuePosition);

function redirect(): void {
    router.push({ path: props.redirectPath });
}

watch(ticketIsProcessed, (value: boolean) => {
    if (value) {
        redirect();
    }
});

if (
    !configStore.webConfig.featureToggleConfiguration.waitingQueue.enabled ||
    !waitlistStore.ticket ||
    waitlistStore.ticketIsProcessed
) {
    redirect();
}
</script>

<template>
    <v-row justify="center" class="mt-12">
        <v-col cols="12" md="8" lg="6" xl="4" class="text-center">
            <v-img
                src="@/assets/images/queue/waiting.png"
                data-testid="queue-image"
                alt="Queue waiting image"
                max-height="200px"
            />
            <p class="my-4 text-body-1">
                Health Gateway is busier than normal at this moment. We have
                placed you in a queue.
            </p>
            <div v-if="queuePosition !== undefined" class="mt-4">
                <h2 class="mb-4 text-h5">Your position in the queue:</h2>
                <p
                    class="font-weight-bold text-h5"
                    data-testid="queue-position"
                >
                    {{ queuePosition }}
                </p>
            </div>
        </v-col>
    </v-row>
</template>
