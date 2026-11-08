import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { readFileSync } from "fs";
import { join } from "path";

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

    describe("after switching environments", () => {
        afterEach(() => {
            updateStores(() =>
                useEnvironmentStore.setState({ selected: undefined })
            );
        });

        it("no way to change it for someone who never switched", async () => {
            await render(
                <AppErrorView type={AppErrorType.General} onRetry={jest.fn()} />
            );
            expect(
                screen.queryByTestId("errorChangeEnvironmentBtn")
            ).toBeNull();
        });

        it("offers to change it, so a chosen environment that is down cannot strand the app", async () => {
            useEnvironmentStore.setState({ selected: "test" });
            await render(
                <AppErrorView type={AppErrorType.General} onRetry={jest.fn()} />
            );
            expect(
                screen.getByTestId("errorChangeEnvironmentBtn")
            ).toBeTruthy();
        });
    });
});
