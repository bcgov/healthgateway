<script setup lang="ts">
import { computed, ref } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import TimelineEntryComponent from "@/components/private/timeline/TimelineEntryComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { ResultError } from "@/models/errors";
import NoteTimelineEntry from "@/models/timeline/noteTimelineEntry";
import { useErrorStore } from "@/stores/error";
import { EventName, useEventStore } from "@/stores/event";
import { useNoteStore } from "@/stores/note";

interface Props {
    hdid: string;
    entry: NoteTimelineEntry;
    isMobileDetails?: boolean;
    commentsAreEnabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    isMobileDetails: false,
    commentsAreEnabled: false,
});

const errorStore = useErrorStore();
const eventStore = useEventStore();
const noteStore = useNoteStore();

const isSaving = ref(false);

const entryIcon = computed(() => entryTypeMap.get(EntryType.Note)?.icon);
const canShowDetails = computed(
    () =>
        props.entry.text.length > 0 &&
        props.entry.text !== props.entry.textSummary
);

function handleDelete(): void {
    if (confirm("Are you sure you want to delete this note?")) {
        isSaving.value = true;
        noteStore
            .deleteNote(props.hdid, props.entry.toModel())
            .catch((err: ResultError) => {
                if (err.statusCode !== 429) {
                    errorStore.addError(
                        ErrorType.Delete,
                        ErrorSourceType.Note,
                        err.traceId
                    );
                }
            })
            .finally(() => {
                isSaving.value = false;
            });
    }
}

function handleEdit(): void {
    eventStore.emit(EventName.OpenNoteDialog, props.entry);
}
</script>

<template>
    <TimelineEntryComponent
        :card-id="'note-' + entry.title"
        :entry-icon="entryIcon"
        icon-class="bg-accent"
        :title="entry.title"
        :subtitle="entry.textSummary"
        :entry="entry"
        :can-show-details="canShowDetails"
        :is-mobile-details="isMobileDetails"
        :allow-comment="false"
    >
        <template #header-menu>
            <v-menu location="bottom" :disabled="isSaving">
                <template #activator="{ props: slotProps }">
                    <HgIconButtonComponent
                        v-bind="slotProps"
                        data-testid="noteMenuBtn"
                        class="mt-n2 mb-n1"
                        icon="ellipsis-v"
                        size="small"
                        :disabled="isSaving"
                    />
                </template>
                <v-list>
                    <v-list-item
                        data-testid="editNoteMenuBtn"
                        title="Edit"
                        @click="handleEdit"
                    />
                    <v-list-item
                        data-testid="deleteNoteMenuBtn"
                        class="menuItem"
                        title="Delete"
                        @click="handleDelete"
                    />
                </v-list>
            </v-menu>
        </template>
        {{ entry.text }}
    </TimelineEntryComponent>
</template>
