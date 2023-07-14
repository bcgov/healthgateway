<script setup lang="ts">
interface Props {
    title: string;
}
defineProps<Props>();
</script>

<template>
    <h3 class="text-subtitle-1 font-weight-bold mb-2 d-flex align-center">
        <span>{{ title }}</span>
        <slot name="append" />
    </h3>
</template>
