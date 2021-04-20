<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class HgButton extends Vue {
    @Prop({ required: true }) variant!: string;
    @Prop({ required: false, default: "" }) size!: string;

    private get classes(): string[] {
        let result = [];

        switch (this.variant) {
            case "primary":
                result.push("hg-button", "hg-primary");
                break;
            case "secondary":
                result.push("hg-button", "hg-secondary");
                break;
            default:
                return [];
        }

        if (this.size === "small") {
            result.push("hg-small");
        }

        return result;
    }

    private get bootstrapVariant(): string {
        switch (this.variant) {
            case "primary":
                return "primary";
            case "secondary":
                return "outline-primary";
            default:
                return "";
        }
    }
}
</script>

<template>
    <b-button
        v-bind="$attrs"
        :variant="bootstrapVariant"
        :class="classes"
        v-on="$listeners"
    >
        <slot></slot>
    </b-button>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.hg-button {
    // add text-decoration-color to transition
    transition: color 0.15s ease-in-out, background-color 0.15s ease-in-out,
        border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out,
        text-decoration-color 0.15s ease-in-out;

    text-decoration: underline;
    text-decoration-color: transparent;

    &.hg-primary {
        background: $hg-button-primary;
        border-color: $hg-button-primary-border;
        color: $hg-button-primary-text;

        &:disabled {
            background: $hg-button-primary-disabled;
            border-color: $hg-button-primary-disabled-border;
            color: $hg-button-primary-disabled-text;
        }

        &:hover,
        &:active {
            &:enabled {
                background: $hg-button-primary-hover;
                border-color: $hg-button-primary-hover-border;
                color: $hg-button-primary-hover-text;
                text-decoration-color: $hg-button-primary-hover-text;
            }
        }
    }

    &.hg-secondary {
        background: $hg-button-secondary;
        border-color: $hg-button-secondary-border;
        color: $hg-button-secondary-text;

        &:disabled {
            background: $hg-button-secondary-disabled;
            border-color: $hg-button-secondary-disabled-border;
            color: $hg-button-secondary-disabled-text;
        }

        &:hover,
        &:active {
            &:enabled {
                background: $hg-button-secondary-hover;
                border-color: $hg-button-secondary-hover-border;
                color: $hg-button-secondary-hover-text;
                text-decoration-color: $hg-button-secondary-hover-text;
            }
        }
    }

    &.hg-small {
        width: 80px;
    }
}
</style>
