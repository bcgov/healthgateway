<script setup lang="ts">
import EventBus, { EventMessageName } from "@/eventbus";

function createNote(): void {
    EventBus.$emit(EventMessageName.CreateNote);
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
