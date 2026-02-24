<script setup lang="ts">
interface Props {
    maxWidth?: number;
    iconTestid?: string;
    tooltipTestid?: string;
}

const props = withDefaults(defineProps<Props>(), {
    maxWidth: 300,
    iconTestid: "interactive-info-tooltip-icon",
    tooltipTestid: "interactive-info-tooltip-content",
});
</script>

<template>
    <v-menu
        open-on-hover
        :close-on-content-click="false"
        location="bottom"
        offset="8"
    >
        <template #activator="{ props: menuProps }">
            <v-icon
                v-bind="menuProps"
                icon="far fa-circle-question"
                color="primary"
                size="x-small"
                class="ml-2"
                :data-testid="props.iconTestid"
            />
        </template>

        <v-card
            class="bg-primary text-white pa-3"
            :style="{ maxWidth: `${props.maxWidth}px` }"
            :data-testid="props.tooltipTestid"
        >
            <slot />
        </v-card>
    </v-menu>
</template>
