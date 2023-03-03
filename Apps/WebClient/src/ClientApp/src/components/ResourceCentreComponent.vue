<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faQuestion } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";

library.add(faQuestion);

@Component
export default class ResourceCentreComponent extends Vue {}
</script>

<template>
    <div
        class="hg-resource-centre position-sticky p-3 mt-auto align-self-end"
        data-testid="hg-resource-centre"
    >
        <b-dropdown
            dropup
            :popper-opts="{
                placement: 'top-end',
            }"
            no-caret
            variant="link"
        >
            <template #button-content>
                <hg-icon
                    icon="question"
                    size="large"
                    square
                    class="rounded-circle bg-white shadow p-3"
                />
            </template>
            <b-dropdown-text
                text-class="hg-resource-centre-title font-weight-bold"
            >
                Resource Centre
            </b-dropdown-text>
            <b-dropdown-divider />
            <b-dropdown-item
                href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                target="_blank"
            >
                FAQ
            </b-dropdown-item>
            <b-dropdown-item to="/release-notes">Release Notes</b-dropdown-item>
        </b-dropdown>
    </div>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.hg-resource-centre {
    bottom: 0;

    .hg-resource-centre-title {
        cursor: default;
        white-space: nowrap;
    }

    button.dropdown-toggle {
        color: $hg-button-resource-centre-text;

        &:hover:not([disabled]),
        &:active:not([disabled]),
        &:focus:not([disabled]) {
            color: $hg-button-resource-centre-hover-text;
        }
    }
}
</style>
