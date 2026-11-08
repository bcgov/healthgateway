import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { DataSource } from "@/constants/dataSource";
import { HealthDataType } from "@/models/patientData";
import { services } from "@/services/container";
import { useErrorStore } from "@/stores/error.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import { hasShadow, press, render, screen, textOf } from "@/test/ui";
import { saveBase64File } from "@/utils/saveFile";
import { ServicesView } from "@/views/services/ServicesView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/saveFile", () => ({
    saveBase64File: jest.fn(() => Promise.resolve()),
}));

const registration = (overrides = {}) => ({
    id: "1",
    type: HealthDataType.OrganDonorRegistration,
    status: "Registered",
    statusMessage: "Your decision about organ donation has been registered.",
    registrationFileId: "FILE",
    organDonorRegistrationLinkText: "Update your decision",
    ...overrides,
});

const mockPatientData = (item: object) =>
    jest
        .spyOn(services.patientData, "getPatientData")
        .mockResolvedValue({ items: [item] } as never);

describe("ServicesView (ServicesView.vue, OrganDonorDetailsCard.vue ports)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
        (saveBase64File as jest.Mock).mockClear();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows a registered patient's status and downloads the decision after the prompt", async () => {
        const get = mockPatientData(registration());
        jest.spyOn(services.patientData, "getFile").mockResolvedValue({
            content: [1, 2, 3],
            contentType: "application/pdf",
        });
        await render(<ServicesView />);
        expect(get).toHaveBeenCalled();
        expect(
            textOf(screen.getByTestId("organ-donor-registration-status"))
        ).toBe("Registered");
        expect(
            textOf(screen.getByTestId("organ-donor-registration-link"))
        ).toBe("Update your decision");
        await press(
            screen.getByTestId("organ-donor-registration-download-button")
        );
        await press(screen.getByTestId("generic-message-submit-btn"));
        expect(saveBase64File).toHaveBeenCalledWith(
            expect.any(String),
            "application/pdf",
            "Organ_Donor_Registration.pdf"
        );
    });

    it("shows the registration as an elevated card with no border or hover (healthgateway d7ab5c429)", async () => {
        mockPatientData(registration());
        await render(<ServicesView />);
        const card = screen.getByTestId("organ-donor-registration-card");
        expect(hasShadow(card)).toBe(true);
        // NativeWind classes compile away on web; the native test renderer keeps them
        if (Platform.OS === "web") return;
        expect(card.props.className).not.toMatch(/\bborder\b|hover:/);
    });

    it("explains a registration without a file", async () => {
        mockPatientData(registration({ registrationFileId: undefined }));
        await render(<ServicesView />);
        expect(
            textOf(
                screen.getByTestId(
                    "organ-donor-registration-decision-registered-no-file"
                )
            )
        ).toContain(
            "Download only available to registrants after November 2022"
        );
    });

    it("shows Not Available for an unregistered patient", async () => {
        mockPatientData(registration({ status: "Not Registered" }));
        await render(<ServicesView />);
        expect(
            textOf(
                screen.getByTestId("organ-donor-registration-decision-no-file")
            )
        ).toBe("Not Available");
    });

    it("a blocked data source fetches nothing, hides the card and adds the error", async () => {
        signInRegistered({
            blockedDataSources: [DataSource.OrganDonorRegistration],
        });
        const get = mockPatientData(registration());
        await render(<ServicesView />);
        expect(get).not.toHaveBeenCalled();
        expect(
            screen.queryByTestId("organ-donor-registration-card")
        ).toBeNull();
        expect(JSON.stringify(useErrorStore.getState().errors)).toContain(
            "Organ Donor Registration is not available at this time. Please try again later."
        );
    });
});
