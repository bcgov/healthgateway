<script lang="ts">
import {
    faEdit,
    faEllipsisV,
    faFlask,
    faLongArrowAltLeft,
    faPills,
    faQuestion,
    faSyringe,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import LoadingComponent from "@/components/loading.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import User from "@/models/user";
import UserNote from "@/models/userNote";

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

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isVisible", { namespace: "idle" }) isIdleWarningVisible!: boolean;

    private entry?: TimelineEntry;
    private noteEntry?: NoteTimelineEntry;
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

    private get backButtonIcon(): IconDefinition {
        return faLongArrowAltLeft;
    }

    private get entryIcon(): IconDefinition {
        if (this.entry?.type == EntryType.Medication) {
            return faPills;
        }
        if (this.entry?.type == EntryType.Immunization) {
            return faSyringe;
        }
        if (this.entry?.type == EntryType.Laboratory) {
            return faFlask;
        }
        if (this.entry?.type == EntryType.Note) {
            return faEdit;
        }
        if (this.entry?.type == EntryType.Encounter) {
            return faSyringe;
        }
        return faQuestion;
    }

    private get modalTitle(): string {
        return "";
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }

    private get isBlankNote(): boolean {
        return this.text === "" && this.title === "";
    }

    private mounted() {
        this.clear();
        this.eventBus.$on(EventMessageName.EntryDetails, this.editNote);

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

    public viewDetails(entry: TimelineEntry): void {
        this.clear();
        this.entry = entry;
        //To-do set Entry Title and Sub-Title
        this.isVisible = true;
    }

    public editNote(entry: NoteTimelineEntry): void {
        this.clear();
        this.noteEntry = entry;
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
        let entry = this.noteEntry as NoteTimelineEntry;
        this.isSaving = true;
        this.updateNote({
            hdid: this.user.hdid,
            note: {
                id: entry.id,
                text: this.text,
                title: this.title,
                journalDateTime: new DateWrapper(this.dateString).toISODate(),
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
                journalDateTime: new DateWrapper(this.dateString).toISODate(),
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
        this.eventBus.$emit(
            EventMessageName.AddedNote,
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
        this.noteEntry = undefined;
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
        hide-footer
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
                            :icon="backButtonIcon"
                            size="lg"
                        ></font-awesome-icon>
                    </div>
                </b-col>
                <b-col>
                    <h5>{{ modalTitle }}</h5>
                </b-col>
            </b-row>
        </template>
        <b-row class="w-100 h-100">
            <b-col cols="auto">
                <div class="icon">
                    <font-awesome-icon
                        :icon="entryIcon"
                        size="2x"
                    ></font-awesome-icon>
                </div>
            </b-col>
            <b-col>
                <span>Entry Title</span>
                <span>Entry Sub-Title</span>
            </b-col>
            <b-col>
                <span>Feb 11, 2021</span>
            </b-col>
        </b-row>
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
    color: grey;
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
    background-color: white;
}
</style>
