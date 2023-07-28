<script setup lang="ts">
import { useSlots } from "vue";

import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";

interface Props {
    name: string;
    value?: string | null;
    nameClass?: string | string[];
    valueClass?: string | string[];
    horizontal?: boolean;
}
withDefaults(defineProps<Props>(), {
    value: undefined,
    nameClass: undefined,
    valueClass: undefined,
    horizontal: true,
});

const slots = useSlots();
</script>

<template>
    <p v-if="horizontal" class="text-body-1">
        <span :class="nameClass">{{ name }}: </span>
        <template v-if="slots.value !== undefined">
            <slot name="value" />
            <slot name="append" />
            <slot name="append-value" />
        </template>
        <span v-else :class="valueClass" v-bind="$attrs">
            {{ value || "N/A" }}<slot name="append" />
            <slot name="append-value" />
        </span>
    </p>
    <template v-else>
        <SectionHeaderComponent :class="nameClass" :title="name">
            <template #append><slot name="append" /></template>
        </SectionHeaderComponent>
        <template v-if="slots.value !== undefined">
            <slot name="value" />
            <slot name="append-value" />
        </template>
        <p v-else class="text-body-1" :class="valueClass" v-bind="$attrs">
            {{ value || "N/A" }}<slot name="append-value" />
        </p>
    </template>
</template>
