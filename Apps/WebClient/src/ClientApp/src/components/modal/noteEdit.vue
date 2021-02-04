<script lang="ts">
import {
    faEdit,
    faEllipsisV,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import LoadingComponent from "@/components/loading.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import User from "@/models/user";
import UserNote from "@/models/userNote";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserNoteService } from "@/services/interfaces";

@Component({
    components: {
        LoadingComponent,
        DatePickerComponent,
    },
})
export default class NoteEditComponent extends Vue {
    @Getter("user", { namespace: "user" }) user!: User;

    private entry?: NoteTimelineEntry;
    private noteService!: IUserNoteService;
    private text = "";
    private title = "";
    private dateString: string = new DateWrapper().toISODate();

    private isSaving = false;
    private errorMessage = "";
    private eventBus = EventBus;

    private isVisible = false;

    private isNewNote = true;

    private get entryIcon(): IconDefinition {
        return faEdit;
    }

    private get modalTitle(): string {
        return this.isNewNote ? "Add Note" : "Update Note";
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }

    private mounted() {
        this.noteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );
        this.clear();
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

    public showModal(entry?: NoteTimelineEntry): void {
        console.log(entry);
        this.clear();
        if (entry) {
            this.entry = entry;
            this.text = entry.text;
            this.title = entry.title;
            this.dateString = entry.date.toISODate();
            this.isNewNote = false;
        }
        this.isVisible = true;
    }

    public hideModal(): void {
        this.$v.$reset();
        this.isVisible = false;
        this.clear();
        this.eventBus.$emit(EventMessageName.TimelineNoteEditClose);
    }

    private updateNote() {
        let entry = this.entry as NoteTimelineEntry;
        this.isSaving = true;
        this.noteService
            .updateNote(this.user.hdid, {
                id: entry.id,
                text: this.text,
                title: this.title,
                journalDateTime: new DateWrapper(this.dateString).toISODate(),
                version: entry.version as number,
                hdId: this.user.hdid,
            })
            .then((result) => {
                this.errorMessage = "";
                this.onNoteUpdated(result);
                this.handleSubmit();
            })
            .catch((err) => {
                this.errorMessage = err;
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
                console.log(result);
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
        console.log("EMIIITIGNG");
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

    private handleOk(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        this.$v.$touch();
        if (this.$v.$invalid) {
            return;
        } else if (this.isNewNote) {
            this.createNote();
        } else {
            this.updateNote();
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
            <b-row class="w-100 h-100">
                <b-col cols="auto">
                    <div class="icon">
                        <font-awesome-icon
                            :icon="entryIcon"
                            size="lg"
                        ></font-awesome-icon>
                    </div>
                </b-col>
                <b-col>
                    <h5>{{ modalTitle }}</h5>
                </b-col>
            </b-row>
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
                        @input="checkBlankNote()"
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
                        @input="checkBlankNote()"
                    ></b-form-textarea>
                </b-col>
            </b-row>
        </form>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <b-btn
                        data-testid="saveNoteBtn"
                        variant="primary"
                        type="submit"
                        :disabled="isSaving"
                        @click="handleOk"
                        >Save</b-btn
                    >
                </div>
                <div>
                    <b-btn
                        data-testid="cancelRegistrationBtn"
                        variant="secondary"
                        :disabled="isSaving"
                        @click="hideModal"
                        >Cancel</b-btn
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

.icon {
    color: white;
    text-align: center;
    padding-top: 0px;
    font-size: 1em;
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
