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
    <div>
        <b-row class="mb-2">
            <b-col>
                <strong>{{ date }} – {{ version }} – {{ title }}</strong>
            </b-col>
        </b-row>
        <b-row v-if="hasSlot">
            <b-col>
                <slot />
            </b-col>
        </b-row>
    </div>
</template>
