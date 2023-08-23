<script setup lang="ts">
import { onMounted } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import { useAuthStore } from "@/stores/auth";

const authStore = useAuthStore();

onMounted(() => {
    authStore.signOut();
});
</script>

<template>
    <LoadingComponent text="Signing Out..." is-loading />
</template>
