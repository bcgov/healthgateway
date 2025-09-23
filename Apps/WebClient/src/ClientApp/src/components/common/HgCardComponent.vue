<script setup lang="ts">
import { computed, useSlots } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";

interface Props {
    title?: string;
    density?: "compact" | "normal";
    compactHeader?: boolean;
    variant?: "elevated" | "flat" | "outlined" | "tonal" | "text";
    elevation?: number | string;
    border?: boolean | string;
    color?: string;
    fillBody?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    title: undefined,
    density: "normal",
    compactHeader: false,
    variant: "elevated",
    elevation: 1,
    border: undefined,
    color: undefined,
    fillBody: false,
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
    <v-card
        class="hover-card"
        :class="props.fillBody ? 'd-flex flex-column' : ''"
        :variant="variant"
        :elevation="elevation"
        :border="border"
        :color="color"
    >
        <template #title>
            <v-row align="center" class="text-wrap">
                <v-col
                    v-if="hasIconSlot"
                    class="flex-grow-0 d-flex align-center"
                    :class="[
                        isNormalDensity
                            ? props.compactHeader
                                ? 'py-3 py-sm-5'
                                : 'py-5'
                            : '',
                        props.compactHeader ? 'ps-4 pe-2 ps-sm-3 pe-sm-3' : '',
                    ]"
                >
                    <slot name="icon" />
                </v-col>
                <v-col
                    v-if="title"
                    data-testid="card-button-title"
                    :class="[
                        isNormalDensity
                            ? props.compactHeader
                                ? 'py-3 py-sm-5'
                                : 'py-5'
                            : '',
                        props.compactHeader ? 'px-0 px-sm-3' : '',
                    ]"
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
        <template v-if="hasDefaultSlot && !props.fillBody" #text>
            <slot name="default" />
        </template>
        <template v-if="hasDefaultSlot && props.fillBody" #default>
            <div class="v-card-text d-flex flex-column flex-grow-1">
                <slot name="default" />
            </div>
        </template>
    </v-card>
</template>

<style scoped>
.hover-card {
    --v-hover-opacity: 0;
}
.hover-card:hover {
    background-color: rgb(var(--v-theme-surfaceHover)) !important;
}
</style>
