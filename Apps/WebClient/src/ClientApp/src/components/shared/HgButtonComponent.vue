<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class HgButtonComponent extends Vue {
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
            case "success":
                result.push("hg-button", "hg-success");
                break;
            case "link":
                result.push("hg-button", "hg-link");
                break;
            case "link-danger":
                result.push("hg-button", "hg-link hg-link-danger");
                break;
            case "icon":
                result.push("hg-button", "hg-icon");
                break;
            case "icon-light":
                result.push("hg-button", "hg-icon hg-icon-light");
                break;
            case "icon-input":
                result.push("hg-button", "hg-icon hg-icon-input");
                break;
            case "icon-input-light":
                result.push(
                    "hg-button",
                    "hg-icon hg-icon-input hg-icon-input-light"
                );
                break;
            case "nav":
                result.push("hg-button", "hg-nav", "btn-block");
                break;
            case "carousel":
                result.push("hg-button", "hg-carousel");
                break;
            case "danger":
                result.push("hg-button", "hg-danger");
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
            case "success":
                return "success";
            case "link":
                return "link";
            case "nav":
                return "nav";
            case "danger":
                return "danger";
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
@use "sass:color";
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

        &:hover:not([disabled]),
        &:active:not([disabled]) {
            background: $hg-button-primary-hover;
            border-color: $hg-button-primary-hover-border;
            color: $hg-button-primary-hover-text;
            text-decoration-color: $hg-button-primary-hover-text;
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

        &:hover:not([disabled]),
        &:active:not([disabled]) {
            background: $hg-button-secondary-hover;
            border-color: $hg-button-secondary-hover-border;
            color: $hg-button-secondary-hover-text;
            text-decoration-color: $hg-button-secondary-hover-text;
            text-decoration-line: underline;
        }

        &.selected {
            background-color: $hg-button-secondary-selected;
            color: $hg-button-secondary-selected-text;
            border-color: $hg-button-secondary-selected-border;

            &:disabled {
                background: $hg-button-secondary-selected-disabled;
                border-color: $hg-button-secondary-selected-disabled-border;
                color: $hg-button-secondary-selected-disabled-text;
            }

            &:hover:not([disabled]),
            &:active:not([disabled]) {
                background: $hg-button-secondary-selected-hover;
                border-color: $hg-button-secondary-selected-hover-border;
                color: $hg-button-secondary-selected-hover-text;
                text-decoration-color: $hg-button-secondary-selected-hover-text;
                text-decoration-line: underline;
            }
        }
    }

    &.hg-success {
        background: $hg-button-success;
        border-color: $hg-button-success-border;
        color: $hg-button-success-text;

        &:disabled {
            background: $hg-button-success-disabled;
            border-color: $hg-button-success-disabled-border;
            color: $hg-button-success-disabled-text;
        }

        &:hover:not([disabled]),
        &:active:not([disabled]) {
            background: $hg-button-success-hover;
            border-color: $hg-button-success-hover-border;
            color: $hg-button-success-hover-text;
            text-decoration-color: $hg-button-success-hover-text;
        }
    }

    &.hg-link {
        background: $hg-button-link;
        color: $hg-button-link-text;
        text-decoration-color: $hg-button-link-text;
        text-decoration-line: underline;
        border: 0px;

        &:disabled {
            background: $hg-button-link;
            color: $hg-button-link-disabled-text;
            text-decoration-color: $hg-button-link-disabled-text;
        }

        &:hover:not([disabled]),
        &:active:not([disabled]) {
            background: $hg-button-link;
            color: $hg-button-link-hover-text;
            text-decoration-color: $hg-button-link-hover-text;
        }

        &:focus {
            outline: revert;
            box-shadow: revert;
        }
    }

    &.hg-link-danger {
        color: $hg-button-link-danger-text;
        text-decoration-color: $hg-button-link-danger-text;

        &:hover:not([disabled]),
        &:active:not([disabled]) {
            color: $hg-button-link-danger-hover-text;
            text-decoration-color: $hg-button-link-danger-hover-text;
        }

        &:focus {
            outline: revert;
            box-shadow: revert;
        }
    }

    &.hg-icon {
        background: none;
        border: none;
        text-decoration-line: none;

        &.selected {
            color: $hg-button-icon-selected-text;
            background-color: $hg-button-icon-selected;
            text-decoration-line: none;
        }

        &:disabled {
            color: $hg-button-icon-disabled-text;
        }
    }

    &.hg-icon-light {
        color: $hg-button-icon-light-text;
    }

    &.hg-icon-input {
        text-align: center;
        align-content: center;
        color: $hg-button-icon-input-text;
        background-color: $hg-button-icon-input-background;

        border: 1px solid $hg-button-icon-input-border;
        border-left: 0px;

        &:disabled {
            color: $hg-button-icon-input-disabled-text;
            background-color: $hg-button-icon-input-disabled-background;
            opacity: 1;
        }

        &:hover:enabled {
            color: $hg-button-icon-input-hover-text;
        }
    }

    &.hg-icon-input-light {
        color: $hg-button-icon-input-light-text;

        &:hover:enabled {
            color: $hg-button-icon-input-light-hover-text;
        }
    }

    &.hg-carousel {
        background-color: transparent;
        border: none;
        border-radius: 0;

        &:focus,
        &:active,
        &:focus:active {
            box-shadow: none;
            background-color: color.adjust($hg-brand-primary, $alpha: -0.7);
            color: white;
        }
    }

    &.hg-small {
        width: 80px;
    }

    &.hg-nav {
        //padding: 0;
        border-radius: 0;
        background: $hg-button-nav;
        color: $hg-button-nav-text;
        border: none;

        &.selected {
            color: $hg-button-nav-selected-text;
            background-color: $hg-button-nav-selected;
            text-decoration-line: none;
        }

        &:hover:not([disabled], .selected),
        &:active:not([disabled], .selected) {
            color: $hg-button-nav-hover-text;
            background-color: $hg-button-nav-hover;
            text-decoration-line: underline;
            text-decoration-color: $hg-button-nav-hover-text;
        }
    }
}
</style>
