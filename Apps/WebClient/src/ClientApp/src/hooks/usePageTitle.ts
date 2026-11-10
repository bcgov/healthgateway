import { useEffect } from "react";
import { Platform } from "react-native";

import { announce } from "@/components/common/announce";

const SITE = "Health Gateway";

// the first title is the page load, which the browser and the screen reader read themselves
let loaded = false;

/**
 * Web: the document title for the page (WCAG 2.4.2), "<title> | Health Gateway", and after a client-side route change
 * an announcement of the new page, since nothing else tells a screen reader the page changed (WCAG 4.1.3). Native: the
 * screens have their own headers, so nothing. Stated deviation: the web client keeps one fixed title.
 */
export function usePageTitle(title: string | undefined): void {
    useEffect(() => {
        if (Platform.OS !== "web" || !title) return;
        document.title = title === SITE ? SITE : `${title} | ${SITE}`;
        if (loaded) announce(title);
        loaded = true;
    }, [title]);
}

/** Test support: forget that a page has loaded. */
export function resetPageTitles(): void {
    loaded = false;
}
