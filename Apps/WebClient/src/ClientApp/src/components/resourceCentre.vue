<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faQuestion } from "@fortawesome/free-solid-svg-icons";

import EventBus, { EventMessageName } from "@/eventbus";

library.add(faQuestion);

import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import { ImmunizationEvent } from "@/models/immunizationModel";

@Component
export default class ResourceCentreComponent extends Vue {
    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    private eventBus = EventBus;

    private showCovidCard(): void {
        this.eventBus.$emit(EventMessageName.TimelineCovidCard);
    }
}
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
            <b-dropdown-item-button
                :disabled="covidImmunizations.length === 0"
                @click="showCovidCard()"
                >COVID-19 Card</b-dropdown-item-button
            >
            <b-dropdown-item to="/faq">FAQ</b-dropdown-item>
            <b-dropdown-item to="/release-notes">Release Notes</b-dropdown-item>
        </b-dropdown>
    </div>
</template>

<style lang="scss">
// deliberately unscoped to affect button styling in dropdown

@import "@/assets/scss/_variables.scss";

.hg-resource-centre {
    bottom: 0;

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
