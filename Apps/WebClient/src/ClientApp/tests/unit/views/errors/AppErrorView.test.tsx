import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { readFileSync } from "fs";
import { join } from "path";
import { Platform } from "react-native";

import { AppErrorType } from "@/constants/errorType";
import { useEnvironmentStore } from "@/stores/environment.store";
import { updateStores } from "@/test/fixtures";
import { render, screen, textOf } from "@/test/ui";
import { AppErrorView } from "@/views/errors/AppErrorView";

describe("AppErrorView (AppErrorView.vue port)", () => {
    it("uses the app's light bar, as the web client does: the logo is drawn for it", () => {
        const source = readFileSync(
            join(process.cwd(), "src/views/errors/AppErrorView.tsx"),
            "utf8"
        );
        expect(source).toContain("bg-app-bar");
        expect(source).not.toContain("bg-primary");
    });

    it("shows the too-busy warning for a 429 at start-up", async () => {
        await render(
            <AppErrorView
                type={AppErrorType.TooManyRequests}
                onRetry={jest.fn()}
            />
        );
        expect(screen.getByText("Application Error!")).toBeTruthy();
        expect(textOf(screen.getByTestId("app-warning"))).toContain(
            "We are unable to complete all actions because the site is too busy. Please try again later."
        );
    });

    it("shows the load failure otherwise", async () => {
        await render(
            <AppErrorView type={AppErrorType.General} onRetry={jest.fn()} />
        );
        expect(textOf(screen.getByTestId("app-error"))).toContain(
            "Unable to load application. Please try refreshing the page or come back later."
        );
    });

    describe("environment banner", () => {
        afterEach(() => {
            updateStores(() =>
                useEnvironmentStore.setState({
                    selected: undefined,
                    effective: "local",
                })
            );
        });

        it("native: names the environment that failed to start", async () => {
            useEnvironmentStore.setState({
                selected: "test",
                effective: "test",
            });
            await render(
                <AppErrorView type={AppErrorType.General} onRetry={jest.fn()} />
            );
            const banner = screen.queryByTestId("environmentBanner");
            // web keeps the web client's page, which has no banner here
            if (Platform.OS === "web") expect(banner).toBeNull();
            else expect(textOf(banner!)).toBe("Test");
        });

        it("native: none on Production", async () => {
            useEnvironmentStore.setState({
                selected: "prod",
                effective: "prod",
            });
            await render(
                <AppErrorView type={AppErrorType.General} onRetry={jest.fn()} />
            );
            expect(screen.queryByTestId("environmentBanner")).toBeNull();
        });
    });

    describe("Change Environment: only when this build offers several (environmentSelector.md)", () => {
        afterEach(() => {
            updateStores(() =>
                useEnvironmentStore.setState({
                    selected: undefined,
                    effective: "local",
                    offered: ["local", "dev", "test", "prod"],
                })
            );
        });

        const renderError = () =>
            render(
                <AppErrorView type={AppErrorType.General} onRetry={jest.fn()} />
            );

        it("a released build (Production only): none", async () => {
            useEnvironmentStore.setState({
                effective: "prod",
                offered: ["prod"],
            });
            await renderError();
            expect(
                screen.queryByTestId("errorChangeEnvironmentBtn")
            ).toBeNull();
        });

        it("native: an unreleased build that couldn't reach the released build keeps its environment and Production, so a Test that is down can't strand it", async () => {
            useEnvironmentStore.setState({
                effective: "test",
                offered: ["test", "prod"],
            });
            await renderError();
            const button = screen.queryByTestId("errorChangeEnvironmentBtn");
            if (Platform.OS === "web") expect(button).toBeNull();
            else expect(button).toBeTruthy();
        });

        it("native: a development build offers every environment", async () => {
            await renderError();
            const button = screen.queryByTestId("errorChangeEnvironmentBtn");
            // web has no environments to choose
            if (Platform.OS === "web") expect(button).toBeNull();
            else expect(button).toBeTruthy();
        });
    });
});
