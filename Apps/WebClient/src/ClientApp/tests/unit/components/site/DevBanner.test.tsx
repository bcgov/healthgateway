import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { DevBanner, isProduction } from "@/components/site/DevBanner";
import { useEnvironmentStore } from "@/stores/environment.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { updateStores } from "@/test/fixtures";
import { render, screen, textOf } from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("DevBanner", () => {
    afterEach(() => {
        updateStores(() =>
            useEnvironmentStore.setState({
                selected: undefined,
                effective: "local",
            })
        );
    });

    if (Platform.OS === "web") {
        it("web keeps the web client's non-production text", async () => {
            setBreakpoint(1366);
            await render(<DevBanner />);
            expect(screen.getByText(/Non-production environment/)).toBeTruthy();
        });

        it("web names the host it was loaded from, as the web client does", async () => {
            await render(<DevBanner />);
            expect(
                screen.getByText(
                    globalThis.location.hostname.toLocaleUpperCase()
                )
            ).toBeTruthy();
        });

        describe("production hosts, as the web client decides them", () => {
            const setDev = (value: boolean) =>
                Object.defineProperty(globalThis, "__DEV__", {
                    configurable: true,
                    writable: true,
                    value,
                });

            afterEach(() => {
                setDev(true);
            });

            it.each([
                "HEALTHGATEWAY.GOV.BC.CA",
                "WWW.HEALTHGATEWAY.GOV.BC.CA",
                "CLASSIC.HEALTHGATEWAY.GOV.BC.CA",
                "SECURE.HEALTHGATEWAY.GOV.BC.CA",
                "LEGACY.HEALTHGATEWAY.GOV.BC.CA",
            ])("%s is production in a release build only", (host) => {
                setDev(false);
                expect(isProduction(host)).toBe(true);
                setDev(true);
                expect(isProduction(host)).toBe(false);
            });

            it.each([
                "DEV.HEALTHGATEWAY.GOV.BC.CA",
                "TEST.HEALTHGATEWAY.GOV.BC.CA",
                "LOCALHOST",
            ])("%s is not", (host) => {
                setDev(false);
                expect(isProduction(host)).toBe(false);
            });

            it("a release build keeps the banner on any other host", async () => {
                setDev(false);
                await render(<DevBanner />);
                expect(
                    screen.getByText(/Non-production environment/)
                ).toBeTruthy();
            });
        });
        return;
    }

    it.each([
        ["local", "Local Development"],
        ["dev", "Development"],
        ["test", "Test"],
    ] as const)("%s reads %s, with no URL", async (selected, text) => {
        useEnvironmentStore.setState({ selected, effective: selected });
        await render(<DevBanner />);
        expect(textOf(screen.getByTestId("environmentBanner"))).toBe(text);
    });

    it("has no banner on Production", async () => {
        useEnvironmentStore.setState({ selected: "prod", effective: "prod" });
        await render(<DevBanner />);
        expect(screen.queryByTestId("environmentBanner")).toBeNull();
    });
});
