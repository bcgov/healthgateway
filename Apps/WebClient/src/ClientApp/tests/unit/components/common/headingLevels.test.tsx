import { describe, expect, it } from "@jest/globals";

import { FieldLabel } from "@/components/common/FieldLabel";
import { VisuallyHidden } from "@/components/common/LiveRegion";
import { SectionHeading } from "@/components/common/SectionHeading";
import { headingLevelOf, render, screen } from "@/test/ui";
import { PageError } from "@/views/errors/PageError";
import { LandingIntro } from "@/views/public/landing/LandingIntro";

// Heading levels match the Vue markup; react-native-web renders a header with no level as an h1.
describe("heading levels", () => {
    it("FieldLabel is an h4 (LabelComponent.vue)", async () => {
        await render(<FieldLabel title="Full Name" />);
        expect(headingLevelOf(screen.getByText("Full Name"))).toBe(4);
    });

    it("SectionHeading is an h3 (SectionHeadingComponent.vue)", async () => {
        await render(<SectionHeading title="Email Address" />);
        expect(headingLevelOf(screen.getByText("Email Address"))).toBe(3);
    });

    it("the landing intro's title is an h1 (LandingView.vue)", async () => {
        await render(<LandingIntro />);
        expect(
            headingLevelOf(
                screen.getByText("Access your health information online")
            )
        ).toBe(1);
    });

    it("PageError's title is an h1 and its subtitle an h2 (PageErrorComponent.vue)", async () => {
        await render(<PageError title="404" subtitle="Page not found" />);
        expect(headingLevelOf(screen.getByText("404"))).toBe(1);
        expect(headingLevelOf(screen.getByText("Page not found"))).toBe(2);
    });

    it("SectionHeading and FieldLabel take a level where a page has no heading between", async () => {
        await render(
            <>
                <SectionHeading title="Contact Information" level={2} />
                <FieldLabel title="Email Address" level={3} />
            </>
        );
        expect(headingLevelOf(screen.getByText("Contact Information"))).toBe(2);
        expect(headingLevelOf(screen.getByText("Email Address"))).toBe(3);
    });

    it("VisuallyHidden can be a heading the screen reader hears and the page does not show", async () => {
        await render(<VisuallyHidden headingLevel={2}>Records</VisuallyHidden>);
        expect(headingLevelOf(screen.getByText("Records"))).toBe(2);
    });
});
