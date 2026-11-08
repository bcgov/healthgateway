import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { services } from "@/services/container";
import { Loader, useLoadingStore } from "@/stores/loading.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { headingLevels, press, render, screen } from "@/test/ui";
import { ProfileView } from "@/views/profile/ProfileView";

describe("ProfileView (ProfileView.vue port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows the profile, and while Loader.UserProfile is active a skeleton and the overlay instead", async () => {
        await render(<ProfileView />);
        expect(screen.queryByTestId("profile-skeleton")).toBeNull();
        expect(screen.getByTestId("fullName")).toBeTruthy();
        // start an edit so we can see it survive the loader
        await press(screen.getByTestId("editEmailBtn"));

        updateStores(() =>
            useLoadingStore
                .getState()
                .setIsLoading(true, Loader.UserProfile, "updateSms")
        );
        expect(screen.getByTestId("profile-skeleton")).toBeTruthy();
        expect(Object.keys(useLoadingStore.getState().requests)).toHaveLength(
            1
        );

        updateStores(() =>
            useLoadingStore
                .getState()
                .setIsLoading(false, Loader.UserProfile, "updateSms")
        );
        expect(screen.queryByTestId("profile-skeleton")).toBeNull();
        expect(Object.keys(useLoadingStore.getState().requests)).toHaveLength(
            0
        );
        // hidden, not unmounted (v-show): the edit is still open
        expect(screen.getByTestId("editEmailSaveBtn")).toBeTruthy();
    });

    it("has a heading outline that never skips a level: h1 Profile, h2 sections, h3 labels", async () => {
        await render(<ProfileView />);
        const levels = headingLevels();
        expect(levels[0]).toBe(1);
        expect(levels).toContain(2);
        levels.forEach((level, i) => {
            if (i > 0) expect(level).toBeLessThanOrEqual(levels[i - 1] + 1);
        });
    });
});
