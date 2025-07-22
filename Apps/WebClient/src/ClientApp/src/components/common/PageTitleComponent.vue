<script setup lang="ts">
import { computed, useSlots } from "vue";

import { useLayoutStore } from "@/stores/layout";

interface Props {
    title: string;
}
defineProps<Props>();

const slots = useSlots();
const layoutStore = useLayoutStore();

const hasPrependSlot = computed(() => slots.prepend !== undefined);
const hasAppendSlot = computed(() => slots.append !== undefined);
</script>

<template>
    <div id="pageTitle" class="mb-4">
        <v-row dense justify="end" align="center">
            <v-col v-if="hasPrependSlot" class="flex-grow-0">
                <slot name="prepend" />
            </v-col>
            <v-col>
                <h1
                    id="subject"
                    class="text-primary font-weight-bold"
                    :class="layoutStore.isMobile ? 'text-h5' : 'text-h4'"
                >
                    {{ title }}
                </h1>
            </v-col>
            <v-col v-if="hasAppendSlot" class="flex-grow-0">
                <slot name="append" />
            </v-col>
        </v-row>
    </div>
</template>
