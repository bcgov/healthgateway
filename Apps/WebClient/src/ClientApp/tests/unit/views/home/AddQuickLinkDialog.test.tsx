import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { UserPreferenceType } from "@/constants/userPreferenceType";
import { ResultError } from "@/models/errors";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { resetStores, signInRegistered } from "@/test/fixtures";
import {
    isChecked,
    isDisabled,
    press,
    pressKey,
    render,
    screen,
    waitFor,
} from "@/test/ui";
import { AddQuickLinkDialog } from "@/views/home/AddQuickLinkDialog";

describe("AddQuickLinkDialog (AddQuickLinkComponent.vue port)", () => {
    beforeEach(() => signInRegistered());
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("offers only enabled datasets that are not already quick links", async () => {
        await render(<AddQuickLinkDialog visible onClose={() => undefined} />);
        // fixture: Immunization enabled, Medication disabled
        expect(screen.getByTestId("Immunization-filter")).toBeTruthy();
        expect(screen.queryByTestId("Medication-filter")).toBeNull();
        expect(isDisabled(screen.getByTestId("add-quick-link-btn"))).toBe(true);
    });

    it("lists a hidden feature card so it can be brought back", async () => {
        signInRegistered({
            preferences: {
                [UserPreferenceType.HideOrganDonorQuickLink]: {
                    hdId: "HDID1",
                    preference: UserPreferenceType.HideOrganDonorQuickLink,
                    value: "true",
                    version: 1,
                    createdDateTime: "x",
                },
            },
        });
        await render(<AddQuickLinkDialog visible onClose={() => undefined} />);
        expect(
            screen.getByTestId("organ-donor-registration-filter")
        ).toBeTruthy();
    });

    it("saves the selected dataset as a quick link and closes", async () => {
        const update = jest.fn(async (_arg: unknown) => undefined);
        useUserStore.setState({ updateQuickLinks: update });
        const onClose = jest.fn();
        await render(<AddQuickLinkDialog visible onClose={onClose} />);
        await press(screen.getByTestId("Immunization-filter"));
        expect(isChecked(screen.getByTestId("Immunization-filter"))).toBe(true);
        await press(screen.getByTestId("add-quick-link-btn"));
        await waitFor(() => expect(onClose).toHaveBeenCalled());
        expect(update).toHaveBeenCalledWith([
            { name: "Immunizations", filter: { modules: ["Immunization"] } },
        ]);
    });

    it("shows the error alert when saving fails", async () => {
        useUserStore.setState({
            updateQuickLinks: async () => {
                throw new Error("boom");
            },
        });
        await render(<AddQuickLinkDialog visible onClose={() => undefined} />);
        await press(screen.getByTestId("Immunization-filter"));
        await press(screen.getByTestId("add-quick-link-btn"));
        await waitFor(() =>
            expect(screen.getByTestId("quick-link-modal-error")).toBeTruthy()
        );
    });

    it("a 429 shows the too-many message inside the dialog, and Cancel clears it", async () => {
        useUserStore.setState({
            updateQuickLinks: async () => {
                throw new ResultError("CE-HG", "busy", 429);
            },
        });
        const onClose = jest.fn();
        await render(<AddQuickLinkDialog visible onClose={onClose} />);
        await press(screen.getByTestId("Immunization-filter"));
        await press(screen.getByTestId("add-quick-link-btn"));
        await waitFor(() =>
            expect(screen.getByTestId("too-many-requests-error")).toBeTruthy()
        );
        expect(useErrorStore.getState().tooManyRequestsError).toBe(
            "addQuickLinkModal"
        );
        expect(screen.queryByTestId("quick-link-modal-error")).toBeNull();
        await press(screen.getByTestId("cancel-add-quick-link-btn"));
        expect(useErrorStore.getState().tooManyRequestsError).toBeUndefined();
        expect(onClose).toHaveBeenCalled();
    });

    it("web: Space toggles a chip, as on a checkbox", async () => {
        if (Platform.OS !== "web") return;
        await render(<AddQuickLinkDialog visible onClose={() => undefined} />);
        await pressKey(screen.getByTestId("Immunization-filter"), " ");
        expect(isChecked(screen.getByTestId("Immunization-filter"))).toBe(true);
        await pressKey(screen.getByTestId("Immunization-filter"), " ");
        expect(isChecked(screen.getByTestId("Immunization-filter"))).toBe(
            false
        );
    });
});
