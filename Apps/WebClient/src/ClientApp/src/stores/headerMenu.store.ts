import { create } from "zustand";

/** A dialog the native header menu opens: Feedback, or the rating asked for before logging out. */
export type HeaderMenuDialog = "feedback" | "rating";

interface HeaderMenuState {
    dialog?: HeaderMenuDialog;
    showDialog: (dialog: HeaderMenuDialog) => void;
    hideDialog: () => void;
    /** The bell was pressed since sign-in: its badge stays hidden (HeaderComponent.vue's notificationButtonClicked). */
    bellSeen: boolean;
    setBellSeen: (seen: boolean) => void;
}

/**
 * Native: the header menu is the platform's own menu inside each navigation bar, so the dialogs it opens live once in
 * the shell (`HeaderMenuDialogs`), shown from here.
 */
export const useHeaderMenuStore = create<HeaderMenuState>((set) => ({
    dialog: undefined,
    showDialog: (dialog) => set({ dialog }),
    hideDialog: () => set({ dialog: undefined }),
    bellSeen: false,
    setBellSeen: (bellSeen) => set({ bellSeen }),
}));
