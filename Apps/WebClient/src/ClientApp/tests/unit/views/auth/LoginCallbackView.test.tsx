import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import { advanceTimers, render, unmount } from "@/test/ui";
import { LoginCallbackView } from "@/views/auth/LoginCallbackView";

describe("LoginCallbackView on native", () => {
    afterEach(async () => {
        await unmount();
        jest.useRealTimers();
        jest.restoreAllMocks();
        resetStores();
    });

    if (Platform.OS === "web") {
        it("web completes the sign-in here", async () => {
            services.initialize(configFixture, fakeAuth());
            const signIn = jest.fn(async () => undefined);
            useAuthStore.setState({ signIn });
            await render(<LoginCallbackView />);
            expect(signIn).toHaveBeenCalled();
        });
        return;
    }

    it("does not sign in again: the first sign-in is still running", async () => {
        jest.useFakeTimers();
        resetRouter(Path.LoginCallback);
        services.initialize(configFixture, fakeAuth());
        const signIn = jest.fn(async () => undefined);
        useAuthStore.setState({ signIn });
        await render(<LoginCallbackView />);
        expect(signIn).not.toHaveBeenCalled();
    });

    it("a link with no sign-in behind it gives way", async () => {
        jest.useFakeTimers();
        resetRouter(Path.LoginCallback);
        services.initialize(configFixture, fakeAuth());
        await render(<LoginCallbackView />);
        expect(router.replace).not.toHaveBeenCalled();
        await advanceTimers(30000);
        expect(router.replace).toHaveBeenCalledWith(Path.Root);
    });
});
