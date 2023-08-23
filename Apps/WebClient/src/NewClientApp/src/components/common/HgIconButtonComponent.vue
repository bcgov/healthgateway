<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    icon?: string | boolean;
}
withDefaults(defineProps<Props>(), {
    icon: true,
});

const slots = useSlots();

const hasSlot = computed(() => slots.default !== undefined);
</script>

<template>
    <v-btn v-if="hasSlot" :icon="icon" variant="flat" class="bg-transparent">
        <slot />
    </v-btn>
    <v-btn v-else :icon="icon" variant="flat" class="bg-transparent" />
</template>
