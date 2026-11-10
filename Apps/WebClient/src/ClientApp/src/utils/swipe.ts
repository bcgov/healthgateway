/**
 * Vuetify's v-touch (used by v-window and v-carousel): a swipe counts on release when it went more than 16px sideways
 * and moved less than half as far up or down. Returns the step: 1 (finger moved left, next), -1 (right, previous) or 0.
 */
export function swipeStep(dx: number, dy: number): -1 | 0 | 1 {
    if (Math.abs(dx) <= 16 || Math.abs(dy) >= 0.5 * Math.abs(dx)) return 0;
    return dx < 0 ? 1 : -1;
}
