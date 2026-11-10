import { describe, expect, it } from "@jest/globals";

import {
    carouselHeight,
    gridColumnClass,
    pageIndex,
    wrapPage,
} from "@/views/public/landing/carouselLayout";

describe("gridColumnClass", () => {
    it("follows LandingView.vue's cols 12 / md 6 / lg 3 or 4 on web", () => {
        expect(gridColumnClass(4, true)).toBe("w-full md:w-1/2 lg:w-1/4");
        expect(gridColumnClass(3, true)).toBe("w-full md:w-1/2 lg:w-1/3");
    });

    it("keeps two columns from sm on native", () => {
        expect(gridColumnClass(4, false)).toBe("w-1/2 lg:w-1/4");
        expect(gridColumnClass(3, false)).toBe("w-1/2 lg:w-1/3");
    });
});

describe("carouselHeight", () => {
    it("matches LandingView.vue for the access links carousel", () => {
        expect(carouselHeight(4, 360)).toBe(360);
        expect(carouselHeight(4, 361)).toBe(340);
        expect(carouselHeight(4, 393)).toBe(340);
    });

    it("matches LandingView.vue for the health services carousel", () => {
        expect(carouselHeight(3, 375)).toBe(400);
        expect(carouselHeight(3, 376)).toBe(370);
    });
});

describe("pageIndex", () => {
    it("rounds the offset to the nearest page", () => {
        expect(pageIndex(0, 361, 4)).toBe(0);
        expect(pageIndex(170, 361, 4)).toBe(0);
        expect(pageIndex(190, 361, 4)).toBe(1);
        expect(pageIndex(722, 361, 4)).toBe(2);
    });

    it("clamps to the available pages and tolerates a zero width", () => {
        expect(pageIndex(-50, 361, 4)).toBe(0);
        expect(pageIndex(5000, 361, 4)).toBe(3);
        expect(pageIndex(100, 0, 4)).toBe(0);
        expect(pageIndex(100, 361, 0)).toBe(0);
    });
});

describe("wrapPage (v-carousel continuous)", () => {
    it("wraps a swipe past either end", () => {
        expect(wrapPage(3, 4, -60, 5)).toBe(0);
        expect(wrapPage(0, 4, 60, 5)).toBe(3);
    });

    it("leaves other swipes to the paging scroll view", () => {
        expect(wrapPage(1, 4, -60, 0)).toBeUndefined();
        expect(wrapPage(3, 4, 60, 0)).toBeUndefined();
        expect(wrapPage(3, 4, -10, 0)).toBeUndefined();
        expect(wrapPage(3, 4, -60, 40)).toBeUndefined();
        expect(wrapPage(0, 1, 60, 0)).toBeUndefined();
    });
});
