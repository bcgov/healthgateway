import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { resetStores, updateStores } from "@/test/fixtures";
import { render, waitFor } from "@/test/ui";
import { LogoutView } from "@/views/auth/LogoutView";

describe("LogoutView", () => {
    const signOut = jest.fn(async () => undefined);
    beforeEach(() => {
        signOut.mockClear();
        useAuthStore.setState({ signOut });
        resetRouter("/logout");
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("signs out", async () => {
        await render(<LogoutView />);
        await waitFor(() => expect(signOut).toHaveBeenCalledTimes(1));
    });

    if (Platform.OS !== "web") {
        it("under the lock nothing is signed out, until the prompt has passed", async () => {
            // a launch through healthgateway://logout with a sealed login: it must not go without the device's prompt
            useAppLockStore.setState({ locked: true });
            jest.mocked(router.replace).mockClear();
            await render(<LogoutView />);
            expect(signOut).not.toHaveBeenCalled();
            expect(router.replace).not.toHaveBeenCalled();

            updateStores(() => useAppLockStore.setState({ locked: false }));
            await waitFor(() => expect(signOut).toHaveBeenCalledTimes(1));
            await waitFor(() =>
                expect(router.replace).toHaveBeenCalledWith(Path.LogoutComplete)
            );
        });
    }
});
