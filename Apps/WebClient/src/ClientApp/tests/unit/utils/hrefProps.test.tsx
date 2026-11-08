import { describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import { hrefProps } from "@/utils/hrefProps";

describe("hrefProps", () => {
    it("web: the anchor props, with a new-tab target on request; native: nothing", () => {
        if (Platform.OS !== "web") {
            expect(hrefProps("https://example.test")).toEqual({});
            return;
        }
        expect(hrefProps("/terms")).toEqual({
            href: "/terms",
            hrefAttrs: undefined,
        });
        expect(hrefProps("https://example.test", { newTab: true })).toEqual({
            href: "https://example.test",
            hrefAttrs: { target: "_blank", rel: "noopener noreferrer" },
        });
    });

    it("nothing without a destination", () => {
        expect(hrefProps(undefined)).toEqual({});
    });
});
