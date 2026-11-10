import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Linking, Platform } from "react-native";

import { Footer } from "@/components/site/Footer";
import { ExternalUrl } from "@/constants/externalUrls";
import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    attrOf,
    press,
    pressKeepsDefault,
    render,
    screen,
    tagOf,
} from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("Footer", () => {
    beforeEach(() => {
        resetRouter("/");
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("lists the support links and the email for a visitor", async () => {
        await render(<Footer />);
        for (const id of [
            "footer-faq-link",
            "footer-release-notes-link",
            "footer-terms-of-service-link",
            "footer-about-us-link",
            "footer-email-link",
        ]) {
            expect(screen.getByTestId(id)).toBeTruthy();
        }
        expect(screen.getByText("Email: HealthGateway@gov.bc.ca")).toBeTruthy();
    });

    it("opens internal links with the router and external ones with Linking", async () => {
        const open = jest
            .spyOn(Linking, "openURL")
            .mockResolvedValue(undefined);
        await render(<Footer />);
        await press(screen.getByTestId("footer-terms-of-service-link"));
        expect(router.push).toHaveBeenCalledWith(Path.TermsOfService);
        await press(screen.getByTestId("footer-faq-link"));
        expect(open).toHaveBeenCalledWith(ExternalUrl.SupportGuide);
        await press(screen.getByTestId("footer-email-link"));
        expect(open).toHaveBeenCalledWith(ExternalUrl.HealthGatewayEmail);
    });

    it("renders for a registered user and hides while offline", async () => {
        signInRegistered();
        await render(<Footer />);
        expect(screen.getByTestId("footer")).toBeTruthy();
        updateStores(() => useConfigStore.setState({ config: undefined }));
        await render(<Footer />);
        expect(screen.queryByTestId("footer-faq-link")).toBeNull();
    });

    it("starts its links 24px in: v-footer's 16px plus its row's pa-2", async () => {
        await render(<Footer />);
        // NativeWind classes compile away on web; the native test renderer keeps them
        if (Platform.OS === "web") return;
        const footer = screen.getByTestId("footer");
        expect(
            String((footer.props as { className?: string }).className)
        ).toMatch(/\bpx-6\b/);
    });

    it("web: the links are real anchors the keyboard can follow, and a press does not also let the browser follow them", async () => {
        if (Platform.OS !== "web") return;
        jest.spyOn(Linking, "openURL").mockResolvedValue(undefined);
        await render(<Footer />);
        const terms = screen.getByTestId("footer-terms-of-service-link");
        expect(tagOf(terms)).toBe("a");
        expect(attrOf(terms, "href")).toBe(Path.TermsOfService);
        const faq = screen.getByTestId("footer-faq-link");
        expect(tagOf(faq)).toBe("a");
        expect(attrOf(faq, "href")).toBe(ExternalUrl.SupportGuide);
        expect(attrOf(faq, "target")).toBe("_blank");
        const email = screen.getByTestId("footer-email-link");
        expect(attrOf(email, "href")).toBe(ExternalUrl.HealthGatewayEmail);
        expect(attrOf(email, "target")).toBeUndefined();
        // the press routes (or opens the tab) itself: the browser must not follow the anchor as well
        expect(await pressKeepsDefault(terms)).toBe(false);
        expect(router.push).toHaveBeenCalledWith(Path.TermsOfService);
        expect(await pressKeepsDefault(faq)).toBe(false);
        expect(Linking.openURL).toHaveBeenCalledWith(ExternalUrl.SupportGuide);
    });
});
