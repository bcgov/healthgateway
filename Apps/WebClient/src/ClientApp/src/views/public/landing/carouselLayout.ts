/**
 * Layout maths for the landing tiles: the xs carousel (v-carousel in LandingView.vue) and the grid's columns.
 * Kept free of React Native imports so it can be unit tested.
 */

import { swipeStep } from "@/utils/swipe";

/**
 * Width of one grid column: LandingView.vue's `v-col cols="12" md="6" lg="3"` (lg="4" for the three-column
 * health services). Native keeps two columns from sm (600pt), the stated deviation for tablets and foldables.
 */
export function gridColumnClass(columns: 3 | 4, web: boolean): string {
    const lg = columns === 4 ? "lg:w-1/4" : "lg:w-1/3";
    return `${web ? "w-full md:w-1/2" : "w-1/2"} ${lg}`;
}

/** Height of the Vuetify delimiter strip overlaid at the bottom of a v-carousel. */
export const CAROUSEL_CONTROLS_HEIGHT = 50;

/**
 * LandingView.vue carousel heights. They depend on the tile copy as much as on the width and were tuned
 * there to avoid clipping on narrow phones (Galaxy Z Fold at 360, iPhone SE/mini at 375).
 */
export function carouselHeight(columns: 3 | 4, width: number): number {
    if (columns === 4) {
        return width <= 360 ? 360 : 340;
    }
    return width <= 375 ? 400 : 370;
}

/** Index of the page under the current horizontal scroll offset, clamped to the available pages. */
export function pageIndex(
    offsetX: number,
    pageWidth: number,
    pageCount: number
): number {
    if (pageWidth <= 0 || pageCount <= 0) return 0;
    const index = Math.round(offsetX / pageWidth);
    return Math.min(Math.max(index, 0), pageCount - 1);
}

/**
 * v-carousel is `continuous`: a swipe past the last slide goes to the first, and back from the first to the last.
 * The paging scroll view does the rest of the swiping; this is the page to jump to when a swipe that started on an
 * end page tries to go past it, otherwise undefined.
 */
export function wrapPage(
    startPage: number,
    pageCount: number,
    dx: number,
    dy: number
): number | undefined {
    if (pageCount < 2) return undefined;
    const step = swipeStep(dx, dy);
    if (step === 1 && startPage === pageCount - 1) return 0;
    if (step === -1 && startPage === 0) return pageCount - 1;
    return undefined;
}
