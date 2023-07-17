<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-router";

import { useWaitlistStore } from "@/stores/waitlist";

const router = useRouter();
const waitlistStore = useWaitlistStore();

const tooBusy = computed<boolean>(() => waitlistStore.tooBusy);

// if (!tooBusy.value) {
//     router.push({ path: "/" });
// }
</script>

<template>
    <v-row justify="center" class="mt-12">
        <v-col cols="12" md="8" lg="6" xl="4">
            <v-alert
                show
                type="error"
                data-testid="too-many-tickets"
                class="mb-12 text-center"
                text="Can’t access Health Gateway. Please try again later."
            />
            <v-img
                src="@/assets/images/queue/error.png"
                data-testid="queue-error-image"
                alt="Queue full image"
                max-height="300px"
            />
        </v-col>
    </v-row>
</template>
