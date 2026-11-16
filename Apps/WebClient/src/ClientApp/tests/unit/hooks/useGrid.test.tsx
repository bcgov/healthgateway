import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Dimensions, Platform } from "react-native";

import { useCardGrid } from "@/hooks/useGrid";
import { renderHook } from "@/test/ui";

const gridAt = async (width: number, fontScale = 1) => {
    jest.spyOn(Dimensions, "get").mockReturnValue({
        width,
        height: Math.max(width, 900),
        scale: 3,
        fontScale,
    });
    const { result } = await renderHook(() => useCardGrid());
    return result.current;
};

describe("useCardGrid (Home's quick-access grid)", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("is two columns of compact tiles on a phone", async () => {
        expect(await gridAt(390)).toEqual({ columns: 2, compact: true });
        expect(await gridAt(599)).toEqual({ columns: 2, compact: true });
    });

    it("keeps compact tiles up to 1.3x text, then falls back to one column of full cards", async () => {
        expect(await gridAt(390, 1.3)).toEqual({ columns: 2, compact: true });
        expect(await gridAt(390, 1.5)).toEqual({ columns: 1, compact: false });
    });

    const columnsAt600 = Platform.OS === "web" ? 1 : 2;

    it(`is ${columnsAt600} column(s) of full cards from 600 wide (useColumns)`, async () => {
        expect(await gridAt(600)).toEqual({
            columns: columnsAt600,
            compact: false,
        });
    });

    it("follows useColumns on wide screens", async () => {
        expect(await gridAt(960)).toEqual({ columns: 2, compact: false });
        expect(await gridAt(1366)).toEqual({ columns: 3, compact: false });
        expect(await gridAt(1920)).toEqual({ columns: 4, compact: false });
    });
});
