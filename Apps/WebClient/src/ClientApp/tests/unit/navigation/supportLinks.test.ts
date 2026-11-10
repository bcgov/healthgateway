import { describe, expect, it } from "@jest/globals";

import { allSupportLinks, supportLinks } from "@/navigation/supportLinks";

describe("support links", () => {
    it("keeps FooterComponent.vue's links in order, email last", () => {
        expect(allSupportLinks.map((l) => l.text)).toEqual([
            "Support Guide",
            "Release Notes",
            "Terms of Service",
            "About Us",
            "Email: HealthGateway@gov.bc.ca",
        ]);
    });

    it("gives every link exactly one destination", () => {
        for (const l of allSupportLinks) {
            expect(Boolean(l.to) !== Boolean(l.href)).toBe(true);
        }
        expect(
            supportLinks.find((l) => l.text === "Terms of Service")?.to
        ).toBe("/termsOfService");
    });
});
