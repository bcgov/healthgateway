<script setup lang="ts">
import { computed, useSlots } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";

interface Props {
    title?: string;
    density?: "compact" | "normal";
}
const props = withDefaults(defineProps<Props>(), {
    title: undefined,
    density: "normal",
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

const isNormalDensity = computed<boolean>(() => {
    return props.density === "normal";
});
</script>

<template>
    <v-card>
        <template #title>
            <v-row align="center" class="text-wrap">
                <v-col
                    v-if="hasIconSlot"
                    class="flex-grow-0 d-flex"
                    :class="{ 'py-5': isNormalDensity }"
                >
                    <slot name="icon" />
                </v-col>
                <v-col
                    v-if="title"
                    data-testid="card-button-title"
                    :class="{ 'py-5': isNormalDensity }"
                >
                    {{ title }}
                </v-col>
                <v-spacer v-else />
                <v-col
                    v-if="hasActionIconSlot"
                    class="flex-grow-0 d-flex"
                    :class="{ 'pa-5': isNormalDensity }"
                >
                    <slot name="action-icon" />
                </v-col>
                <v-col
                    v-if="hasMenuItemsSlot"
                    class="flex-grow-0"
                    :class="{ 'py-3': isNormalDensity }"
                >
                    <v-menu location="bottom end">
                        <template #activator="{ props: activatorProps }">
                            <HgIconButtonComponent
                                v-bind="activatorProps"
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
