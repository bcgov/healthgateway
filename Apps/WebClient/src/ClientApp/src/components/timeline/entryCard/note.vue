<script lang="ts">
import {
    faEdit,
    faEllipsisV,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import BannerError from "@/models/bannerError";
import { DateWrapper } from "@/models/dateWrapper";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import User from "@/models/user";
import UserNote from "@/models/userNote";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserNoteService } from "@/services/interfaces";
import ErrorTranslator from "@/utility/errorTranslator";

import EntrycardTimelineComponent from "./entrycard.vue";

// TODO: Task 9998
@Component({
    components: {
        DatePickerComponent,
        EntryCard: EntrycardTimelineComponent,
    },
})
export default class NoteTimelineComponent extends Vue {
    @Prop() isAddMode!: boolean;
    @Prop() entry!: NoteTimelineEntry;
    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;
    @Getter("user", { namespace: "user" }) user!: User;

    private noteService!: IUserNoteService;
    private text = "";
    private title = "";
    private dateString: string = new DateWrapper().toISODate();
    private detailsVisible = false;
    private isEditMode = false;
    private isSaving = false;
    private eventBus = EventBus;

    private mounted() {
        this.noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );

        if (this.isAddMode) {
            this.$nextTick().then(() => {
                let container = this.$refs["titleInput"] as HTMLElement;
                container.focus();
            });
        }
    }

    private validations() {
        return {
            title: {
                required: required,
            },
            dateString: {
                required: required,
            },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private checkBlankNote() {
        if (this.text === "" && this.title === "") {
            this.eventBus.$emit(EventMessageName.IsNoteBlank, true);
        } else {
            this.eventBus.$emit(EventMessageName.IsNoteBlank, false);
        }
    }

    private get entryIcon(): IconDefinition {
        return faEdit;
    }

    private get isEditing(): boolean {
        return this.isAddMode || this.isEditMode;
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }
    private toggleDetails(): void {
        this.detailsVisible = !this.detailsVisible;
    }

    private onSubmit(evt: Event): void {
        evt.preventDefault();
        this.$v.$touch();
        if (this.$v.$invalid) {
            return;
        } else if (this.isEditMode) {
            this.updateNote();
        } else if (this.isAddMode) {
            this.createNote();
        }
    }

    private updateNote() {
        this.isSaving = true;
        this.noteService
            .updateNote(this.user.hdid, {
                id: this.entry.id,
                text: this.text,
                title: this.title,
                journalDateTime: new DateWrapper(this.dateString).toISODate(),
                version: this.entry.version as number,
                hdId: this.user.hdid,
            })
            .then((result) => {
                this.isEditMode = false;
                this.onNoteUpdated(result);
            })
            .catch((err) => {
                this.addError(
                    ErrorTranslator.toBannerError("Update Note Error", err)
                );
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    private createNote() {
        this.isSaving = true;
        this.noteService
            .createNote(this.user.hdid, {
                text: this.text,
                title: this.title,
                journalDateTime: new DateWrapper(this.dateString).toISODate(),
                hdId: this.user.hdid,
                version: 0,
            })
            .then((result) => {
                if (result) {
                    this.onNoteAdded(result);
                }
            })
            .catch((err) => {
                this.addError(
                    ErrorTranslator.toBannerError("Create Note Error", err)
                );
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    private editNote(): void {
        this.text = this.entry.text;
        this.title = this.entry.title;
        this.dateString = this.entry.date.toISODate();
        this.isEditMode = true;
        this.onEditStarted(this.entry);
    }

    private deleteNote(): void {
        this.text = this.entry.id;
        if (confirm("Are you sure you want to delete this note?")) {
            this.noteService
                .deleteNote(this.user.hdid, this.entry.toModel())
                .then(() => {
                    this.onNoteDeleted(this.entry);
                })
                .catch((err) => {
                    this.addError(
                        ErrorTranslator.toBannerError("Delete Note Error", err)
                    );
                });
        }
    }

    private onReset(): void {
        this.onEditClose(this.entry);
        this.isEditMode = false;
    }

    private onEditClose(note: NoteTimelineEntry) {
        if (this.isAddMode) {
            this.eventBus.$emit(EventMessageName.TimelineEntryAddClose, note);
        } else {
            this.eventBus.$emit(EventMessageName.TimelineEntryEditClose, note);
        }
    }

    private onEditStarted(note: NoteTimelineEntry) {
        this.eventBus.$emit(EventMessageName.TimelineEntryEdit, note);
    }

    private onNoteDeleted(note: NoteTimelineEntry) {
        this.eventBus.$emit(EventMessageName.TimelineEntryDeleted, note);
    }

    private onNoteAdded(note: UserNote) {
        this.eventBus.$emit(
            EventMessageName.TimelineEntryAdded,
            new NoteTimelineEntry(note)
        );
    }

    private onNoteUpdated(note: UserNote) {
        this.eventBus.$emit(
            EventMessageName.TimelineEntryUpdated,
            new NoteTimelineEntry(note)
        );
    }
}
</script>

<template>
    <EntryCard
        :card-id="'note-' + title"
        :entry-icon="entryIcon"
        :icon-class="'note-icon'"
        :title="entry.title"
        :entry="entry"
        :allow-comment="false"
    >
        <div slot="header-description">
            {{ !detailsVisible ? entry.textSummary : entry.text }}
        </div>

        <b-row slot="details-body" class="justify-content-between">
            <b-col v-if="!isEditing" class="entryDetails">
                {{ !detailsVisible ? entry.textSummary : entry.text }}
                <b-btn
                    v-b-toggle="'entryDetails-' + entry.id"
                    variant="link"
                    class="detailsButton"
                    @click="toggleDetails()"
                >
                    <span
                        v-if="detailsVisible && entry.textSummary != entry.text"
                        >Hide Details</span
                    >
                    <span v-else-if="entry.textSummary != entry.text"
                        >Read More</span
                    >
                </b-btn>
            </b-col>
        </b-row>
    </EntryCard>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

$radius: 15px;

.timelineCard {
    border-radius: $radius $radius $radius $radius;
    border-color: $soft_background;
    border-style: solid;
    border-width: 2px;
}

.entryTitle {
    background-color: $soft_background;
    color: $primary;
    padding: 10px 15px;
    font-weight: bold;
    word-wrap: break-word;
    width: 100%;
    margin-right: -1px;
    border-radius: 0px $radius 0px 0px;
}

.editableEntryTitle {
    background-color: $soft_background;
    padding: 9px 0px 9px 15px;
    width: 100%;
    margin: 0px;
    margin-right: -1px;
    border-radius: 0px $radius 0px 0px;
}

.entryDetails {
    word-wrap: break-word;
    padding-left: 15px;
}

.leftPane {
    width: 60px;
    max-width: 60px;
}

.detailsButton {
    padding: 0px;
}

.detailSection {
    margin-top: 15px;
}

.editableEntryDetails {
    padding-left: 30px;
    padding-right: 20px;
}

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
