import { createContext, useContext, useEffect, useRef } from "react";
import type { View } from "react-native";
import type { useAnimatedScrollHandler } from "react-native-reanimated";

export type ScrollHandler = ReturnType<typeof useAnimatedScrollHandler>;

/** Lets a page's own scroll view (native) report to the shell's collapsible header, on the UI thread. */
export const PageScrollContext = createContext<ScrollHandler | undefined>(
    undefined
);

/** Scrolls the page back to the top (window.scrollTo(0, 0) in the web client, e.g. on a timeline page change). */
export const ScrollToTopContext = createContext<() => void>(() => undefined);
export const useScrollToTop = () => useContext(ScrollToTopContext);

/** Brings a view inside the page's scroll view to the top, for a link within a document. */
export const ScrollToViewContext = createContext<
    (view: React.ComponentRef<typeof View>) => void
>(() => undefined);

export type RefreshHandler = () => Promise<void>;

/** Lets the page inside a `PageScroll` (native) offer pull to refresh. */
export const PageRefreshContext = createContext<
    (handler: RefreshHandler | undefined) => void
>(() => undefined);

/** Turns on pull to refresh for the page while `handler` is set. Web has no pull gesture: `PageScroll.web` ignores it. */
export function usePageRefresh(handler: RefreshHandler | undefined) {
    const register = useContext(PageRefreshContext);
    const latest = useRef(handler);
    useEffect(() => {
        latest.current = handler;
    });
    const enabled = handler !== undefined;
    useEffect(() => {
        if (!enabled) return;
        register(async () => latest.current?.());
        return () => register(undefined);
    }, [enabled, register]);
}

/** Lets the page inside a `PageScroll` (native) say it is still fetching. */
export const PageRetrievingContext = createContext<(active: boolean) => void>(
    () => undefined
);

/** Shows the Retrieving pill while `active`. Web has its own toast: `PageScroll.web` ignores it. */
export function usePageRetrieving(active: boolean) {
    const setRetrieving = useContext(PageRetrievingContext);
    useEffect(() => {
        if (!active) return;
        setRetrieving(true);
        return () => setRetrieving(false);
    }, [active, setRetrieving]);
}
