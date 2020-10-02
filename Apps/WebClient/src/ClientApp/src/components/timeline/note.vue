<script lang="ts">
import Vue from "vue";
import EventBus, { EventMessageName } from "@/eventbus";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import { Component, Emit, Prop, PropSync } from "vue-property-decorator";
import { Action, Getter, State } from "vuex-class";
import {
    IconDefinition,
    faEdit,
    faEllipsisV,
} from "@fortawesome/free-solid-svg-icons";
import { IUserNoteService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import ErrorTranslator from "@/utility/errorTranslator";
import container from "@/plugins/inversify.config";
import UserNote from "@/models/userNote";
import User from "@/models/user";
import { DateWrapper } from "@/models/dateWrapper";
import BannerError from "@/models/bannerError";

@Component
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
        if (this.isEditMode) {
            this.updateNote();
        } else if (this.isAddMode) {
            this.createNote();
        }
    }

    private updateNote() {
        this.isSaving = true;
        this.noteService
            .updateNote({
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
            .createNote({
                text: this.text,
                title: this.title,
                journalDateTime: new DateWrapper(this.dateString).toISODate(),
                hdId: this.user.hdid,
                version: 0,
            })
            .then((result) => {
                this.onNoteAdded(result);
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
                .deleteNote(this.entry.toModel())
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
    <b-col class="timelineCard" :class="isEditing ? 'px-0' : ''">
        <b-form @submit="onSubmit" @reset="onReset">
            <b-row class="entryHeading">
                <b-col class="d-flex" :class="!isEditing ? 'px-0' : ''">
                    <div class="icon leftPane">
                        <font-awesome-icon
                            :icon="entryIcon"
                            size="2x"
                        ></font-awesome-icon>
                    </div>
                    <div v-if="!isEditing" class="entryTitle d-flex">
                        <div class="pt-1 w-100" data-testid="noteTitle">
                            {{ entry.title }}
                        </div>
                        <div>
                            <!-- Right aligned nav items -->
                            <b-navbar-nav class="ml-auto">
                                <b-nav-item-dropdown
                                    right
                                    text=""
                                    :no-caret="true"
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
                        </div>
                    </div>
                    <b-row v-else class="editableEntryTitle pr-1">
                        <b-col class="p-0 col-lg-7 col-md-7 col-6">
                            <b-form-input
                                id="title"
                                v-model="title"
                                data-testid="noteTitleInput"
                                type="text"
                                placeholder="Title"
                                maxlength="100"
                            />
                        </b-col>
                        <b-col class="p-0 pl-1 col-lg-5 col-md-5 col-6">
                            <b-form-input
                                id="date"
                                v-model="dateString"
                                data-testid="noteDateInput"
                                required
                                type="date"
                            />
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
            <b-row>
                <b-col class="leftPane"></b-col>
                <b-col>
                    <b-row>
                        <b-col v-if="!isEditing" class="entryDetails">
                            {{
                                !detailsVisible ? entry.textSummary : entry.text
                            }}
                            <b-btn
                                v-b-toggle="'entryDetails-' + entry.id"
                                variant="link"
                                class="detailsButton"
                                @click="toggleDetails()"
                            >
                                <span
                                    v-if="
                                        detailsVisible &&
                                        entry.textSummary != entry.text
                                    "
                                    >Hide Details</span
                                >
                                <span
                                    v-else-if="entry.textSummary != entry.text"
                                    >Read More</span
                                >
                            </b-btn>
                        </b-col>
                        <b-col v-if="isEditing" class="editableEntryDetails">
                            <b-form-textarea
                                id="text"
                                v-model="text"
                                data-testid="noteTextInput"
                                placeholder="Your note here"
                                rows="3"
                                max-rows="6"
                                maxlength="1000"
                            ></b-form-textarea>
                        </b-col>
                    </b-row>
                    <b-row v-if="isEditing" class="py-2 pr-1">
                        <b-col class="d-flex flex-row-reverse">
                            <div>
                                <b-btn
                                    data-testid="saveNoteBtn"
                                    variant="primary"
                                    type="submit"
                                    class="mr-2"
                                    :disabled="isSaving"
                                >
                                    <span>Save</span>
                                </b-btn>
                                <b-btn
                                    variant="light"
                                    type="reset"
                                    :disabled="isSaving"
                                >
                                    <span>Cancel</span>
                                </b-btn>
                            </div>
                        </b-col>
                    </b-row>
                </b-col>
            </b-row>
        </b-form>
    </b-col>
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

.icon {
    background-color: $bcgold;
    color: white;
    text-align: center;
    padding: 10px 15px;
    border-radius: $radius 0px 0px 0px;
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
