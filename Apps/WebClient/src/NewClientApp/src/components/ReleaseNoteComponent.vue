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
        <v-row>
            <v-col class="text-body-1 font-weight-bold">
                {{ date }} – {{ version }} – {{ title }}
            </v-col>
        </v-row>
        <v-row v-if="hasSlot" class="mt-1 mb-2">
            <v-col class="px-3 py-0">
                <slot />
            </v-col>
        </v-row>
    </div>
</template>
