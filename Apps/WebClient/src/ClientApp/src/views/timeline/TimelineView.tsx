import { View } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { HgButton, PageTitle } from "@/components/ui";
import { EntryType, entryTypes as allEntryTypes } from "@/constants/entryType";
import { Path } from "@/constants/path";
import { Action, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { services } from "@/services/container";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import {
    selectNotesAreLoading,
    useNoteDialogStore,
    useNoteStore,
} from "@/stores/note.store";
import { useUserStore } from "@/stores/user.store";
import { ConfigUtil } from "@/utils/configUtil";

import { NoteDialog } from "./NoteDialog";
import { Timeline } from "./Timeline";

/** Port of UserTimelineView.vue */
export function TimelineView() {
    const hdid = useUserStore((s) => s.user.hdid);
    const commentsAreEnabled = useConfigStore(
        (s) =>
            selectWebConfig(s)?.featureToggleConfiguration.timeline.comment ??
            false
    );
    // re-evaluated when the configuration changes
    useConfigStore((s) => s.config);
    const { xs } = useBreakpoint();
    const notesAreLoading = useNoteStore(selectNotesAreLoading);
    const openNoteDialog = useNoteDialogStore((s) => s.openDialog);
    const notesAreEnabled = ConfigUtil.isDatasetEnabled(EntryType.Note);
    const entryTypes = allEntryTypes
        .filter((d) => ConfigUtil.isDatasetEnabled(d.type))
        .map((d) => d.type);

    return (
        <View>
            <Breadcrumb
                items={[
                    {
                        text: "Health Records",
                        to: Path.Timeline,
                        active: true,
                        testId: "breadcrumb-timeline",
                    },
                ]}
            />
            <PageTitle
                title="Health Records"
                append={
                    notesAreEnabled && !notesAreLoading ? (
                        <HgButton
                            testID="addNoteBtn"
                            variant="secondary"
                            icon="pen-to-square"
                            // the label shows from sm up (d-none d-sm-inline)
                            text={xs ? "" : "Add a Note"}
                            accessibilityLabel="Add a Note"
                            onPress={() => {
                                services.tracking.trackEvent({
                                    action: Action.ButtonClick,
                                    text: Text.AddNote,
                                    type: Type.Notes,
                                });
                                openNoteDialog();
                            }}
                        />
                    ) : undefined
                }
            />
            <Timeline
                hdid={hdid}
                entryTypes={entryTypes}
                commentsAreEnabled={commentsAreEnabled}
                refreshable
            />
            <NoteDialog />
        </View>
    );
}
