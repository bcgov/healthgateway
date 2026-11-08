import { describe, expect, it } from "@jest/globals";

import { completesSwipeBack } from "@/views/timeline/FullscreenTimelineEntry";

describe("FullscreenTimelineEntry swipe back (iOS)", () => {
    it("closes past a third of the width or on a flick, otherwise springs back", () => {
        expect(completesSwipeBack(140, 0, 390)).toBe(true);
        expect(completesSwipeBack(60, 0.8, 390)).toBe(true);
        expect(completesSwipeBack(60, 0.1, 390)).toBe(false);
    });
});
