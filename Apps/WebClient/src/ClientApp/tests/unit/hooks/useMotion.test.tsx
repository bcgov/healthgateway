import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { useReducedMotion } from "react-native-reanimated";

import { useMotion } from "@/hooks/useMotion";
import { renderHook } from "@/test/ui";

describe("useMotion", () => {
    afterEach(() => {
        jest.mocked(useReducedMotion).mockReturnValue(false);
    });

    it("keeps the durations and modal animations while motion is allowed", async () => {
        const { result } = await renderHook(() => useMotion());
        expect(result.current.reduced).toBe(false);
        expect(result.current.ms(200)).toBe(200);
        expect(result.current.modal("fade")).toBe("fade");
        expect(result.current.modal(undefined)).toBeUndefined();
    });

    it("zeroes them while the system asks for reduced motion", async () => {
        jest.mocked(useReducedMotion).mockReturnValue(true);
        const { result } = await renderHook(() => useMotion());
        expect(result.current.reduced).toBe(true);
        expect(result.current.ms(200)).toBe(0);
        expect(result.current.modal("slide")).toBe("none");
    });
});
