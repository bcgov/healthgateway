<script lang="ts">
import {
    faEdit,
    faEllipsisV,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import BannerError from "@/models/bannerError";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserNoteService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

import EntrycardTimelineComponent from "./entrycard.vue";

@Component({
    components: {
        DatePickerComponent,
        EntryCard: EntrycardTimelineComponent,
    },
})
export default class NoteTimelineComponent extends Vue {
    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    @Getter("user", { namespace: "user" }) user!: User;

    @Prop() entry!: NoteTimelineEntry;

    private noteService!: IUserNoteService;

    private isSaving = false;
    private eventBus = EventBus;

    private mounted() {
        this.noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );
    }

    private get entryIcon(): IconDefinition {
        return faEdit;
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }

    private get showDetailsButton(): boolean {
        return (
            this.entry.text.length > 0 &&
            this.entry.text !== this.entry.textSummary
        );
    }

    private deleteNote(): void {
        if (confirm("Are you sure you want to delete this note?")) {
            this.isSaving = true;
            this.noteService
                .deleteNote(this.user.hdid, this.entry.toModel())
                .then(() => {
                    this.onNoteDeleted(this.entry);
                })
                .catch((err) => {
                    this.addError(
                        ErrorTranslator.toBannerError("Delete Note Error", err)
                    );
                })
                .finally(() => {
                    this.isSaving = false;
                });
        }
    }

    private editNote() {
        this.eventBus.$emit(EventMessageName.TimelineEntryEdit, this.entry);
    }

    private onNoteDeleted(note: NoteTimelineEntry) {
        this.eventBus.$emit(EventMessageName.TimelineEntryDeleted, note);
    }
}
</script>

<template>
    <EntryCard
        :card-id="'note-' + entry.title"
        :entry-icon="entryIcon"
        :icon-class="'note-icon'"
        :title="entry.title"
        :subtitle="entry.textSummary"
        :entry="entry"
        :allow-comment="false"
        :show-details-button="showDetailsButton"
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
                    <font-awesome-icon
                        data-testid="noteMenuBtn"
                        class="noteMenu"
                        :icon="menuIcon"
                        size="1x"
                    ></font-awesome-icon>
                </template>
                <b-dropdown-item
                    data-testid="editNoteMenuBtn"
                    class="menuItem"
                    @click="editNote()"
                >
                    Edit
                </b-dropdown-item>
                <b-dropdown-item
                    data-testid="deleteNoteMenuBtn"
                    class="menuItem"
                    @click="deleteNote()"
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
