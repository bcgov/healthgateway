<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    title: string;
    dense?: boolean;
    isInteractable?: boolean;
}
withDefaults(defineProps<Props>(), {
    dense: false,
    isInteractable: false,
});

const slots = useSlots();

const hasIconSlot = computed<boolean>(() => {
    return slots.icon !== undefined;
});

const hasActionIconSlot = computed<boolean>(() => {
    return slots["action-icon"] !== undefined;
});

const hasMenuSlot = computed<boolean>(() => {
    return slots.menu !== undefined;
});

const hasDefaultSlot = computed<boolean>(() => {
    return slots.default !== undefined;
});
</script>

<template>
    <div
        class="hg-card h-100 w-100 d-flex flex-column align-content-start text-left rounded"
        :class="{
            interactable: isInteractable,
            shadow: !isInteractable,
            'p-3': dense,
            'p-4': !dense,
        }"
    >
        <b-row
            no-gutters
            align-h="end"
            class="mt-n3 w-100"
            :class="{ 'mb-4': hasDefaultSlot }"
        >
            <b-col
                v-if="hasIconSlot"
                cols="auto"
                align-self="center"
                class="pr-3 mt-3 d-flex"
            >
                <slot name="icon" />
            </b-col>
            <b-col
                data-testid="card-button-title"
                class="hg-card-title mt-3"
                :class="{ dense: dense }"
            >
                {{ title }}
            </b-col>
            <b-col
                v-if="hasActionIconSlot"
                cols="auto"
                align-self="center"
                class="mt-3 d-flex"
            >
                <slot name="action-icon" />
            </b-col>
            <b-col v-if="hasMenuSlot" cols="auto" class="mt-2">
                <slot name="menu" />
            </b-col>
        </b-row>
        <slot />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.hg-card {
    background: $light_background;
    color: $hg-text-primary;
    outline-width: 0.2rem;
    outline-style: solid;
    outline-color: transparent;

    // add outline-color to transition
    transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
        border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out,
        outline-color 0.15s ease-in-out;

    .hg-card-title {
        &:not(.dense) {
            font-size: 1.2rem;
            font-weight: bold;
        }
    }

    &.interactable {
        // add text-decoration-color to transition
        transition: text-decoration-color 0.15s ease-in-out;

        text-decoration: underline;
        text-decoration-color: transparent;

        &:hover:not([disabled]),
        &:active:not([disabled]),
        &:focus:not([disabled]),
        &:focus-visible:not([disabled]) {
            color: $hg-text-primary;
            background: #f5f5f5;

            .hg-card-title {
                text-decoration-color: $hg-text-primary;
            }
        }

        &:active:not([disabled]),
        &:focus:not([disabled]),
        &:focus-visible:not([disabled]) {
            outline-width: 0.2rem;
            outline-style: solid;
            outline-color: rgba(86, 86, 86, 0.25);
        }
    }

    .chevron-icon {
        color: $primary;
    }
}
</style>
