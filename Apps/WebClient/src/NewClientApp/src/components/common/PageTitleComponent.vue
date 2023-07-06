<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    title: string;
}
defineProps<Props>();

const slots = useSlots();

const hasPrependSlot = computed(() => slots.prepend !== undefined);
const hasAppendSlot = computed(() => slots.append !== undefined);
</script>

<template>
    <div id="pageTitle">
        <v-row dense justify="end" align="center">
            <v-col v-if="hasPrependSlot" class="flex-grow-0">
                <slot name="prepend" />
            </v-col>
            <v-col>
                <h1 id="subject" class="text-primary text-h4 font-weight-bold">
                    {{ title }}
                </h1>
            </v-col>
            <v-col v-if="hasAppendSlot" class="flex-grow-0">
                <slot name="append" />
            </v-col>
        </v-row>
        <v-divider
            class="border-opacity-100 my-2"
            color="primary"
            :thickness="2"
        />
    </div>
</template>
