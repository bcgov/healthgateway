<script setup lang="ts">
import { computed, useSlots } from "vue";

import HgButtonComponent, {
    HgButtonVariant,
} from "@/components/common/HgButtonComponent.vue";

interface Props {
    icon?: string | boolean;
    variant?: HgButtonVariant;
}
withDefaults(defineProps<Props>(), {
    icon: true,
    variant: "transparent",
});

const slots = useSlots();

const hasSlot = computed(() => slots.default !== undefined);
</script>

<template>
    <HgButtonComponent v-if="hasSlot" :icon="icon" :variant="variant">
        <slot />
    </HgButtonComponent>
    <HgButtonComponent v-else :icon="icon" :variant="variant" />
</template>
