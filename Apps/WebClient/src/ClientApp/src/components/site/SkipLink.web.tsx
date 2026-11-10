import { createElement } from "react";

/**
 * "Skip to main content" (WCAG 2.4.1): the first tab stop on every page, visible only while it has the focus (stated
 * deviation: the web client has none). It moves the focus to the `<main>` landmark, which `AppShell.web.tsx` gives
 * `id="main"` and `tabIndex={-1}`.
 */
export function SkipLink() {
    return createElement(
        "a",
        {
            href: "#main",
            className:
                "sr-only focus:not-sr-only focus:fixed focus:left-4 focus:top-4 focus:z-50 focus:rounded focus:bg-surface focus:px-4 focus:py-2 focus:text-link focus:underline focus:shadow-lg",
            onClick: (event: { preventDefault: () => void }) => {
                event.preventDefault();
                const main = document.getElementById("main");
                main?.focus();
                main?.scrollIntoView();
            },
        },
        "Skip to main content"
    );
}
