import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { services } from "@/services/container";
import {
    Loader,
    selectLoaderActive,
    useLoadingStore,
} from "@/stores/loading.store";
import {
    configFixture,
    fakeAuth,
    makeProfile,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import {
    advanceTimers,
    changeText,
    isDisabled,
    press,
    render,
    screen,
} from "@/test/ui";
import { SmsSection } from "@/views/private/profile/SmsSection";

describe("SmsSection (UserProfileSmsComponent.vue port)", () => {
    beforeEach(() => {
        // the server check is debounced 500 ms; fake timers keep its state updates inside act()
        jest.useFakeTimers();
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
    });
    afterEach(() => {
        jest.useRealTimers();
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows the masked number, Not Verified, and a Verify button", async () => {
        jest.spyOn(services.userProfile, "updateSmsNumber").mockResolvedValue(
            undefined
        );
        await render(<SmsSection />);
        expect(screen.getByTestId("smsStatusNotVerified")).toBeTruthy();
        expect(screen.getByTestId("verifySMSBtn")).toBeTruthy();
    });

    it("masks typing and asks the server whether the number is valid", async () => {
        const check = jest
            .spyOn(services.userProfile, "isPhoneNumberValid")
            .mockResolvedValue(false);
        await render(<SmsSection />);
        await press(screen.getByTestId("editSMSBtn"));
        await changeText(screen.getByTestId("smsNumberInput"), "6045550000");
        await advanceTimers(600);
        expect(check).toHaveBeenCalledWith("6045550000");
        expect(screen.getByText("Valid SMS number is required")).toBeTruthy();
        expect(isDisabled(screen.getByTestId("saveSMSEditBtn"))).toBe(true);
    });

    it("enables Save once the server accepts the number", async () => {
        jest.spyOn(
            services.userProfile,
            "isPhoneNumberValid"
        ).mockResolvedValue(true);
        await render(<SmsSection />);
        await press(screen.getByTestId("editSMSBtn"));
        await changeText(screen.getByTestId("smsNumberInput"), "6045550000");
        await advanceTimers(600);
        expect(isDisabled(screen.getByTestId("saveSMSEditBtn"))).toBe(false);
    });

    it("warns when the number is cleared (opt out)", async () => {
        await render(<SmsSection />);
        await press(screen.getByTestId("editSMSBtn"));
        await changeText(screen.getByTestId("smsNumberInput"), "");
        expect(screen.getByTestId("smsOptOutMessage")).toBeTruthy();
    });

    it("shows a spinner inside the field while the server check is pending", async () => {
        let answer: (ok: boolean) => void = () => undefined;
        jest.spyOn(services.userProfile, "isPhoneNumberValid").mockReturnValue(
            new Promise<boolean>((resolve) => (answer = resolve))
        );
        await render(<SmsSection />);
        await press(screen.getByTestId("editSMSBtn"));
        await changeText(screen.getByTestId("smsNumberInput"), "6045550000");
        expect(screen.getByTestId("smsNumberPending")).toBeTruthy();
        await advanceTimers(600);
        answer(true);
        await advanceTimers(0);
        expect(screen.queryByTestId("smsNumberPending")).toBeNull();
    });

    it("saves under the profile loader, then opens the verify dialog", async () => {
        jest.spyOn(
            services.userProfile,
            "isPhoneNumberValid"
        ).mockResolvedValue(true);
        let finish: () => void = () => undefined;
        const save = jest
            .spyOn(services.userProfile, "updateSmsNumber")
            .mockReturnValueOnce(
                new Promise<void>((resolve) => (finish = resolve))
            )
            .mockResolvedValue(undefined);
        jest.spyOn(services.userProfile, "getProfile").mockResolvedValue(
            makeProfile({ smsNumber: "6045550000" })
        );
        await render(<SmsSection />);
        await press(screen.getByTestId("editSMSBtn"));
        await changeText(screen.getByTestId("smsNumberInput"), "6045550000");
        await advanceTimers(600);
        await press(screen.getByTestId("saveSMSEditBtn"));
        expect(save).toHaveBeenCalledWith("HDID1", "6045550000");
        expect(
            selectLoaderActive(Loader.UserProfile)(useLoadingStore.getState())
        ).toBe(true);
        finish();
        await advanceTimers(0);
        expect(
            selectLoaderActive(Loader.UserProfile)(useLoadingStore.getState())
        ).toBe(false);
        expect(screen.getByTestId("verifySMSModal")).toBeTruthy();
    });
});
