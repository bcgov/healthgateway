<script setup lang="ts">
import { computed } from "vue";

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
    className: string;
    vuetifyVariant: "flat" | "outlined" | "text";
    color: string;
}

const variants = new Map<HgButtonVariant, HgButtonVariantDetails>([
    [
        "primary",
        { className: "hg-primary", vuetifyVariant: "flat", color: "primary" },
    ],
    [
        "secondary",
        {
            className: "hg-secondary",
            vuetifyVariant: "outlined",
            color: "primary",
        },
    ],
    ["link", { className: "hg-link", vuetifyVariant: "text", color: "link" }],
]);

variants.set("primary", {
    className: "hg-primary",
    vuetifyVariant: "flat",
    color: "primary",
});
variants.set("secondary", {
    className: "hg-secondary",
    vuetifyVariant: "outlined",
    color: "primary",
});
variants.set("link", {
    className: "hg-link",
    vuetifyVariant: "text",
    color: "link",
});

const className = computed(() => variants.get(props.variant)?.className);
const vuetifyVariant = computed(
    () => variants.get(props.variant)?.vuetifyVariant
);
const color = computed(() => variants.get(props.variant)?.color);
</script>

<template>
    <v-btn
        :variant="vuetifyVariant"
        class="hg-button"
        :class="className"
        :color="color"
        :ripple="false"
    >
        <slot />
    </v-btn>
</template>
