<script setup lang="ts">
import { useVuelidate } from "@vuelidate/core";
import { helpers, required } from "@vuelidate/validators";
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import TooManyRequestsComponent from "@/components/error/TooManyRequestsComponent.vue";
import { Loader } from "@/constants/loader";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper } from "@/models/dateWrapper";
import { ResultError } from "@/models/errors";
import NoteTimelineEntry from "@/models/timeline/noteTimelineEntry";
import { ILogger } from "@/services/interfaces";
import { useAppStore } from "@/stores/app";
import { useErrorStore } from "@/stores/error";
import { EventName, useEventStore } from "@/stores/event";
import { useLoadingStore } from "@/stores/loading";
import { useNoteStore } from "@/stores/note";
import { useTimelineStore } from "@/stores/timeline";
import { useUserStore } from "@/stores/user";
import ValidationUtil from "@/utility/validationUtil";

const defaultDateString = new DateWrapper().toISODate();

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const appStore = useAppStore();
const errorStore = useErrorStore();
const eventStore = useEventStore();
const loadingStore = useLoadingStore();
const noteStore = useNoteStore();
const timelineStore = useTimelineStore();
const userStore = useUserStore();

const isVisible = ref(false);
const entry = ref<NoteTimelineEntry>();
const noteTitle = ref("");
const noteDateString = ref(defaultDateString);
const noteText = ref("");
const isDateStringValidDate = ref(true);
const errorMessage = ref("");

const isLoading = computed(() => loadingStore.isLoading(Loader.NoteDialog));
const updatingExistingEntry = computed(() => entry.value !== undefined);
const isBlankNote = computed(() => !noteText.value && !noteTitle.value);
const validations = computed(() => ({
    noteTitle: {
        required: helpers.withMessage("Title is required.", required),
    },
    noteDateString: {
        required: helpers.withMessage("Date is required.", required),
    },
}));

const v$ = useVuelidate(validations, { noteTitle, noteDateString });

function openDialog(existingEntry?: NoteTimelineEntry): void {
    entry.value = existingEntry;
    noteText.value = existingEntry?.text ?? "";
    noteTitle.value = existingEntry?.title ?? "";
    noteDateString.value =
        existingEntry?.date?.toISODate() ?? defaultDateString;

    v$.value.$reset();
    errorMessage.value = "";

    if (updatingExistingEntry.value) {
        logger.debug("Opening dialog to edit note");
    } else {
        logger.debug("Opening dialog to create note");
    }
    isVisible.value = true;
}

function closeDialog(): void {
    isVisible.value = false;
}

function save(): void {
    v$.value.$touch();
    if (v$.value.$invalid || !isDateStringValidDate.value) {
        return;
    } else if (updatingExistingEntry.value) {
        updateNote();
    } else {
        createNote();
    }
}

function updateNote(): void {
    if (entry.value === undefined) {
        return;
    }

    loadingStore.applyLoader(
        Loader.NoteDialog,
        "updateNote",
        noteStore
            .updateNote(userStore.hdid, {
                id: entry.value.id,
                text: noteText.value,
                title: noteTitle.value,
                journalDate: new DateWrapper(noteDateString.value).toISODate(),
                version: entry.value.version as number,
                hdId: userStore.hdid,
            })
            .then(closeDialog)
            .catch((error: ResultError) => {
                if (error.statusCode === 429) {
                    errorStore.setTooManyRequestsError("noteDialog");
                } else {
                    errorMessage.value = error.resultMessage;
                }
            })
    );
}

function createNote(): void {
    const date = new DateWrapper(noteDateString.value).toISODate();
    loadingStore.applyLoader(
        Loader.NoteDialog,
        "createNote",
        noteStore
            .createNote(userStore.hdid, {
                text: noteText.value,
                title: noteTitle.value,
                journalDate: date,
                hdId: userStore.hdid,
                version: 0,
            })
            .then(() => {
                timelineStore.clearFilter();
                timelineStore.setSelectedDate(new DateWrapper(date));
            })
            .then(closeDialog)
            .catch((err: ResultError) => {
                if (err.statusCode === 429) {
                    errorStore.setTooManyRequestsError("noteDialog");
                } else {
                    errorMessage.value = err.resultMessage;
                }
            })
    );
}

function onBrowserClose(event: BeforeUnloadEvent): void {
    if (isVisible.value && !appStore.isIdle && !isBlankNote.value) {
        event.returnValue =
            "You have unsaved changes. Are you sure you want to leave?";
    }
}

window.addEventListener("beforeunload", onBrowserClose);

eventStore.subscribe(EventName.OpenNoteDialog, openDialog);
</script>

<template>
    <div class="d-flex justify-center">
        <v-dialog
            v-model="isVisible"
            data-testid="noteDialog"
            persistent
            no-click-animation
            scrollable
            :fullscreen="appStore.isMobile"
            :width="appStore.isMobile ? 960 : 700"
        >
            <v-card :loading="isLoading">
                <template #loader="{ isActive }">
                    <v-progress-linear
                        :active="isActive"
                        color="primary"
                        indeterminate
                    />
                </template>
                <v-card-title class="bg-accent px-0">
                    <v-toolbar density="compact" color="accent">
                        <template #title>
                            <v-icon class="mr-2" icon="edit" />
                            {{
                                updatingExistingEntry
                                    ? "Update Note"
                                    : "Add Note"
                            }}
                        </template>
                        <HgIconButtonComponent
                            icon="close"
                            @click="closeDialog"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-4">
                    <TooManyRequestsComponent location="noteDialog" />
                    <v-alert
                        :model-value="Boolean(errorMessage)"
                        data-testid="noteEditErrorBanner"
                        class="d-print-none mb-4"
                        type="error"
                        closable
                        variant="outlined"
                        border
                    >
                        <p data-testid="noteEditErrorText">
                            {{ errorMessage }}
                        </p>
                        <p>
                            If you continue to have issues, please contact
                            <a href="mailto:HealthGateway@gov.bc.ca"
                                >HealthGateway@gov.bc.ca</a
                            >.
                        </p>
                    </v-alert>
                    <v-row class="mb-2">
                        <v-col cols="12" sm="6">
                            <v-text-field
                                v-model.trim="noteTitle"
                                data-testid="noteTitleInput"
                                clearable
                                label="Title"
                                maxlength="100"
                                counter
                                :state="ValidationUtil.isValid(v$.noteTitle)"
                                :error-messages="
                                    ValidationUtil.getErrorMessages(
                                        v$.noteTitle
                                    )
                                "
                                @blur="v$.noteTitle.$touch()"
                            />
                        </v-col>
                        <v-col cols="12" sm="6">
                            <HgDatePickerComponent
                                v-model="noteDateString"
                                data-testid="noteDateInput"
                                :state="
                                    ValidationUtil.isValid(v$.noteDateString)
                                "
                                :error-messages="
                                    ValidationUtil.getErrorMessages(
                                        v$.noteDateString
                                    )
                                "
                                @blur="v$.noteDateString.$touch()"
                                @validity-updated="
                                    isDateStringValidDate = $event
                                "
                            />
                        </v-col>
                    </v-row>
                    <v-textarea
                        v-model="noteText"
                        data-testid="noteTextInput"
                        placeholder="Enter your note here. Your notes are only available for your own viewing."
                        :rows="3"
                        auto-grow
                        maxlength="1000"
                        counter
                    />
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        data-testid="cancelRegistrationBtn"
                        variant="secondary"
                        text="Cancel"
                        :disabled="isLoading"
                        @click="closeDialog"
                    />
                    <HgButtonComponent
                        data-testid="saveNoteBtn"
                        class="ml-2"
                        variant="primary"
                        text="Save"
                        :loading="isLoading"
                        @click="save"
                    />
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>
