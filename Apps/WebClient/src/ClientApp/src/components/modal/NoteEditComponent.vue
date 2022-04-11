<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEdit } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import User from "@/models/user";
import UserNote from "@/models/userNote";

library.add(faEdit);

@Component({
    components: {
        LoadingComponent,
        DatePickerComponent,
    },
})
export default class NoteEditComponent extends Vue {
    @Action("createNote", { namespace: "note" }) createNote!: (params: {
        hdid: string;
        note: UserNote;
    }) => Promise<UserNote>;
    @Action("updateNote", { namespace: "note" }) updateNote!: (params: {
        hdid: string;
        note: UserNote;
    }) => Promise<UserNote>;

    @Action("setSelectedDate", { namespace: "timeline" }) setSelectedDate!: (
        date: DateWrapper
    ) => void;

    @Action("clearFilter", { namespace: "timeline" }) clearFilter!: () => void;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isVisible", { namespace: "idle" }) isIdleWarningVisible!: boolean;

    private entry?: NoteTimelineEntry;
    private text = "";
    private title = "";
    private dateString: string = new DateWrapper().toISODate();

    private isSaving = false;
    private errorMessage = "";
    private eventBus = EventBus;

    private isVisible = false;

    private isNewNote = true;

    private readonly unsavedChangesText =
        "You have unsaved changes. Are you sure you want to leave?";

    private get modalTitle(): string {
        return this.isNewNote ? "Add Note" : "Update Note";
    }

    private get isBlankNote(): boolean {
        return this.text === "" && this.title === "";
    }

    private mounted() {
        this.clear();
        this.eventBus.$on(EventMessageName.EditNote, this.editNote);
        this.eventBus.$on(EventMessageName.CreateNote, this.newNote);

        window.addEventListener("beforeunload", this.onBrowserClose);
    }

    private onBrowserClose(event: BeforeUnloadEvent) {
        if (this.isVisible && !this.isIdleWarningVisible && !this.isBlankNote) {
            event.returnValue = this.unsavedChangesText;
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

    public editNote(entry: NoteTimelineEntry): void {
        this.clear();
        this.entry = entry;
        this.text = entry.text;
        this.title = entry.title;
        this.dateString = entry.date.toISODate();
        this.isNewNote = false;
        this.isVisible = true;
    }

    public newNote(): void {
        this.clear();
        this.isNewNote = true;
        this.isVisible = true;
    }

    public hideModal(): void {
        this.$v.$reset();
        this.isVisible = false;
        this.clear();
    }

    private update() {
        let entry = this.entry as NoteTimelineEntry;
        this.isSaving = true;
        this.updateNote({
            hdid: this.user.hdid,
            note: {
                id: entry.id,
                text: this.text,
                title: this.title,
                journalDate: new DateWrapper(this.dateString).toISODate(),
                version: entry.version as number,
                hdId: this.user.hdid,
            },
        })
            .then(() => {
                this.errorMessage = "";
                this.handleSubmit();
            })
            .catch((err) => {
                this.errorMessage = err;
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    private create() {
        this.isSaving = true;
        this.createNote({
            hdid: this.user.hdid,
            note: {
                text: this.text,
                title: this.title,
                journalDate: new DateWrapper(this.dateString).toISODate(),
                hdId: this.user.hdid,
                version: 0,
            },
        })
            .then((result) => {
                if (result) {
                    this.errorMessage = "";
                    this.onNoteAdded(result);
                    this.handleSubmit();
                }
            })
            .catch((err) => {
                this.errorMessage = err;
            })
            .finally(() => {
                this.isSaving = false;
            });
    }

    private onNoteAdded(note: UserNote) {
        this.clearFilter();
        this.setSelectedDate(new DateWrapper(note.journalDate));
    }

    private handleOk(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        this.$v.$touch();
        if (this.$v.$invalid) {
            return;
        } else if (this.isNewNote) {
            this.create();
        } else {
            this.update();
        }
    }

    private handleSubmit() {
        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
    }

    private clear() {
        this.entry = undefined;
        this.text = "";
        this.title = "";
        this.dateString = new DateWrapper().toISODate();

        this.isSaving = false;
        this.errorMessage = "";
        this.isNewNote = true;
    }
}
</script>

<template>
    <b-modal
        id="note-edit-modal"
        v-model="isVisible"
        data-testid="noteEditModal"
        content-class="mt-5"
        size="lg"
        header-class="edit-modal-header"
        header-text-variant="light"
        centered
        @hidden="clear"
    >
        <b-alert
            data-testid="noteEditErrorBanner"
            variant="danger"
            dismissible
            class="no-print"
            :show="!!errorMessage"
        >
            <p data-testid="noteEditErrorText">{{ errorMessage }}</p>
            <span>
                If you continue to have issues, please contact
                HealthGateway@gov.bc.ca.
            </span>
        </b-alert>
        <template #modal-header>
            <h5 class="mb-0">
                <hg-icon icon="edit" size="large" class="mr-2" />
                <span>{{ modalTitle }}</span>
            </h5>
        </template>
        <form>
            <b-row>
                <b-col class="col-sm-7 col-12 pr-3 pr-sm-0">
                    <b-form-input
                        id="title"
                        ref="titleInput"
                        v-model="title"
                        data-testid="noteTitleInput"
                        type="text"
                        placeholder="Title"
                        maxlength="100"
                        :state="isValid($v.title)"
                        @blur.native="$v.title.$touch()"
                    />
                    <b-form-invalid-feedback :state="isValid($v.title)">
                        Title is required
                    </b-form-invalid-feedback>
                </b-col>
                <b-col class="col-sm-5 col-12 pt-3 pt-sm-0">
                    <DatePickerComponent
                        id="date"
                        v-model="dateString"
                        data-testid="noteDateInput"
                        :state="isValid($v.dateString)"
                        @blur="$v.dateString.$touch()"
                    />
                    <b-form-invalid-feedback :state="isValid($v.dateString)">
                        Invalid Date
                    </b-form-invalid-feedback>
                </b-col>
            </b-row>
            <b-row class="pt-3">
                <b-col>
                    <b-form-textarea
                        id="text"
                        v-model="text"
                        data-testid="noteTextInput"
                        placeholder="Enter your note here. Your notes are only available for your own viewing."
                        rows="3"
                        max-rows="6"
                        maxlength="1000"
                    ></b-form-textarea>
                </b-col>
            </b-row>
        </form>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <hg-button
                        data-testid="cancelRegistrationBtn"
                        variant="secondary"
                        :disabled="isSaving"
                        @click="hideModal"
                        >Cancel</hg-button
                    >
                </div>
                <div>
                    <hg-button
                        data-testid="saveNoteBtn"
                        variant="primary"
                        type="submit"
                        :disabled="isSaving"
                        @click="handleOk"
                        >Save</hg-button
                    >
                </div>
            </b-row>
        </template>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.entryTitle {
    background-color: $soft_background;
    color: $primary;
    padding: 10px 15px;
    font-weight: bold;
    word-wrap: break-word;
    width: 100%;
    margin-right: -1px;
}

.editableEntryTitle {
    background-color: $soft_background;
    padding: 9px 0px 9px 15px;
    width: 100%;
    margin: 0px;
    margin-right: -1px;
}

.entryDetails {
    word-wrap: break-word;
    padding-left: 15px;
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
.edit-modal-header {
    background-color: $bcgold;
}
</style>
