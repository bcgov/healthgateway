import { afterEach, describe, expect, it } from "@jest/globals";

import { Path } from "@/constants/path";
import {
    calculateUserState,
    getDefaultPath,
    isFullyRegisteredUser,
    UserState,
} from "@/navigation/userState";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { useUserStore } from "@/stores/user.store";
import {
    bcscUserInfo,
    configFixture,
    makeProfile,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

describe("calculateUserState (beforeEachGuard port)", () => {
    afterEach(resetStores);

    it("is offline until the configuration is loaded", () => {
        expect(calculateUserState()).toBe(UserState.offline);
    });

    it("is offline inside the configured maintenance window", () => {
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    clientIP: "1.2.3.4",
                    offlineMode: {
                        startDateTime: "2000-01-01T00:00:00Z",
                        message: "Down",
                        whitelist: [],
                    },
                },
            },
        });
        expect(calculateUserState()).toBe(UserState.offline);
    });

    it("is unauthenticated when configured but signed out", () => {
        useConfigStore.setState({ config: configFixture });
        expect(calculateUserState()).toBe(UserState.unauthenticated);
    });

    it("flags a non-BCSC identity provider", () => {
        useConfigStore.setState({ config: configFixture });
        useAuthStore.setState({ isAuthenticated: true });
        useUserStore
            .getState()
            .setOidcUserInfo({ ...bcscUserInfo, idp: "idir" });
        expect(calculateUserState()).toBe(UserState.invalidIdentityProvider);
    });

    it("walks noPatient → notRegistered → pendingDeletion → acceptTermsOfService → registered", () => {
        useConfigStore.setState({ config: configFixture });
        useAuthStore.setState({ isAuthenticated: true });
        const user = useUserStore.getState();
        user.setOidcUserInfo(bcscUserInfo);

        useUserStore.setState({ patientRetrievalFailed: true });
        expect(calculateUserState()).toBe(UserState.noPatient);
        useUserStore.setState({ patientRetrievalFailed: false });

        expect(calculateUserState()).toBe(UserState.notRegistered);

        user.setProfile(
            makeProfile({ closedDateTime: "2026-10-01T00:00:00Z" })
        );
        expect(calculateUserState()).toBe(UserState.pendingDeletion);

        user.setProfile(makeProfile({ hasTermsOfServiceUpdated: true }));
        expect(calculateUserState()).toBe(UserState.acceptTermsOfService);

        user.setProfile(makeProfile());
        expect(calculateUserState()).toBe(UserState.registered);
        expect(isFullyRegisteredUser()).toBe(true);
    });

    it("signInRegistered fixture yields the registered state", () => {
        signInRegistered();
        expect(calculateUserState()).toBe(UserState.registered);
    });
});

describe("getDefaultPath", () => {
    it.each([
        [UserState.offline, true, Path.Root],
        [UserState.pendingDeletion, true, Path.Profile],
        [UserState.registered, true, Path.Home],
        [UserState.registered, false, Path.Unauthorized],
        [UserState.notRegistered, true, Path.Registration],
        [UserState.invalidIdentityProvider, true, Path.IdirLoggedIn],
        [UserState.noPatient, true, Path.PatientRetrievalError],
        [UserState.unauthenticated, true, Path.Login],
        [UserState.unauthenticated, false, Path.Unauthorized],
        [UserState.acceptTermsOfService, true, Path.AcceptTermsOfService],
    ])("%s (features %s) → %s", (state, features, path) => {
        expect(getDefaultPath(state, features)).toBe(path);
    });
});
