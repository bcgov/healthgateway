<script setup lang="ts">
interface Props {
    title: string;
    subtitle?: string;
}
defineProps<Props>();
</script>

<template>
    <div class="d-flex justify-content-center">
        <div class="px-3 py-5">
            <h1>{{ title }}</h1>
            <h2 v-if="subtitle">{{ subtitle }}</h2>
            <slot />
        </div>
    </div>
</template>
