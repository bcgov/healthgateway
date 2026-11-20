import { Slot } from "expo-router";

import { NoteDialog } from "@/views/private/timeline/NoteDialog";

/**
 * Web: a flat route under the shell; the folder only gives native its navigation bar. The note dialog is here, as on
 * native (`_layout.tsx`), rather than in the timeline page.
 */
export default function TimelineStackLayout() {
    return (
        <>
            <Slot />
            <NoteDialog />
        </>
    );
}
