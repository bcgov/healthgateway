<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEdit, faEllipsisV } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import EventBus, { EventMessageName } from "@/eventbus";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import { ResultError } from "@/models/requestResult";
import User from "@/models/user";
import UserNote from "@/models/userNote";

import EntrycardTimelineComponent from "./entrycard.vue";

library.add(faEdit, faEllipsisV);

@Component({
    components: {
        EntryCard: EntrycardTimelineComponent,
    },
})
export default class NoteTimelineComponent extends Vue {
    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("deleteNote", { namespace: "note" })
    deleteNote!: (params: { hdid: string; note: UserNote }) => Promise<void>;

    @Getter("user", { namespace: "user" }) user!: User;

    @Prop() entry!: NoteTimelineEntry;
    @Prop() isMobileDetails!: boolean;

    private isSaving = false;
    private eventBus = EventBus;

    private get canShowDetails(): boolean {
        return (
            this.entry.text.length > 0 &&
            this.entry.text !== this.entry.textSummary
        );
    }

    private handleDelete(): void {
        if (confirm("Are you sure you want to delete this note?")) {
            this.isSaving = true;
            this.deleteNote({
                hdid: this.user.hdid,
                note: this.entry.toModel(),
            })
                .catch((err: ResultError) => {
                    this.addError({
                        errorType: ErrorType.Delete,
                        source: ErrorSourceType.Note,
                        traceId: err.traceId,
                    });
                })
                .finally(() => {
                    this.isSaving = false;
                });
        }
    }

    private handleEdit() {
        this.eventBus.$emit(EventMessageName.EditNote, this.entry);
    }
}
</script>

<template>
    <EntryCard
        :card-id="'note-' + entry.title"
        entry-icon="edit"
        :icon-class="'note-icon'"
        :title="entry.title"
        :subtitle="entry.textSummary"
        :entry="entry"
        :allow-comment="false"
        :can-show-details="canShowDetails"
        :is-mobile-details="isMobileDetails"
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
    </EntryCard>
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
