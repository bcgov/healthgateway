<script setup lang="ts">
import { computed, useSlots } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";

interface Props {
    title?: string;
}
withDefaults(defineProps<Props>(), {
    title: "",
});

const slots = useSlots();

const hasIconSlot = computed<boolean>(() => {
    return slots.icon !== undefined;
});

const hasActionIconSlot = computed<boolean>(() => {
    return slots["action-icon"] !== undefined;
});

const hasMenuItemsSlot = computed<boolean>(() => {
    return slots["menu-items"] !== undefined;
});

const hasDefaultSlot = computed<boolean>(() => {
    return slots.default !== undefined;
});
</script>

<template>
    <v-card>
        <template #title>
            <v-row align="center" class="text-wrap">
                <v-col v-if="hasIconSlot" class="flex-grow-0 d-flex py-5">
                    <slot name="icon" />
                </v-col>
                <v-col data-testid="card-button-title" class="py-5">
                    {{ title }}
                </v-col>
                <v-col v-if="hasActionIconSlot" class="flex-grow-0 d-flex pa-5">
                    <slot name="action-icon" />
                </v-col>
                <v-col v-if="hasMenuItemsSlot" class="flex-grow-0 py-3">
                    <v-menu location="bottom end">
                        <template #activator="{ props }">
                            <HgIconButtonComponent
                                v-bind="props"
                                icon="fas fa-ellipsis-vertical"
                                aria-label="Menu"
                                data-testid="card-menu-button"
                                @click.stop.prevent
                            />
                        </template>
                        <v-list>
                            <slot name="menu-items" />
                        </v-list>
                    </v-menu>
                </v-col>
            </v-row>
        </template>
        <template v-if="hasDefaultSlot" #text>
            <slot name="default" />
        </template>
    </v-card>
</template>
