import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { fireEvent } from "@testing-library/react-native";
import { act } from "react";
import { Platform } from "react-native";

import { ProtectiveWordDialog } from "@/components/site/ProtectiveWordDialog";
import { ActionType } from "@/constants/actionType";
import { LoadStatus } from "@/models/datasetState";
import { services } from "@/services/container";
import { useMedicationStore } from "@/stores/medication.store";
import {
    configFixture,
    fakeAuth,
    medicationStatementsFixture,
    requestResult,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    changeText,
    isDisabled,
    press,
    render,
    screen,
    waitFor,
} from "@/test/ui";

const protectedState = (attempts: number) =>
    updateStores(() =>
        useMedicationStore.setState({
            states: {
                HDID1: {
                    data: [],
                    status: LoadStatus.PROTECTED,
                    statusMessage: "",
                    protectiveWordAttempts: attempts,
                },
            },
        })
    );

describe("ProtectiveWordDialog (ProtectiveWordComponent.vue port)", () => {
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

    it("hidden unless the medications are protected", async () => {
        await render(<ProtectiveWordDialog hdid="HDID1" />);
        expect(screen.queryByTestId("protectiveWordModal")).toBeNull();
    });

    it("asks for the word, retries with it, and closes once the medications load", async () => {
        protectedState(1);
        const get = jest
            .spyOn(services.medication, "getPatientMedicationStatements")
            .mockResolvedValue(requestResult(medicationStatementsFixture()));
        await render(<ProtectiveWordDialog hdid="HDID1" />);
        expect(screen.getByText("Restricted PharmaNet Records")).toBeTruthy();
        expect(
            screen.queryByText("Invalid protective word. Try again.")
        ).toBeNull();
        expect(
            isDisabled(screen.getByTestId("protectiveWordContinueBtn"))
        ).toBe(true);
        await changeText(screen.getByTestId("protectiveWordInput"), "word");
        await press(screen.getByTestId("protectiveWordContinueBtn"));
        expect(get).toHaveBeenCalledWith("HDID1", "word");
        await waitFor(() =>
            expect(useMedicationStore.getState().states.HDID1.status).toBe(
                LoadStatus.LOADED
            )
        );
    });

    it("native: the field takes the cursor, and the return key submits the word (nothing while it is empty)", async () => {
        if (Platform.OS === "web") return; // the web client's field has no Enter
        protectedState(1);
        const get = jest
            .spyOn(services.medication, "getPatientMedicationStatements")
            .mockResolvedValue(requestResult(medicationStatementsFixture()));
        await render(<ProtectiveWordDialog hdid="HDID1" />);
        const input = screen.getByTestId("protectiveWordInput");
        // and the field takes the cursor as the dialog opens
        expect(input.props.autoFocus).toBe(true);
        await act(async () => {
            await fireEvent(input, "submitEditing");
        });
        expect(get).not.toHaveBeenCalled();
        await changeText(input, "word");
        await act(async () => {
            await fireEvent(input, "submitEditing");
        });
        expect(get).toHaveBeenCalledWith("HDID1", "word");
    });

    it("a second attempt shows the invalid-word message; Close dismisses it", async () => {
        protectedState(2);
        const onAbort = jest.fn();
        await render(<ProtectiveWordDialog hdid="HDID1" onAbort={onAbort} />);
        expect(
            screen.getByText("Invalid protective word. Try again.")
        ).toBeTruthy();
        await press(screen.getByTestId("protectiveWordCloseButton"));
        expect(onAbort).toHaveBeenCalled();
    });

    it("the action code PROTECTED from the API opens it", async () => {
        jest.spyOn(
            services.medication,
            "getPatientMedicationStatements"
        ).mockResolvedValue(
            requestResult([], 2, {
                errorCode: "x",
                traceId: "t",
                actionCode: ActionType.Protected,
            })
        );
        await render(<ProtectiveWordDialog hdid="HDID1" />);
        await act(async () => {
            await useMedicationStore.getState().retrieveMedications("HDID1");
        });
        await waitFor(() =>
            expect(screen.getByTestId("protectiveWordInput")).toBeTruthy()
        );
    });
});
