import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform, StyleSheet } from "react-native";

import { CommunicationBanner } from "@/components/site/CommunicationBanner";
import { Path } from "@/constants/path";
import { type Communication, CommunicationType } from "@/models/communication";
import { services } from "@/services/container";
import { useCommunicationStore } from "@/stores/communication.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import { render, screen, type UiElement, waitFor } from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const html =
    '<p>Test Banner - <a href="healthgateway@gov.bc.ca">healthgateway@gov.bc.ca</a></p>';

/** the end padding of the box around the banner's text (v-banner-text) */
function textEndPadding(el: UiElement): string {
    if (Platform.OS === "web")
        return (el as unknown as HTMLElement).parentElement!.style.paddingRight;
    let node = (el as { parent: UiElement | null }).parent;
    while (node && !(node.props as { style?: unknown }).style)
        node = (node as { parent: UiElement | null }).parent;
    const style = StyleSheet.flatten(
        (node?.props as { style?: never }).style
    ) as { paddingRight?: number };
    return `${style.paddingRight}px`;
}

describe("CommunicationBanner (CommunicationComponent.vue)", () => {
    beforeEach(() => {
        resetRouter(Path.Root);
        services.initialize(configFixture, fakeAuth());
        // what the shell's useCommunicationRetrieval fetched: the banner only shows it
        useCommunicationStore.setState({
            entries: {
                [CommunicationType.Banner]: {
                    communication: {
                        text: html,
                        communicationTypeCode: CommunicationType.Banner,
                    } as Communication,
                    fetchedAt: Date.now(),
                },
            },
        });
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("fetches nothing itself: the shell's useCommunicationRetrieval does", async () => {
        const getActive = jest.spyOn(services.communication, "getActive");
        setBreakpoint(1366);
        await render(<CommunicationBanner />);
        expect(screen.getByTestId("communicationBanner")).toBeTruthy();
        expect(getActive).not.toHaveBeenCalled();
    });

    it("keeps v-banner-text's 90px end padding on a wide window", async () => {
        setBreakpoint(1366);
        await render(<CommunicationBanner />);
        const banner = await waitFor(() =>
            screen.getByTestId("communicationBanner")
        );
        expect(textEndPadding(banner)).toBe("90px");
    });

    it("uses the stacked banner's 36px below Vuetify's lg breakpoint", async () => {
        setBreakpoint(1279);
        await render(<CommunicationBanner />);
        const banner = await waitFor(() =>
            screen.getByTestId("communicationBanner")
        );
        expect(textEndPadding(banner)).toBe("36px");
    });

    it("colours the text and its links like the web client's banner", async () => {
        setBreakpoint(390);
        await render(<CommunicationBanner />);
        const banner = await waitFor(() =>
            screen.getByTestId("communicationBanner")
        );
        if (Platform.OS === "web") {
            // the HTML goes in as given; the token classes style it
            expect(
                (banner as unknown as HTMLElement).querySelector("a")
                    ?.textContent
            ).toBe("healthgateway@gov.bc.ca");
            return;
        }
        // NativeWind classes compile away on web; the native test renderer keeps them
        const classOf = (el: UiElement) =>
            String((el.props as { className?: string }).className);
        const link = screen.getByText("healthgateway@gov.bc.ca");
        expect(classOf(link)).toContain("text-banner-link");
        expect(classOf(screen.getByText(/^Test Banner/))).toContain(
            "text-banner-text"
        );
    });
});
