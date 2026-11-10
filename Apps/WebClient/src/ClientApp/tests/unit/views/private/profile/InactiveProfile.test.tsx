import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { DateTime } from "luxon";

import { ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import {
    describeTimeForDeletion,
    InactiveProfile,
} from "@/views/private/profile/InactiveProfile";

describe("describeTimeForDeletion (timeForDeletionString)", () => {
    it.each([
        [3 * 86_400_000, "3 days"],
        [36 * 3_600_000, "36 hours"],
        [90 * 60_000, "90 minutes"],
        [5_000, "5 seconds"],
        [1_000, "Your account will be closed imminently"],
        [-1, "Your account will be closed imminently"],
    ])("%d ms → %s", (ms, text) => {
        expect(describeTimeForDeletion(ms)).toBe(text);
    });
});

describe("InactiveProfile (InactiveUserProfileComponent.vue port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered({
            closedDateTime: DateTime.now().minus({ hours: 1 }).toISO() ?? "",
        });
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows the removal notice and the time remaining", async () => {
        await render(<InactiveProfile />);
        expect(screen.getByTestId("emailOptOutMessage")).toBeTruthy();
        expect(screen.getByText("Account marked for removal")).toBeTruthy();
        expect(
            screen.getByText(/Time remaining for deletion: \d+ (days|hours)/)
        ).toBeTruthy();
    });

    it("recovers the account, and reports a failure as a custom banner", async () => {
        const recover = jest
            .fn<() => Promise<void>>()
            .mockResolvedValueOnce(undefined)
            .mockRejectedValueOnce(new ResultError("CE-HG", "boom", 500));
        useUserStore.setState({ recoverUserAccount: recover });
        await render(<InactiveProfile />);
        await press(screen.getByTestId("recoverAccountCancelBtn"));
        expect(recover).toHaveBeenCalledTimes(1);
        expect(useErrorStore.getState().errors).toHaveLength(0);
        await press(screen.getByTestId("recoverAccountCancelBtn"));
        expect(useErrorStore.getState().errors[0].title).toBe(
            "Unable to recover profile"
        );
    });
});
