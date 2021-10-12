<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class HgDropdownComponent extends Vue {
    @Prop({ required: true }) variant!: string;

    private get classes(): string[] {
        let result = [];
        result.push("hg-dropdown", "hg-primary");
        return result;
    }

    private get bootstrapVariant(): string {
        return "primary";
    }
}
</script>

<template>
    <b-dropdown
        v-bind="$attrs"
        :variant="bootstrapVariant"
        :class="classes"
        v-on="$listeners"
    >
        <slot></slot>
    </b-dropdown>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.hg-dropdown {
    .dropdown-toggle {
        // add text-decoration-color to transition
        transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
            border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out,
            text-decoration-color 0.15s ease-in-out;

        text-decoration: underline;
        text-decoration-color: transparent;
    }

    &.hg-primary {
        .dropdown-toggle {
            background: $hg-button-primary;
            border-color: $hg-button-primary-border;
            color: $hg-button-primary-text;

            &:disabled {
                background: $hg-button-primary-disabled;
                border-color: $hg-button-primary-disabled-border;
                color: $hg-button-primary-disabled-text;
            }

            &:hover:not([disabled]),
            &:active:not([disabled]) {
                background: $hg-button-primary-hover;
                border-color: $hg-button-primary-hover-border;
                color: $hg-button-primary-hover-text;
                text-decoration-color: $hg-button-primary-hover-text;
            }
        }
    }
}
</style>
