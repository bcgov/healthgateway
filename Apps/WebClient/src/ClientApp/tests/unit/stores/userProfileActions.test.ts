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
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    makeProfile,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

/** Profile actions: each calls its service with the user's hdid, then reloads the profile (UserProfile store port). */
describe("user store profile actions", () => {
    let getProfile: jest.SpiedFunction<typeof services.userProfile.getProfile>;
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
        getProfile = jest
            .spyOn(services.userProfile, "getProfile")
            .mockResolvedValue(makeProfile({ email: "reloaded@example.test" }));
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it.each([
        ["updateUserEmail", "updateEmail", ["x@example.test"]],
        ["updateSmsNumber", "updateSmsNumber", ["2505550000"]],
        [
            "updateNotificationSettings",
            "updateNotificationSettings",
            [{ type: "BcCancerScreening", emailEnabled: true }],
        ],
        ["closeUserAccount", "closeAccount", []],
        ["recoverUserAccount", "recoverAccount", []],
        ["updateAcceptedTerms", "updateAcceptedTerms", ["tos-2"]],
        ["validateEmail", "validateEmail", ["invite"]],
    ] as const)(
        "%s calls %s and reloads the profile",
        async (action, method, args) => {
            const svc = services.userProfile as unknown as Record<
                string,
                (...a: unknown[]) => Promise<unknown>
            >;
            const call = jest.spyOn(svc, method).mockResolvedValue(true);
            await (
                useUserStore.getState()[action] as (
                    ...a: unknown[]
                ) => Promise<unknown>
            )(...args);
            expect(call).toHaveBeenCalledWith("HDID1", ...args);
            expect(getProfile).toHaveBeenCalledWith("HDID1");
            expect(useUserStore.getState().user.email).toBe(
                "reloaded@example.test"
            );
        }
    );

    it("validateSms reloads only when the code is accepted", async () => {
        jest.spyOn(services.userProfile, "validateSms")
            .mockResolvedValueOnce(false)
            .mockResolvedValueOnce(true);
        await expect(
            useUserStore.getState().validateSms("000000")
        ).resolves.toBe(false);
        expect(getProfile).not.toHaveBeenCalled();
        await expect(
            useUserStore.getState().validateSms("123456")
        ).resolves.toBe(true);
        expect(getProfile).toHaveBeenCalledTimes(1);
    });

    it("an email conflict (409) is rethrown without a banner; other failures add one", async () => {
        jest.spyOn(services.userProfile, "updateEmail")
            .mockRejectedValueOnce(new ResultError("CE-HG", "taken", 409))
            .mockRejectedValueOnce(new ResultError("CE-HG", "boom", 500));
        await expect(
            useUserStore.getState().updateUserEmail("a@example.test")
        ).rejects.toThrow("taken");
        expect(useErrorStore.getState().errors).toHaveLength(0);
        await expect(
            useUserStore.getState().updateUserEmail("a@example.test")
        ).rejects.toThrow("boom");
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to save changes to user"
        );
    });

    it("createProfile stores the created profile, or banners and rethrows", async () => {
        jest.spyOn(services.userProfile, "createProfile")
            .mockResolvedValueOnce(
                makeProfile({ email: "created@example.test" })
            )
            .mockRejectedValueOnce(new ResultError("CE-HG", "nope", 500));
        await useUserStore.getState().createProfile(makeProfile());
        expect(useUserStore.getState().user.email).toBe("created@example.test");
        await expect(
            useUserStore.getState().createProfile(makeProfile())
        ).rejects.toThrow("nope");
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to add profile"
        );
    });

    it("setSmsResendDateTime and clearUserData", () => {
        useUserStore.getState().setSmsResendDateTime("2026-10-29T00:00:00Z");
        expect(useUserStore.getState().smsResendDateTime).toBe(
            "2026-10-29T00:00:00Z"
        );
        useUserStore.getState().clearUserData();
        expect(useUserStore.getState().user.hdid).toBe("");
        expect(useUserStore.getState().smsResendDateTime).toBeUndefined();
    });
});
