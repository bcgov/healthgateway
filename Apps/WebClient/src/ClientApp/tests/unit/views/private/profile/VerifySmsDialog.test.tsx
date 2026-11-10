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
import { changeText, press, render, screen, waitFor } from "@/test/ui";
import { VerifySmsDialog } from "@/views/private/profile/VerifySmsDialog";

describe("VerifySmsDialog (VerifySmsDialogComponent.vue port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("sends a code on first open and shows the resend countdown", async () => {
        const send = jest
            .spyOn(services.userProfile, "updateSmsNumber")
            .mockResolvedValue(undefined);
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={() => undefined}
                onVerified={() => undefined}
            />
        );
        await waitFor(() =>
            expect(send).toHaveBeenCalledWith("HDID1", "2505551234")
        );
        expect(screen.getByText("(250) 555-1234")).toBeTruthy();
        await waitFor(() =>
            expect(screen.getByTestId("countdownText")).toBeTruthy()
        );
    });

    it("verifies once six digits are entered and closes on success", async () => {
        useUserStore.setState({
            smsResendDateTime: DateTime.now().toISO() ?? undefined,
        });
        const validate = jest
            .spyOn(services.userProfile, "validateSms")
            .mockResolvedValue(true);
        const onClose = jest.fn();
        const onVerified = jest.fn();
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={onClose}
                onVerified={onVerified}
            />
        );
        await changeText(
            screen.getByTestId("verifySMSModalCodeInput"),
            "123456"
        );
        await waitFor(() => expect(onVerified).toHaveBeenCalled());
        expect(validate).toHaveBeenCalledWith("HDID1", "123456");
        expect(onClose).toHaveBeenCalled();
    });

    it("shows the invalid-code message when the server rejects it", async () => {
        useUserStore.setState({
            smsResendDateTime: DateTime.now().toISO() ?? undefined,
        });
        jest.spyOn(services.userProfile, "validateSms").mockResolvedValue(
            false
        );
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={() => undefined}
                onVerified={() => undefined}
            />
        );
        await changeText(
            screen.getByTestId("verifySMSModalCodeInput"),
            "000000"
        );
        await waitFor(() =>
            expect(
                screen.getByText("Invalid verification code. Try again.")
            ).toBeTruthy()
        );
    });

    it("offers Resend once the cooldown has passed", async () => {
        useUserStore.setState({
            smsResendDateTime:
                DateTime.now().minus({ minutes: 5 }).toISO() ?? undefined,
        });
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={() => undefined}
                onVerified={() => undefined}
            />
        );
        expect(screen.getByText("Resend")).toBeTruthy();
    });

    it("a 429 while verifying shows the too-many message inside the dialog", async () => {
        useUserStore.setState({
            smsResendDateTime: DateTime.now().toISO() ?? undefined,
        });
        jest.spyOn(services.userProfile, "validateSms").mockRejectedValue(
            new ResultError("CE-HG", "busy", 429)
        );
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={() => undefined}
                onVerified={() => undefined}
            />
        );
        await changeText(
            screen.getByTestId("verifySMSModalCodeInput"),
            "123456"
        );
        await waitFor(() =>
            expect(screen.getByTestId("too-many-requests-error")).toBeTruthy()
        );
        expect(useErrorStore.getState().tooManyRequestsError).toBe(
            "verifySmsModal"
        );
    });

    it("any other failure shows the unexpected-error text and an OK button", async () => {
        useUserStore.setState({
            smsResendDateTime: DateTime.now().toISO() ?? undefined,
        });
        jest.spyOn(services.userProfile, "validateSms").mockRejectedValue(
            new ResultError("CE-HG", "boom", 500)
        );
        const onClose = jest.fn();
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={onClose}
                onVerified={() => undefined}
            />
        );
        await changeText(
            screen.getByTestId("verifySMSModalCodeInput"),
            "123456"
        );
        await waitFor(() =>
            expect(
                screen.getByText(
                    "An unexpected error has occurred. Please try refreshing your browser or try again later."
                )
            ).toBeTruthy()
        );
        await press(screen.getByText("OK"));
        expect(onClose).toHaveBeenCalled();
    });

    it("has the web client's close button id and title", async () => {
        useUserStore.setState({
            smsResendDateTime: DateTime.now().toISO() ?? undefined,
        });
        const onClose = jest.fn();
        await render(
            <VerifySmsDialog
                visible
                smsNumber="2505551234"
                onClose={onClose}
                onVerified={() => undefined}
            />
        );
        expect(screen.getByText("Phone Verification")).toBeTruthy();
        await press(screen.getByTestId("messageModalCloseButton"));
        expect(onClose).toHaveBeenCalled();
    });
});
