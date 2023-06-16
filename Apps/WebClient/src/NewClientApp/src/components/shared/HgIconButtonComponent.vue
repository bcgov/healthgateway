<script setup lang="ts">
interface Props {
    icon: string;
}
defineProps<Props>();
</script>

<template>
    <v-btn :icon="icon" variant="flat" class="bg-transparent" :ripple="false" />
</template>
