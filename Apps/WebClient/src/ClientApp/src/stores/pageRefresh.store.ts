import { create } from "zustand";

interface PageRefreshState {
    /** The page on screen's pull to refresh, run as if pulled; undefined when that page has none. */
    refresh?: () => void;
    /** A refresh is running (pulled, or from the navigation bar's button). */
    refreshing: boolean;
}

/**
 * Native: the page on screen offers its pull to refresh (`PageScroll`) to the navigation bar's Refresh button, which
 * sits outside the page.
 */
export const usePageRefreshStore = create<PageRefreshState>(() => ({
    refresh: undefined,
    refreshing: false,
}));
