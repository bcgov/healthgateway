import { describe, expect, it } from "@jest/globals";

import { cn } from "@/utils/cn";

describe("cn", () => {
    it("lets a later colour token replace an earlier one", () => {
        expect(
            cn(
                "font-bold text-headline-small text-primary",
                "text-bc-black mb-2"
            )
        ).toBe("font-bold text-headline-small text-bc-black mb-2");
    });

    it("treats the Vuetify scale as sizes and tokens as colours", () => {
        expect(
            cn("text-body-large text-bc-black", "text-[14px] text-on-primary")
        ).toBe("text-[14px] text-on-primary");
        expect(cn("text-body-large", "text-center")).toBe(
            "text-body-large text-center"
        );
    });

    it("replaces letter spacing and alignment", () => {
        expect(
            cn("tracking-[0.5px] self-start", "tracking-[0.1px] self-center")
        ).toBe("tracking-[0.1px] self-center");
    });

    it("keeps a more specific padding beside the shorthand", () => {
        expect(cn("p-5", "pb-12")).toBe("p-5 pb-12");
        expect(cn("pb-12", "p-5")).toBe("p-5");
    });

    it("keeps variant-prefixed classes", () => {
        expect(
            cn("bg-surface", "hover:bg-surface-hover web:cursor-pointer")
        ).toBe("bg-surface hover:bg-surface-hover web:cursor-pointer");
    });

    it("lets a later font-bold replace font-sans, which would otherwise stop text being bold on iOS", () => {
        expect(cn("font-sans text-bc-black", "font-bold")).toBe(
            "text-bc-black font-bold"
        );
    });
});
