import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import {
    Loader,
    selectLoaderActive,
    useLoadingStore,
} from "@/stores/loading.store";
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { idOf, press, render, screen, waitFor } from "@/test/ui";
import { ManageAccountSection } from "@/views/profile/ManageAccountSection";

describe("ManageAccountSection (UserProfileManageAccountComponent.vue port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("asks for confirmation, then closes the account under the profile loader", async () => {
        let finish: () => void = () => undefined;
        const close = jest.fn(
            () => new Promise<void>((resolve) => (finish = resolve))
        );
        useUserStore.setState({ closeUserAccount: close });
        await render(<ManageAccountSection />);
        expect(
            idOf(screen.getByTestId("recoverAccountShowCloseWarningBtn"))
        ).toBe("recoverAccountShowCloseWarningBtn");
        await press(screen.getByTestId("recoverAccountShowCloseWarningBtn"));
        expect(
            screen.getByText(/Your account will be marked for removal/)
        ).toBeTruthy();
        await press(screen.getByTestId("closeAccountBtn"));
        expect(close).toHaveBeenCalled();
        expect(
            selectLoaderActive(Loader.UserProfile)(useLoadingStore.getState())
        ).toBe(true);
        finish();
        await waitFor(() =>
            expect(
                screen.getByTestId("recoverAccountShowCloseWarningBtn")
            ).toBeTruthy()
        );
        expect(
            selectLoaderActive(Loader.UserProfile)(useLoadingStore.getState())
        ).toBe(false);
    });

    it("a 429 sets the page too-many error; other failures add the close banner", async () => {
        const close = jest
            .fn<() => Promise<void>>()
            .mockRejectedValueOnce(new ResultError("CE-HG", "busy", 429))
            .mockRejectedValueOnce(new ResultError("CE-HG", "boom", 500));
        useUserStore.setState({ closeUserAccount: close });
        await render(<ManageAccountSection />);
        await press(screen.getByTestId("recoverAccountShowCloseWarningBtn"));
        await press(screen.getByTestId("closeAccountBtn"));
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");
        await press(screen.getByTestId("closeAccountBtn"));
        expect(useErrorStore.getState().errors[0]).toMatchObject({
            title: "Unable to close profile",
            source: "profile",
        });
    });
});
