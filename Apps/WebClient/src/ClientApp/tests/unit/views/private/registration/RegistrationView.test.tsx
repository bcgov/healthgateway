import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { useUserStore } from "@/stores/user.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    bcscUserInfo,
    configFixture,
    fakeAuth,
    resetStores,
} from "@/test/fixtures";
import {
    attrOf,
    changeText,
    isDisabled,
    press,
    render,
    screen,
    waitFor,
} from "@/test/ui";
import { RegistrationView } from "@/views/private/registration/RegistrationView";

const terms = {
    id: "tos-3",
    content: "<p>Terms</p>",
    effectiveDate: "2026-01-01",
};

describe("RegistrationView (RegistrationView.vue port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        resetRouter("/registration");
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        useAuthStore.setState({ isAuthenticated: true });
        useUserStore
            .getState()
            .setOidcUserInfo({ ...bcscUserInfo, email: "dr@example.test" });
        jest.spyOn(services.userProfile, "getTermsOfService").mockResolvedValue(
            terms
        );
        jest.spyOn(
            services.userProfile,
            "isPhoneNumberValid"
        ).mockResolvedValue(true);
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("blocks under-age users with the minimum age message", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockResolvedValue(
            false
        );
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("minimumAgeErrorText")).toBeTruthy()
        );
        expect(screen.getByText("12")).toBeTruthy();
    });

    it("shows the client registry error when age validation fails", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockRejectedValue(
            new Error("down")
        );
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("clientRegistryErrorText")).toBeTruthy()
        );
    });

    it("prefills the email, validates the confirmation and requires the terms", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockResolvedValue(true);
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("registerButton")).toBeTruthy()
        );
        expect(isDisabled(screen.getByTestId("registerButton"))).toBe(true);
        await changeText(
            screen.getByTestId("emailConfirmationInput"),
            "other@example.test"
        );
        expect(
            screen.getByText("Both email addresses must match")
        ).toBeTruthy();
        await changeText(screen.getByTestId("smsNumberInput"), "");
        expect(screen.getByText("Value is required")).toBeTruthy();
        await press(screen.getByTestId("acceptCheckbox"));
        expect(isDisabled(screen.getByTestId("registerButton"))).toBe(false);
    });

    it("waits for the phone check when Register is pressed before it has run", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockResolvedValue(true);
        const create = jest.fn(async (_arg: unknown) => undefined);
        useUserStore.setState({ createProfile: create });
        let finishCheck: (valid: boolean) => void = () => undefined;
        const check = jest
            .spyOn(services.userProfile, "isPhoneNumberValid")
            .mockImplementation(
                () =>
                    new Promise((resolve) => {
                        finishCheck = resolve;
                    })
            );
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("registerButton")).toBeTruthy()
        );
        await changeText(
            screen.getByTestId("emailConfirmationInput"),
            "dr@example.test"
        );
        await changeText(screen.getByTestId("smsNumberInput"), "2505084843");
        await press(screen.getByTestId("acceptCheckbox"));
        await press(screen.getByTestId("registerButton"));
        expect(check).toHaveBeenCalled();
        expect(create).not.toHaveBeenCalled();
        finishCheck(true);
        await waitFor(() => expect(create).toHaveBeenCalled());
    });

    it("does not register when the phone check says the number is invalid", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockResolvedValue(true);
        const create = jest.fn(async (_arg: unknown) => undefined);
        useUserStore.setState({ createProfile: create });
        const check = jest
            .spyOn(services.userProfile, "isPhoneNumberValid")
            .mockResolvedValue(false);
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("registerButton")).toBeTruthy()
        );
        await changeText(
            screen.getByTestId("emailConfirmationInput"),
            "dr@example.test"
        );
        await changeText(screen.getByTestId("smsNumberInput"), "2505084843");
        await press(screen.getByTestId("acceptCheckbox"));
        await press(screen.getByTestId("registerButton"));
        await waitFor(() => expect(check).toHaveBeenCalled());
        await waitFor(() =>
            expect(screen.getByText("Invalid phone number")).toBeTruthy()
        );
        expect(create).not.toHaveBeenCalled();
    });

    it("creates the profile with the chosen preferences and goes Home", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockResolvedValue(true);
        const create = jest.fn(async (_arg: unknown) => undefined);
        useUserStore.setState({ createProfile: create });
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("registerButton")).toBeTruthy()
        );
        await press(screen.getByTestId("sms-checkbox"));
        await press(screen.getByTestId("acceptCheckbox"));
        await press(screen.getByTestId("registerButton"));
        await waitFor(() => expect(create).toHaveBeenCalled());
        expect(create).toHaveBeenCalledWith(
            expect.objectContaining({
                hdid: "HDID1",
                termsOfServiceId: "tos-3",
                acceptedTermsOfService: true,
                email: "dr@example.test",
                smsNumber: "",
            })
        );
        await waitFor(() =>
            expect(router.replace).toHaveBeenCalledWith({
                pathname: "/home",
                params: { registration: "success" },
            })
        );
    });

    it("names the contact fields for the screen reader and says what they collect", async () => {
        jest.spyOn(services.userProfile, "validateAge").mockResolvedValue(true);
        await render(<RegistrationView />);
        await waitFor(() =>
            expect(screen.getByTestId("registerButton")).toBeTruthy()
        );
        const email = screen.getByLabelText("Email address");
        const confirmation = screen.getByLabelText("Confirm email address");
        const phone = screen.getByLabelText("Phone number");
        if (Platform.OS === "web") {
            expect(attrOf(email, "autocomplete")).toBe("email");
            expect(attrOf(confirmation, "autocomplete")).toBe("email");
            expect(attrOf(phone, "autocomplete")).toBe("tel");
        } else {
            expect(
                (email as unknown as { props: { autoComplete?: string } }).props
                    .autoComplete
            ).toBe("email");
            expect(
                (phone as unknown as { props: { autoComplete?: string } }).props
                    .autoComplete
            ).toBe("tel");
        }
    });
});
