import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { ActionType } from "@/constants/actionType";
import { ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import {
    changeText,
    inputValue,
    isDisabled,
    press,
    render,
    screen,
    textOf,
} from "@/test/ui";
import { formatDate, today } from "@/utils/dateUtil";
import {
    AddDependentDialog,
    dependentFieldError,
} from "@/views/private/dependents/AddDependentDialog";

const birthdate = today().minus({ years: 5 });
const valid = {
    firstName: "Sam",
    lastName: "Testfive",
    dateOfBirth: formatDate(birthdate, "yyyy-MM-dd"),
    PHN: "9874 307 168",
};

async function fill(fields = valid) {
    await changeText(
        screen.getByTestId("dependent-first-name-input"),
        fields.firstName
    );
    await changeText(
        screen.getByTestId("dependent-last-name-input"),
        fields.lastName
    );
    await changeText(
        screen.getByTestId("dependent-date-of-birth-input"),
        formatDate(birthdate).toUpperCase()
    );
    await changeText(
        screen.getByTestId("dependent-phn-input"),
        fields.PHN.replaceAll(" ", "")
    );
    await press(screen.getByTestId("dependent-terms-checkbox"));
}

describe("dependentFieldError (AddDependentComponent.vue rules)", () => {
    const error = (
        field: keyof typeof valid,
        value: string,
        phns: string[] = []
    ) => dependentFieldError(field, { ...valid, [field]: value }, 12, phns);

    it("passes a valid dependent", () => {
        for (const field of Object.keys(valid) as (keyof typeof valid)[])
            expect(error(field, valid[field])).toBeUndefined();
    });

    it("requires names", () => {
        expect(error("firstName", "  ")).toBe("Given names are required.");
        expect(error("lastName", "")).toBe("Last name is required.");
    });

    it("checks the date of birth", () => {
        expect(error("dateOfBirth", "")).toBe("Invalid date.");
        expect(
            error(
                "dateOfBirth",
                formatDate(today().minus({ years: 13 }), "yyyy-MM-dd")
            )
        ).toBe("Dependent must be under the age of 12.");
        expect(
            error(
                "dateOfBirth",
                formatDate(today().plus({ days: 2 }), "yyyy-MM-dd")
            )
        ).toBe("Invalid Date.");
    });

    it("checks the PHN and refuses one already added", () => {
        expect(error("PHN", "9874")).toBe("Valid PHN is required.");
        expect(error("PHN", "9874 307 169")).toBe(
            "Personal Health Number must be valid."
        );
        expect(error("PHN", "9874 307 168", ["9874307168"])).toBe(
            "This dependent has already been added."
        );
    });
});

describe("AddDependentDialog (AddDependentComponent.vue port)", () => {
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

    it("keeps Register disabled until the form is valid, then posts the cleaned values", async () => {
        const add = jest
            .spyOn(services.dependent, "addDependent")
            .mockResolvedValue(undefined);
        const onSubmit = jest.fn();
        await render(<AddDependentDialog onSubmit={onSubmit} />);
        await press(screen.getByTestId("add-dependent-button"));
        expect(isDisabled(screen.getByTestId("register-dependent-btn"))).toBe(
            true
        );
        await fill({ ...valid, firstName: " Sam   Lee " });
        expect(isDisabled(screen.getByTestId("register-dependent-btn"))).toBe(
            false
        );
        await press(screen.getByTestId("register-dependent-btn"));
        expect(add.mock.calls[0][1]).toEqual({
            firstName: "Sam Lee",
            lastName: "Testfive",
            dateOfBirth: valid.dateOfBirth,
            PHN: "9874307168",
        });
        expect(onSubmit).toHaveBeenCalled();
    });

    it.each([
        [
            ActionType.DataMismatch,
            "does not match our records",
            "not-condensed-error-contact-message",
        ],
        [
            ActionType.Protected,
            "Unable to add dependent.",
            "condensed-error-contact-message",
        ],
        [undefined, "Server said no", "not-condensed-error-contact-message"],
    ])(
        "explains a %s failure and keeps the dialog open",
        async (action, copy, contactId) => {
            const failure = new ResultError("X", "Server said no", 500);
            failure.actionCode = action;
            jest.spyOn(services.dependent, "addDependent").mockRejectedValue(
                failure
            );
            await render(<AddDependentDialog onSubmit={() => undefined} />);
            await press(screen.getByTestId("add-dependent-button"));
            await fill();
            await press(screen.getByTestId("register-dependent-btn"));
            expect(
                textOf(screen.getByTestId("dependent-error-text"))
            ).toContain(copy);
            expect(screen.getByTestId(contactId)).toBeTruthy();
            expect(screen.getByTestId("add-dependent-dialog")).toBeTruthy();
        }
    );

    it("shows too many requests inside the dialog", async () => {
        jest.spyOn(services.dependent, "addDependent").mockRejectedValue(
            new ResultError("X", "busy", 429)
        );
        await render(<AddDependentDialog onSubmit={() => undefined} />);
        await press(screen.getByTestId("add-dependent-button"));
        await fill();
        await press(screen.getByTestId("register-dependent-btn"));
        expect(useErrorStore.getState().tooManyRequestsError).toBe(
            "addDependentDialog"
        );
        expect(screen.getByTestId("too-many-requests-error")).toBeTruthy();
    });

    it("Cancel clears the form", async () => {
        await render(<AddDependentDialog onSubmit={() => undefined} />);
        await press(screen.getByTestId("add-dependent-button"));
        await changeText(
            screen.getByTestId("dependent-first-name-input"),
            "Sam"
        );
        await press(screen.getByTestId("cancel-dependent-registration-btn"));
        await press(screen.getByTestId("add-dependent-button"));
        expect(
            inputValue(screen.getByTestId("dependent-first-name-input"))
        ).toBe("");
    });
});
