<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    date: string;
    version: string;
    title: string;
}
defineProps<Props>();

const slots = useSlots();
const hasSlot = computed<boolean>(() => {
    return slots.default !== undefined;
});
</script>

<template>
    <div class="mb-6">
        <div class="text-h6 font-weight-bold">
            {{ date }} – {{ version }} – {{ title }}
        </div>

        <div v-if="hasSlot" class="text-body-1 mt-2">
            <slot />
        </div>
    </div>
</template>
