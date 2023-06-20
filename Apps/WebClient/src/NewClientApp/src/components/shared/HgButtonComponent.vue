<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    variant?: HgButtonVariant;
    size?: string;
    inverse?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    variant: "primary",
    size: "",
    inverse: false,
});

type HgButtonVariant = "primary" | "secondary" | "link";

interface HgButtonVariantDetails {
    vuetifyVariant: "flat" | "outlined" | "text";
    color: string;
}

const variants = new Map<HgButtonVariant, HgButtonVariantDetails>();
variants.set("primary", {
    vuetifyVariant: "flat",
    color: "primary",
});
variants.set("secondary", {
    vuetifyVariant: "outlined",
    color: "primary",
});
variants.set("link", {
    vuetifyVariant: "text",
    color: "link",
});

const slots = useSlots();

const vuetifyVariant = computed(
    () => variants.get(props.variant)?.vuetifyVariant
);
const color = computed(() => variants.get(props.variant)?.color);
const hasSlot = computed(() => slots.default !== undefined);
</script>

<template>
    <v-btn
        v-if="hasSlot"
        :variant="vuetifyVariant"
        :color="color"
        :ripple="false"
    >
        <slot />
    </v-btn>
    <v-btn v-else :variant="vuetifyVariant" :color="color" :ripple="false" />
</template>
