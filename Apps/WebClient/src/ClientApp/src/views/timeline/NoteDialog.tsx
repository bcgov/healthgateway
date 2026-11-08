import { useEffect, useState } from "react";
import { Platform, View } from "react-native";

import { TooManyRequests } from "@/components/site/TooManyRequests";
import {
    AppText,
    Dialog,
    ExternalLink,
    HgAlert,
    HgButton,
    HgDatePicker,
    HgTextField,
} from "@/components/ui";
import { TooManyRequestsLocation } from "@/constants/errorType";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { isSessionEnded, ResultError } from "@/models/errors";
import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { useLayoutStore } from "@/stores/layout.store";
import {
    Loader,
    selectLoaderActive,
    useLoadingStore,
} from "@/stores/loading.store";
import { useNoteDialogStore, useNoteStore } from "@/stores/note.store";
import { useTimelineStore } from "@/stores/timeline.store";
import { useUserStore } from "@/stores/user.store";
import { formatDate, fromIsoDate, today } from "@/utils/dateUtil";

const todayIso = () => formatDate(today(), "yyyy-MM-dd");

/** Port of NoteDialogComponent.vue: add or update a note (title, date, text). */
export function NoteDialog() {
    const { isMobile, xs } = useBreakpoint();
    const hdid = useUserStore((s) => s.user.hdid);
    const { open, entryId, closeDialog } = useNoteDialogStore();
    const notes = useNoteStore((s) => s.notes);
    const createNote = useNoteStore((s) => s.createNote);
    const updateNote = useNoteStore((s) => s.updateNote);
    const applyLoader = useLoadingStore((s) => s.applyLoader);
    const isLoading = useLoadingStore(selectLoaderActive(Loader.NoteDialog));
    const isIdle = useLayoutStore((s) => s.isIdle);

    const [entry, setEntry] = useState<NoteTimelineEntry>();
    const [noteTitle, setNoteTitle] = useState("");
    const [noteDateString, setNoteDateString] = useState(todayIso());
    const [noteText, setNoteText] = useState("");
    const [isDateStringValidDate, setIsDateStringValidDate] = useState(true);
    const [errorMessage, setErrorMessage] = useState("");
    const [touched, setTouched] = useState({ title: false, date: false });

    // openDialog(existingEntry?)
    const [prevOpen, setPrevOpen] = useState(open);
    if (prevOpen !== open) {
        setPrevOpen(open);
        if (open) {
            const note = entryId
                ? notes.find((n) => new NoteTimelineEntry(n).id === entryId)
                : undefined;
            const existing = note ? new NoteTimelineEntry(note) : undefined;
            setEntry(existing);
            setNoteText(existing?.text ?? "");
            setNoteTitle(existing?.title ?? "");
            setNoteDateString(
                existing ? formatDate(existing.date, "yyyy-MM-dd") : todayIso()
            );
            setTouched({ title: false, date: false });
            setErrorMessage("");
            logger.debug(
                existing
                    ? "Opening dialog to edit note"
                    : "Opening dialog to create note"
            );
        }
    }
    const updatingExistingEntry = entry !== undefined;
    const isBlankNote = !noteText && !noteTitle;
    const titleErrors =
        touched.title && !noteTitle ? ["Title is required."] : [];
    const dateErrors =
        touched.date && !noteDateString ? ["Date is required."] : [];

    // web: warn before leaving the page with an unsaved note
    useEffect(() => {
        if (Platform.OS !== "web") return;
        const onBrowserClose = (event: BeforeUnloadEvent) => {
            if (open && !isIdle && !isBlankNote)
                event.returnValue =
                    "You have unsaved changes. Are you sure you want to leave?";
        };
        globalThis.addEventListener("beforeunload", onBrowserClose);
        return () =>
            globalThis.removeEventListener("beforeunload", onBrowserClose);
    }, [open, isIdle, isBlankNote]);

    function handleSaveError(error: unknown) {
        if (isSessionEnded(error)) return;
        if (error instanceof ResultError && error.statusCode === 429)
            useErrorStore
                .getState()
                .setTooManyRequestsError(TooManyRequestsLocation.NoteDialog);
        else
            setErrorMessage(
                error instanceof Error ? error.message : String(error)
            );
    }

    function save() {
        setTouched({ title: true, date: true });
        if (!noteTitle || !noteDateString || !isDateStringValidDate) return;
        const date = fromIsoDate(noteDateString);
        const journalDate = formatDate(date, "yyyy-MM-dd");
        if (entry)
            void applyLoader(
                Loader.NoteDialog,
                "updateNote",
                updateNote(hdid, {
                    id: entry.id,
                    text: noteText,
                    title: noteTitle,
                    journalDate,
                    version: entry.version as number,
                    hdId: hdid,
                })
                    .then(closeDialog)
                    .catch(handleSaveError)
            );
        else
            void applyLoader(
                Loader.NoteDialog,
                "createNote",
                createNote(hdid, {
                    text: noteText,
                    title: noteTitle,
                    journalDate,
                    hdId: hdid,
                    version: 0,
                })
                    .then(() => {
                        const timeline = useTimelineStore.getState();
                        timeline.clearFilter();
                        timeline.setSelectedDate(date);
                    })
                    .then(closeDialog)
                    .catch(handleSaveError)
            );
    }

    return (
        <Dialog
            visible={open}
            title={updatingExistingEntry ? "Update Note" : "Add Note"}
            titleIcon="pen-to-square"
            tone="accent"
            persistent
            maxWidth={isMobile ? 960 : 700}
            fullscreen={isMobile}
            testID="noteDialog"
            onRequestClose={closeDialog}
            actions={
                <>
                    <HgButton
                        testID="cancelRegistrationBtn"
                        variant="secondary"
                        text="Cancel"
                        disabled={isLoading}
                        onPress={closeDialog}
                    />
                    <HgButton
                        testID="saveNoteBtn"
                        className="ml-2"
                        text="Save"
                        loading={isLoading}
                        onPress={save}
                    />
                </>
            }
        >
            <TooManyRequests location={TooManyRequestsLocation.NoteDialog} />
            {errorMessage ? (
                <HgAlert
                    testID="noteEditErrorBanner"
                    type="error"
                    live="assertive"
                    closable
                    onClose={() => setErrorMessage("")}
                    className="mb-4"
                >
                    <AppText testID="noteEditErrorText" className="mb-4">
                        {errorMessage}
                    </AppText>
                    <AppText>
                        If you continue to have issues, please contact{" "}
                        <ExternalLink href="mailto:HealthGateway@gov.bc.ca">
                            HealthGateway@gov.bc.ca
                        </ExternalLink>
                        .
                    </AppText>
                </HgAlert>
            ) : null}
            {/* v-row: title and date side by side from sm up */}
            <View className={xs ? "mb-2" : "flex-row mb-2 -mx-3"}>
                <View className={xs ? "" : "flex-1 px-3"}>
                    <HgTextField
                        testID="noteTitleInput"
                        label="Title"
                        value={noteTitle}
                        onChangeText={(v) =>
                            setNoteTitle(v.trim() === v ? v : v.trimStart())
                        }
                        onBlur={() => {
                            setNoteTitle((v) => v.trim());
                            setTouched((t) => ({ ...t, title: true }));
                        }}
                        clearable
                        maxLength={100}
                        counter
                        errorMessages={titleErrors}
                    />
                </View>
                <View className={xs ? "" : "flex-1 px-3"}>
                    <HgDatePicker
                        testID="noteDateInput"
                        value={noteDateString}
                        onChange={setNoteDateString}
                        errorMessages={dateErrors}
                        onBlur={() => setTouched((t) => ({ ...t, date: true }))}
                        onValidityChange={setIsDateStringValidDate}
                    />
                </View>
            </View>
            <HgTextField
                testID="noteTextInput"
                accessibilityLabel="Note"
                value={noteText}
                onChangeText={setNoteText}
                placeholder="Enter your note here. Your notes are only available for your own viewing."
                multiline
                numberOfLines={3}
                maxLength={1000}
                counter
            />
        </Dialog>
    );
}
