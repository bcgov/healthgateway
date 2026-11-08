import { useState } from "react";
import { Platform, Pressable } from "react-native";

import { AppText, HgMenu, Icon } from "@/components/ui";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, ResultError } from "@/models/errors";
import type { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { useErrorStore } from "@/stores/error.store";
import { useNoteDialogStore, useNoteStore } from "@/stores/note.store";
import { confirmAction } from "@/utils/confirm";

import { TimelineEntryCard } from "../TimelineEntryCard";
import type { TimelineEntryProps } from "./entryProps";

/** Port of entry/NoteTimelineComponent.vue */
export function NoteEntry({
    entry: timelineEntry,
    hdid,
    isMobileDetails = false,
    testID,
}: TimelineEntryProps) {
    const entry = timelineEntry as NoteTimelineEntry;
    const colors = useThemeColors();
    const deleteNote = useNoteStore((s) => s.deleteNote);
    const openDialog = useNoteDialogStore((s) => s.openDialog);
    const [isSaving, setIsSaving] = useState(false);
    const canShowDetails =
        entry.text.length > 0 && entry.text !== entry.textSummary;

    async function handleDelete() {
        if (
            !(await confirmAction("Are you sure you want to delete this note?"))
        )
            return;
        setIsSaving(true);
        deleteNote(hdid, entry.toModel())
            .catch((error: unknown) => {
                if (isSessionEnded(error)) return;
                if (error instanceof ResultError && error.statusCode !== 429)
                    useErrorStore
                        .getState()
                        .addError(
                            ErrorType.Delete,
                            ErrorSourceType.Note,
                            error.traceId
                        );
            })
            .finally(() => setIsSaving(false));
    }

    return (
        <TimelineEntryCard
            testID={testID}
            entry={entry}
            entryIcon={getEntryTypeDetails(EntryType.Note)?.icon}
            iconClass="bg-accent"
            title={entry.title}
            subtitle={entry.textSummary}
            canShowDetails={canShowDetails}
            isMobileDetails={isMobileDetails}
            headerMenu={
                <HgMenu
                    items={[
                        {
                            title: "Edit",
                            testID: "editNoteMenuBtn",
                            onPress: () => openDialog(entry.id),
                        },
                        {
                            title: "Delete",
                            testID: "deleteNoteMenuBtn",
                            onPress: () => void handleDelete(),
                        },
                    ]}
                    width={140}
                    align="end"
                >
                    {(open, isOpen) => (
                        <Pressable
                            testID="noteMenuBtn"
                            accessibilityRole="button"
                            accessibilityLabel={`Options for note ${entry.title}`}
                            aria-expanded={isOpen}
                            {...(Platform.OS === "web"
                                ? ({ "aria-haspopup": "menu" } as object)
                                : {})}
                            disabled={isSaving}
                            onPress={open}
                            className="p-2 -mt-2 -mb-1"
                        >
                            <Icon
                                name="ellipsis-vertical"
                                size={16}
                                color={colors["bc-black"]}
                            />
                        </Pressable>
                    )}
                </HgMenu>
            }
        >
            <AppText>{entry.text}</AppText>
        </TimelineEntryCard>
    );
}
