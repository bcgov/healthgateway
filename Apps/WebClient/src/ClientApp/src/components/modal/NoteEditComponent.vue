<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEdit } from "@fortawesome/free-solid-svg-icons";
import { BaseValidation, useVuelidate } from "@vuelidate/core";
import { required } from "@vuelidate/validators";
import { computed, nextTick, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import TooManyRequestsComponent from "@/components/TooManyRequestsComponent.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import User from "@/models/user";
import UserNote from "@/models/userNote";

library.add(faEdit);

const defaultDateString = new DateWrapper().toISODate();
const unsavedChangesText =
    "You have unsaved changes. Are you sure you want to leave?";

const store = useStore();

const entry = ref<NoteTimelineEntry>();
const text = ref("");
const title = ref("");
const dateString = ref(defaultDateString);
const isSaving = ref(false);
const errorMessage = ref("");
const isVisible = ref(false);
const isNewNote = ref(true);
const isDateStringValidDate = ref(true);

const user = computed<User>(() => store.getters["user/user"]);
const isIdleWarningVisible = computed<boolean>(
    () => store.getters["idle/isVisible"]
);

const modalTitle = computed(() =>
    isNewNote.value ? "Add Note" : "Update Note"
);
const isBlankNote = computed(() => text.value === "" && title.value === "");

const validations = computed(() => ({
    title: {
        required,
    },
    dateString: {
        required,
    },
}));

const v$ = useVuelidate(validations, { title, dateString });

function createNote(hdid: string, note: UserNote): Promise<UserNote> {
    return store.dispatch("note/createNote", { hdid, note });
}

function updateNote(hdid: string, note: UserNote): Promise<UserNote> {
    return store.dispatch("note/updateNote", { hdid, note });
}

function setSelectedDate(date: DateWrapper): void {
    store.dispatch("timeline/setSelectedDate", date);
}

function clearFilter(): void {
    store.dispatch("timeline/clearFilter");
}

function setTooManyRequestsError(key: string): void {
    store.dispatch("errorBanner/setTooManyRequestsError", { key });
}

function onBrowserClose(event: BeforeUnloadEvent): void {
    if (isVisible.value && !isIdleWarningVisible.value && !isBlankNote.value) {
        event.returnValue = unsavedChangesText;
    }
}

function isValid(param: BaseValidation): boolean | undefined {
    return param.$dirty ? !param.$invalid : undefined;
}

function editNote(noteTimelineEntry: NoteTimelineEntry): void {
    clear();
    entry.value = noteTimelineEntry;
    text.value = noteTimelineEntry.text;
    title.value = noteTimelineEntry.title;
    dateString.value = noteTimelineEntry.date.toISODate();
    isNewNote.value = false;
    isVisible.value = true;
}

function newNote(): void {
    clear();
    isNewNote.value = true;
    isVisible.value = true;
}

function hideModal(): void {
    v$.value.$reset();
    isVisible.value = false;
    clear();
}

function update(): void {
    if (entry.value === undefined) {
        return;
    }

    isSaving.value = true;
    updateNote(user.value.hdid, {
        id: entry.value.id,
        text: text.value,
        title: title.value,
        journalDate: new DateWrapper(dateString.value).toISODate(),
        version: entry.value.version as number,
        hdId: user.value.hdid,
    })
        .then(() => {
            errorMessage.value = "";
            handleSubmit();
        })
        .catch((error: ResultError) => {
            if (error.statusCode === 429) {
                setTooManyRequestsError("noteEditModal");
            } else {
                errorMessage.value = error.resultMessage;
            }
        })
        .finally(() => {
            isSaving.value = false;
        });
}

function create(): void {
    isSaving.value = true;
    createNote(user.value.hdid, {
        text: text.value,
        title: title.value,
        journalDate: new DateWrapper(dateString.value).toISODate(),
        hdId: user.value.hdid,
        version: 0,
    })
        .then((result) => {
            if (result) {
                errorMessage.value = "";
                onNoteAdded(result);
                handleSubmit();
            }
        })
        .catch((err: ResultError) => {
            if (err.statusCode === 429) {
                setTooManyRequestsError("noteEditModal");
            } else {
                errorMessage.value = err.resultMessage;
            }
        })
        .finally(() => {
            isSaving.value = false;
        });
}

function onNoteAdded(note: UserNote): void {
    clearFilter();
    setSelectedDate(new DateWrapper(note.journalDate));
}

function handleOk(bvModalEvt: Event): void {
    // Prevent modal from closing
    bvModalEvt.preventDefault();
    v$.value.$touch();
    if (v$.value.$invalid || !isDateStringValidDate.value) {
        return;
    } else if (isNewNote.value) {
        create();
    } else {
        update();
    }
}

async function handleSubmit(): Promise<void> {
    await nextTick();

    hideModal();
}

function clear(): void {
    entry.value = undefined;
    text.value = "";
    title.value = "";
    dateString.value = defaultDateString;

    isSaving.value = false;
    errorMessage.value = "";
    isNewNote.value = true;
}

function touchDate(): void {
    v$.value.dateString.$touch();
}

watch(dateString, () => touchDate());

EventBus.$on(EventMessageName.EditNote, editNote);
EventBus.$on(EventMessageName.CreateNote, newNote);

window.addEventListener("beforeunload", onBrowserClose);
</script>

<template>
    <b-modal
        id="note-edit-modal"
        v-model="isVisible"
        data-testid="noteEditModal"
        size="lg"
        header-class="edit-modal-header"
        header-text-variant="light"
        centered
        @hidden="clear"
    >
        <TooManyRequestsComponent location="noteEditModal" />
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
                <a href="mailto:HealthGateway@gov.bc.ca"
                    >HealthGateway@gov.bc.ca</a
                >.
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
                        :state="isValid(v$.title)"
                        @blur.native="v$.title.$touch()"
                    />
                    <b-form-invalid-feedback :state="isValid(v$.title)">
                        Title is required
                    </b-form-invalid-feedback>
                </b-col>
                <b-col class="col-sm-5 col-12 pt-3 pt-sm-0">
                    <DatePickerComponent
                        id="date"
                        :value="dateString"
                        data-testid="noteDateInput"
                        :state="isValid(v$.dateString)"
                        @blur="touchDate()"
                        @is-date-valid="isDateStringValidDate = $event"
                        @update:value="(value) => (dateString = value)"
                    />
                    <b-form-invalid-feedback :state="isValid(v$.dateString)">
                        Date is required.
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
                        :disabled="
                            isSaving ||
                            !isDateStringValidDate ||
                            !isValid(v$.dateString)
                        "
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
