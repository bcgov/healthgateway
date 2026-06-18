<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    variant?: HgButtonVariant;
    inverse?: boolean;
    disabled?: boolean;
    uppercase?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    variant: "primary",
    inverse: false,
    disabled: undefined,
    uppercase: true,
});

export type HgButtonVariant =
    | "primary"
    | "secondary"
    | "error"
    | "error-secondary"
    | "link"
    | "transparent";

interface HgButtonVariantDetails {
    inverseVariant: HgButtonVariant;
    vuetifyVariant: "elevated" | "flat" | "text" | "outlined";
    color: string;
    disabledColor?: string;
}

const variants = new Map<HgButtonVariant, HgButtonVariantDetails>();
variants.set("primary", {
    inverseVariant: "secondary",
    vuetifyVariant: "elevated",
    color: "primary",
});
variants.set("secondary", {
    inverseVariant: "primary",
    vuetifyVariant: "outlined",
    color: "primary",
    disabledColor: "grey-lighten-6",
});
variants.set("error", {
    inverseVariant: "error-secondary",
    vuetifyVariant: "elevated",
    color: "error",
});
variants.set("error-secondary", {
    inverseVariant: "error",
    vuetifyVariant: "outlined",
    color: "error",
    disabledColor: "grey-lighten-6",
});
variants.set("link", {
    inverseVariant: "link",
    vuetifyVariant: "text",
    color: "link",
});
variants.set("transparent", {
    inverseVariant: "transparent",
    vuetifyVariant: "flat",
    color: "transparent",
});

const slots = useSlots();

const hasSlot = computed(() => slots.default !== undefined);
const variantDetails = computed(() => {
    const details = variants.get(props.variant)!;
    return props.inverse ? variants.get(details.inverseVariant)! : details;
});
const color = computed(() => variantDetails.value.color);
const disabledColor = computed(
    () => variantDetails.value.disabledColor ?? color.value
);

const textCaseClass = computed(() =>
    props.uppercase ? "text-uppercase" : "text-none"
);
</script>

<template>
    <v-btn
        v-if="hasSlot"
        :variant="variantDetails.vuetifyVariant"
        :color="disabled ? disabledColor : color"
        :disabled="disabled"
        :class="['transition-none', textCaseClass]"
    >
        <slot />
    </v-btn>
    <v-btn
        v-else
        :variant="variantDetails.vuetifyVariant"
        :color="disabled ? disabledColor : color"
        :disabled="disabled"
        :class="['transition-none', textCaseClass]"
    />
</template>

<style lang="scss" scoped>
.transition-none {
    transition: none;
}
</style>
