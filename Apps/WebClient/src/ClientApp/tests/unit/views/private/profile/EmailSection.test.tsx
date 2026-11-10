import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { services } from "@/services/container";
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import {
    changeText,
    isDisabled,
    press,
    render,
    screen,
    waitFor,
} from "@/test/ui";
import { EmailSection } from "@/views/private/profile/EmailSection";

describe("EmailSection (UserProfileEmailComponent.vue port)", () => {
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

    it("shows the email read-only with its status until Edit", async () => {
        await render(<EmailSection onEmailUpdated={() => undefined} />);
        expect(screen.getByTestId("emailStatusVerified")).toBeTruthy();
        expect(screen.queryByTestId("editEmailSaveBtn")).toBeNull();
        await press(screen.getByTestId("editEmailBtn"));
        expect(screen.getByTestId("editEmailSaveBtn")).toBeTruthy();
    });

    it("validates: same address and malformed addresses block Save", async () => {
        await render(<EmailSection onEmailUpdated={() => undefined} />);
        await press(screen.getByTestId("editEmailBtn"));
        expect(
            screen.getByText(
                "New email must be different from the previous one"
            )
        ).toBeTruthy();
        expect(isDisabled(screen.getByTestId("editEmailSaveBtn"))).toBe(true);
        await changeText(screen.getByTestId("email-input"), "not-an-email");
        expect(screen.getByText("Valid email is required")).toBeTruthy();
        expect(isDisabled(screen.getByTestId("editEmailSaveBtn"))).toBe(true);
    });

    it("warns when the address is cleared (opt out)", async () => {
        await render(<EmailSection onEmailUpdated={() => undefined} />);
        await press(screen.getByTestId("editEmailBtn"));
        await changeText(screen.getByTestId("email-input"), "");
        expect(screen.getByTestId("emailOptOutMessage")).toBeTruthy();
    });

    it("saves a new address through the store and reports the reloaded address", async () => {
        // the real action saves, then reloads the profile into the store
        const update = jest
            .spyOn(useUserStore.getState(), "updateUserEmail")
            .mockImplementation(async (email: string) => {
                useUserStore.setState((s) => ({
                    user: { ...s.user, email, verifiedEmail: false },
                }));
            });
        useUserStore.setState({ updateUserEmail: update as never });
        const onUpdated = jest.fn();
        await render(<EmailSection onEmailUpdated={onUpdated} />);
        await press(screen.getByTestId("editEmailBtn"));
        await changeText(screen.getByTestId("email-input"), "new@example.test");
        expect(isDisabled(screen.getByTestId("editEmailSaveBtn"))).toBe(false);
        await press(screen.getByTestId("editEmailSaveBtn"));
        await waitFor(() =>
            expect(onUpdated).toHaveBeenCalledWith("new@example.test")
        );
        expect(update).toHaveBeenCalledWith("new@example.test");
    });
});
