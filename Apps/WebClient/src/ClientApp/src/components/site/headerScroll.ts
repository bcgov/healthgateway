/**
 * Port of the app bar hide-on-scroll rule: Vuetify's `scroll-behavior="hide"` with the web client's
 * `headerScrollThreshold` (HeaderComponent.vue). Pure so it can be unit tested.
 */

/** HeaderComponent.vue `headerScrollThreshold` */
export const HEADER_SCROLL_THRESHOLD = 100;

export interface WebScrollState {
    /** whether the bar is visible */
    shown: boolean;
    /** offset at the previous scroll event */
    y: number;
    /** total scrollable content height at the previous event */
    contentHeight: number;
    /** visible height of the scroll view at the previous event */
    viewportHeight: number;
}

export const initialWebScrollState: WebScrollState = {
    shown: true,
    y: 0,
    contentHeight: 0,
    viewportHeight: 0,
};

export interface WebScrollSample {
    y: number;
    contentHeight: number;
    viewportHeight: number;
    /** the bar's full height */
    headerHeight: number;
    /** how much of the bar shows now (0 hidden, headerHeight shown; between while it slides) */
    barHeight: number;
    /**
     * HeaderComponent.vue's `layoutStore.isHeaderShown`: true at phone width (`isMobile`), where the web client
     * never sets the hide behaviour, so the bar never hides.
     */
    alwaysShown: boolean;
}

/**
 * Web: Vuetify 4.3.1's `scroll-behavior="hide"` (`composables/scroll.js`, `VAppBar.js`) as HeaderComponent.vue
 * sets it up, with `scroll-threshold` 100:
 * - the bar shows on any scroll up and within the threshold of the top, and hides scrolling down past it;
 * - it never hides on a page that cannot scroll more than the threshold plus the bar's height
 *   (`hasEnoughScrollableSpace`, measured with the bar shown);
 * - within the threshold of the bottom HeaderComponent turns the behaviour off (`isScrollNearBottom`), which
 *   leaves the bar as it is rather than showing it: Vuetify stops its watcher without resetting the model;
 * - at phone width the behaviour is never on.
 * The port's bar sits above the scroll view, so hiding it makes the viewport taller and the browser may pull the
 * offset back to the new end of the page: such an event only re-anchors, so the bar cannot bring itself back. Any
 * other event after a resize counts, so the first scroll up after the bar hides is not lost.
 * An unchanged offset (the trailing scroll-end event) leaves it as it is.
 */
export function nextWebHeaderState(
    state: WebScrollState,
    {
        y,
        contentHeight,
        viewportHeight,
        headerHeight,
        barHeight,
        alwaysShown,
    }: WebScrollSample
): WebScrollState {
    const next = { ...state, y, contentHeight, viewportHeight };
    if (alwaysShown) return { ...next, shown: true };
    const viewportWithBar = viewportHeight - (headerHeight - barHeight);
    const enoughSpace =
        contentHeight - viewportWithBar >
        HEADER_SCROLL_THRESHOLD + headerHeight;
    if (!enoughSpace) return { ...next, shown: true };
    const resized =
        contentHeight !== state.contentHeight ||
        viewportHeight !== state.viewportHeight;
    // the browser pulled the offset back to the new end of the page: not the user scrolling up
    if (resized && y >= contentHeight - viewportHeight - 1) return next;
    if (y === state.y) return state;
    const nearBottom =
        y + viewportHeight >= contentHeight - HEADER_SCROLL_THRESHOLD;
    if (nearBottom) return next;
    return { ...next, shown: y < state.y || y < HEADER_SCROLL_THRESHOLD };
}

/** Native: how far back up a finger has to travel to bring the bar back; less is jitter between drags. */
export const HEADER_REVEAL_DISTANCE = 32;

export interface NativeScrollState {
    shown: boolean;
    /** Where upward travel is measured from: the furthest point down while the bar is hidden. */
    anchorY: number;
    contentHeight: number;
    viewportHeight: number;
}

/**
 * Native (stated deviation): a finger is not a mouse wheel. A small move back up between drags leaves the bar alone, and
 * so does any offset change that comes from the page rather than the user: records arriving, or the bar itself resizing
 * the viewport, can each push the offset up. No reveal near the bottom either: it resizes the viewport as well, so the
 * bar can flicker there, and the list has to end somewhere without the header returning.
 */
export function nextNativeHeaderState(
    state: NativeScrollState,
    y: number,
    contentHeight: number,
    viewportHeight: number
): NativeScrollState {
    "worklet";
    const sizes = { contentHeight, viewportHeight };
    if (
        contentHeight !== state.contentHeight ||
        viewportHeight !== state.viewportHeight
    )
        return { ...state, ...sizes, anchorY: y };
    if (y < HEADER_SCROLL_THRESHOLD)
        return { ...sizes, shown: true, anchorY: y };
    if (y > state.anchorY) return { ...sizes, shown: false, anchorY: y };
    if (state.anchorY - y >= HEADER_REVEAL_DISTANCE)
        return { ...sizes, shown: true, anchorY: y };
    return { ...state, ...sizes };
}
