import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Linking } from "react-native";

import { openExternal } from "@/utils/openExternal";

describe("openExternal", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("opens the URL through Linking", async () => {
        const open = jest
            .spyOn(Linking, "openURL")
            .mockResolvedValue(undefined);
        await openExternal("https://example.test/");
        expect(open).toHaveBeenCalledWith("https://example.test/");
    });

    it("never throws when no app handles the scheme (mailto on a simulator)", async () => {
        jest.spyOn(Linking, "openURL").mockRejectedValue(
            new Error("No handler")
        );
        await expect(
            openExternal("mailto:x@example.test")
        ).resolves.toBeUndefined();
    });
});
