<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import EventBus, { EventMessageName } from "@/eventbus";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {};

@Component(options)
export default class AddNoteButtonComponent extends Vue {
    private eventBus = EventBus;

    private createNote(): void {
        this.eventBus.$emit(EventMessageName.CreateNote);
    }
}
</script>

<template>
    <div>
        <hg-button
            id="addNoteBtn"
            data-testid="addNoteBtn"
            variant="secondary"
            @click="createNote"
        >
            <hg-icon icon="edit" size="medium" class="mr-lg-2" square />
            <span class="d-none d-lg-inline">Add a Note</span>
        </hg-button>
    </div>
</template>
