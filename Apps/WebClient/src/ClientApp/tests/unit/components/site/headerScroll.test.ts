import { describe, expect, it } from "@jest/globals";

import {
    HEADER_SCROLL_THRESHOLD,
    initialWebScrollState,
    nextWebHeaderState,
    type WebScrollSample,
} from "@/components/site/headerScroll";

// a 3000-tall page in an 800-tall viewport with the 64-tall bar shown
const page = { contentHeight: 3000, viewportHeight: 800 };
const sample = (y: number, over: Partial<WebScrollSample> = {}) => ({
    ...page,
    y,
    headerHeight: 64,
    barHeight: 64,
    alwaysShown: false,
    ...over,
});
/** scrolls through each offset in turn from a settled page */
const scrollWeb = (ys: number[], over: Partial<WebScrollSample> = {}) =>
    ys.reduce(
        (s, y) => nextWebHeaderState(s, sample(y, over)),
        nextWebHeaderState(initialWebScrollState, sample(0, over))
    );

describe("nextWebHeaderState", () => {
    it("uses the web client's 100px threshold", () => {
        expect(HEADER_SCROLL_THRESHOLD).toBe(100);
    });

    it("keeps the header while within the threshold of the top", () => {
        expect(scrollWeb([50, 99]).shown).toBe(true);
        expect(scrollWeb([-40]).shown).toBe(true);
    });

    it("hides the header while scrolling down past the threshold", () => {
        expect(scrollWeb([50, 100]).shown).toBe(false);
        expect(scrollWeb([850, 900]).shown).toBe(false);
    });

    it("shows the header again as soon as the user scrolls up", () => {
        expect(scrollWeb([850, 900, 899]).shown).toBe(true);
    });

    it("leaves the header as it is when the offset has not changed", () => {
        const hidden = scrollWeb([850, 900]);
        expect(nextWebHeaderState(hidden, sample(900))).toBe(hidden);
    });

    it("near the bottom leaves the header as it is instead of showing it (scroll-behavior off)", () => {
        // 2100 + 800 is within 100 of 3000
        expect(scrollWeb([850, 2100]).shown).toBe(false);
        expect(scrollWeb([850, 2100, 2150, 2120]).shown).toBe(false);
        expect(scrollWeb([850, 2050, 1900, 2150]).shown).toBe(true);
        // leaving the bottom zone upwards brings it back
        expect(scrollWeb([850, 2150, 2099]).shown).toBe(true);
    });

    it("shows the header when the page can no longer scroll more than the threshold plus the bar", () => {
        const hidden = scrollWeb([850, 900]);
        // 800 + 100 + 64 = 964: one pixel short of enough
        expect(
            nextWebHeaderState(hidden, sample(150, { contentHeight: 964 }))
                .shown
        ).toBe(true);
        expect(
            nextWebHeaderState(hidden, sample(150, { contentHeight: 965 }))
                .shown
        ).toBe(false);
    });

    it("measures that space with the bar shown, so its own slide cannot change the answer", () => {
        // hidden: the viewport is 64 taller; 1000 - 800 = 200 is enough with the bar shown, 1000 - 864 = 136 is not
        const hidden = scrollWeb([850, 900]);
        const next = nextWebHeaderState(
            hidden,
            sample(100, {
                contentHeight: 1000,
                viewportHeight: 864,
                barHeight: 0,
            })
        );
        expect(next.shown).toBe(false);
    });

    it("ignores an offset pulled back to the end of the page by the viewport growing as the bar hides", () => {
        const hidden = scrollWeb([850, 2140]);
        // 64 taller: the end of the page is now 3000 - 864 = 2136
        const clamped = nextWebHeaderState(
            hidden,
            sample(2136, { viewportHeight: 864, barHeight: 0 })
        );
        expect(clamped.shown).toBe(false);
    });

    it("does not lose the first scroll up after the bar hid and the viewport grew", () => {
        const hidden = scrollWeb([50, 300]);
        expect(hidden.shown).toBe(false);
        expect(
            nextWebHeaderState(
                hidden,
                sample(299, { viewportHeight: 864, barHeight: 0 })
            ).shown
        ).toBe(true);
    });

    it("never hides at phone width", () => {
        expect(scrollWeb([850, 900], { alwaysShown: true }).shown).toBe(true);
    });
});
