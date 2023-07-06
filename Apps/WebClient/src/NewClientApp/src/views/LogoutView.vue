<script setup lang="ts">
import { onMounted } from "vue";

import LoadingComponent from "@/components/shared/LoadingComponent.vue";
import { useAuthStore } from "@/stores/auth";
import { useWaitlistStore } from "@/stores/waitlist";

const authStore = useAuthStore();
const waitlistStore = useWaitlistStore();

function signOut(): void {
    authStore.signOut();
    waitlistStore.releaseTicket();
}

onMounted(() => {
    signOut();
});
</script>

<template>
    <LoadingComponent text="Signing Out..." is-loading />
</template>
