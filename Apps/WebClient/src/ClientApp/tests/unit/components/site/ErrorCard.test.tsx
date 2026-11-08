import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ErrorCard } from "@/components/site/ErrorCard";
import {
    ErrorSourceType,
    ErrorType,
    TooManyRequestsLocation,
} from "@/constants/errorType";
import { useErrorStore } from "@/stores/error.store";
import { resetRouter } from "@/test/expoRouterMock";
import { resetStores, signInRegistered, updateStores } from "@/test/fixtures";
import { press, render, screen, textOf, waitFor } from "@/test/ui";
import { copyToClipboard } from "@/utils/clipboard";

jest.mock("@/utils/clipboard", () => ({
    copyToClipboard: jest.fn(async () => true),
}));

describe("ErrorCard (ErrorCardComponent.vue port)", () => {
    beforeEach(() => {
        resetRouter("/home");
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.clearAllMocks();
    });

    it("renders nothing without errors", async () => {
        await render(<ErrorCard />);
        expect(screen.queryByTestId("errorBanner")).toBeNull();
    });

    it("shows one error's title, and details as source/date/hdid:traceId", async () => {
        await render(<ErrorCard />);
        updateStores(() =>
            useErrorStore
                .getState()
                .addError(ErrorType.Retrieve, ErrorSourceType.QuickLinks, "t9")
        );
        await waitFor(() =>
            expect(
                screen.getByText("Unable to retrieve quick links")
            ).toBeTruthy()
        );
        expect(screen.getByText("View Details")).toBeTruthy();
        await press(screen.getByTestId("errorDetailsBtn"));
        expect(screen.getByText("Hide Details")).toBeTruthy();
        expect(textOf(screen.getByTestId("error-details-span-1"))).toMatch(
            /^quick‑links\/\d{4}-[A-Z][a-z]{2}-\d{2}\/HDID1:t9$/
        );
    });

    it("summarises several errors and copies every detail line", async () => {
        await render(<ErrorCard />);
        updateStores(() => {
            const s = useErrorStore.getState();
            s.addError(ErrorType.Retrieve, ErrorSourceType.User);
            s.addError(ErrorType.Update, ErrorSourceType.Profile);
        });
        await waitFor(() =>
            expect(
                screen.getByText("Multiple errors have occurred")
            ).toBeTruthy()
        );
        await press(screen.getByTestId("errorDetailsBtn"));
        expect(screen.getByTestId("error-details-span-2")).toBeTruthy();
        await press(screen.getByTestId("copyToClipBoardBtn"));
        expect(copyToClipboard).toHaveBeenCalledWith(
            expect.stringMatching(/^user\/.*\/HDID1\nprofile\/.*\/HDID1$/)
        );
        await waitFor(() =>
            expect(screen.getByText("Copied Successfully")).toBeTruthy()
        );
    });

    it("shows the page too-many-requests error but not one for a dialog", async () => {
        await render(<ErrorCard />);
        updateStores(() =>
            useErrorStore
                .getState()
                .setTooManyRequestsError(TooManyRequestsLocation.VerifySmsModal)
        );
        expect(screen.queryByTestId("too-many-requests-error")).toBeNull();
        updateStores(() => useErrorStore.getState().setTooManyRequestsError());
        await waitFor(() =>
            expect(screen.getByTestId("too-many-requests-error")).toBeTruthy()
        );
        expect(
            screen.getByText(
                "Unable to complete action as the site is too busy. Please try again later."
            )
        ).toBeTruthy();
        updateStores(() =>
            useErrorStore.getState().setTooManyRequestsWarning()
        );
        expect(
            screen.getByText(
                "Unable to complete action as the site is too busy. Please try again later."
            )
        ).toBeTruthy();
    });

    it("shows the warning when there is no error for the location", async () => {
        await render(<ErrorCard />);
        updateStores(() =>
            useErrorStore.getState().setTooManyRequestsWarning()
        );
        await waitFor(() =>
            expect(
                screen.getByText(
                    "We are unable to complete all actions because the site is too busy. Please try again later."
                )
            ).toBeTruthy()
        );
    });
});
