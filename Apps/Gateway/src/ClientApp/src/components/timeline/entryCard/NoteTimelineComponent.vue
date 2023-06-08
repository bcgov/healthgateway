<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEllipsisV } from "@fortawesome/free-solid-svg-icons";
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import EntryCardTimelineComponent from "@/components/timeline/entryCard/EntrycardTimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import EventBus, { EventMessageName } from "@/eventbus";
import { ResultError } from "@/models/errors";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import UserNote from "@/models/userNote";

const store = useStore();

library.add(faEllipsisV);

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

const isSaving = ref(false);

const entryIcon = computed(() => {
    return entryTypeMap.get(EntryType.Note)?.icon;
});

const canShowDetails = computed(() => {
    return (
        props.entry.text.length > 0 &&
        props.entry.text !== props.entry.textSummary
    );
});

function addError(params: {
    errorType: ErrorType;
    source: ErrorSourceType;
    traceId: string | undefined;
}): void {
    store.dispatch("errorBanner/addError", params);
}

function deleteNote(params: { hdid: string; note: UserNote }): Promise<void> {
    return store.dispatch("note/deleteNote", params);
}

function handleDelete(): void {
    if (confirm("Are you sure you want to delete this note?")) {
        isSaving.value = true;
        deleteNote({
            hdid: props.hdid,
            note: props.entry.toModel(),
        })
            .catch((err: ResultError) => {
                if (err.statusCode !== 429) {
                    addError({
                        errorType: ErrorType.Delete,
                        source: ErrorSourceType.Note,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                isSaving.value = false;
            });
    }
}

function handleEdit(): void {
    EventBus.$emit(EventMessageName.EditNote, props.entry);
}
</script>

<template>
    <EntryCardTimelineComponent
        :card-id="'note-' + entry.title"
        :entry-icon="entryIcon"
        :icon-class="'note-icon'"
        :title="entry.title"
        :subtitle="entry.textSummary"
        :entry="entry"
        :can-show-details="canShowDetails"
        :is-mobile-details="isMobileDetails"
        :allow-comment="false"
    >
        <b-navbar-nav slot="header-menu">
            <b-nav-item-dropdown
                right
                text=""
                :no-caret="true"
                :disabled="isSaving"
            >
                <!-- Using 'button-content' slot -->
                <template slot="button-content">
                    <hg-icon
                        icon="ellipsis-v"
                        size="small"
                        data-testid="noteMenuBtn"
                        class="noteMenu"
                    />
                </template>
                <b-dropdown-item
                    data-testid="editNoteMenuBtn"
                    class="menuItem"
                    @click.stop="handleEdit()"
                >
                    Edit
                </b-dropdown-item>
                <b-dropdown-item
                    data-testid="deleteNoteMenuBtn"
                    class="menuItem"
                    @click.stop="handleDelete()"
                >
                    Delete
                </b-dropdown-item>
            </b-nav-item-dropdown>
        </b-navbar-nav>

        <span slot="details-body">
            {{ entry.text }}
        </span>
    </EntryCardTimelineComponent>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.noteMenu {
    color: $soft_text;
}
</style>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.note-icon {
    background-color: $bcgold !important;
}
</style>
