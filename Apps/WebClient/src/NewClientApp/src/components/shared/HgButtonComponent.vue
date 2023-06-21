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
    inverseVariant: HgButtonVariant;
    vuetifyVariant: "flat" | "outlined" | "text";
    color: string;
    bgColor?: string;
}

const variants = new Map<HgButtonVariant, HgButtonVariantDetails>();
variants.set("primary", {
    inverseVariant: "secondary",
    vuetifyVariant: "flat",
    color: "primary",
});
variants.set("secondary", {
    inverseVariant: "primary",
    vuetifyVariant: "outlined",
    color: "primary",
    bgColor: "white",
});
variants.set("link", {
    inverseVariant: "link",
    vuetifyVariant: "text",
    color: "link",
});

const slots = useSlots();

const hasSlot = computed(() => slots.default !== undefined);
const variantDetails = computed(() => {
    const details = variants.get(props.variant)!;
    return props.inverse ? variants.get(details.inverseVariant)! : details;
});
const bgColor = computed(() => variantDetails.value.bgColor);
const classes = computed(() => ({
    [`bg-${bgColor.value}`]: Boolean(bgColor.value),
}));
</script>

<template>
    <v-btn
        v-if="hasSlot"
        :variant="variantDetails.vuetifyVariant"
        :color="variantDetails.color"
        :class="classes"
    >
        <slot />
    </v-btn>
    <v-btn
        v-else
        :variant="variantDetails.vuetifyVariant"
        :color="variantDetails.color"
        :class="classes"
    />
</template>
