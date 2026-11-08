import { create } from "zustand";

interface LayoutState {
    /** undefined = default (open on desktop, closed on mobile) until the user toggles it. */
    sidebarOpen?: boolean;
    toggleSidebar: (isMobile: boolean) => void;
    setSidebar: (open: boolean) => void;
    /** appStore.isIdle on the web client: the idle countdown dialog is showing */
    isIdle: boolean;
    setIsIdle: (idle: boolean) => void;
}

export const useLayoutStore = create<LayoutState>((set, get) => ({
    sidebarOpen: undefined,
    toggleSidebar: (isMobile) => {
        const current = get().sidebarOpen ?? !isMobile;
        set({ sidebarOpen: !current });
    },
    setSidebar: (open) => set({ sidebarOpen: open }),
    isIdle: false,
    setIsIdle: (idle) => set({ isIdle: idle }),
}));

export const selectIsSidebarOpen = (isMobile: boolean) => (s: LayoutState) =>
    s.sidebarOpen ?? !isMobile;
