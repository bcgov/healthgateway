<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import TutorialComponent from "@/components/shared/TutorialComponent.vue";
import UserPreferenceType from "@/constants/userPreferenceType";
import EventBus, { EventMessageName } from "@/eventbus";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        TutorialComponent,
    },
};

@Component(options)
export default class AddNoteButtonComponent extends Vue {
    private eventBus = EventBus;

    private get noteTutorialPreference(): string {
        return UserPreferenceType.TutorialNote;
    }

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
        <TutorialComponent
            :preference-type="noteTutorialPreference"
            target="addNoteBtn"
            placement="bottom"
        >
            <div data-testid="notesPopover">
                Add your own notes to track important details, such as health
                visit reason, medication side effect, etc.
            </div>
        </TutorialComponent>
    </div>
</template>
