import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Dimensions, Platform } from "react-native";

import { useBreakpoint } from "@/hooks/useBreakpoint";
import { renderHook } from "@/test/ui";

const inlineAt = async (width: number, height: number) => {
    jest.spyOn(Dimensions, "get").mockReturnValue({
        width,
        height,
        scale: 3,
        fontScale: 1,
    });
    const { result } = await renderHook(() => useBreakpoint());
    return result.current.inlineEntryDetails;
};

describe("useBreakpoint inlineEntryDetails", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("follows the web client's rule from 960 wide on every platform", async () => {
        expect(await inlineAt(1366, 768)).toBe(true);
        expect(await inlineAt(960, 500)).toBe(true);
    });

    it("stays full screen on a phone, in both orientations", async () => {
        expect(await inlineAt(402, 874)).toBe(false);
        expect(await inlineAt(956, 440)).toBe(false);
    });

    it("stays full screen on the folded iPhone Duo", async () => {
        expect(await inlineAt(466, 678)).toBe(false);
    });

    const expected = Platform.OS !== "web";

    it(`is ${expected} on the open iPhone Duo, in both orientations`, async () => {
        expect(await inlineAt(669, 951)).toBe(expected);
        expect(await inlineAt(951, 669)).toBe(expected);
    });

    it(`is ${expected} on an iPad mini in portrait`, async () => {
        expect(await inlineAt(744, 1133)).toBe(expected);
    });

    it("stays full screen in an iPad's narrow split view", async () => {
        expect(await inlineAt(507, 1024)).toBe(false);
    });
});
