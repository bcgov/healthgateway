import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import type { Address } from "@/models/patient";
import { services } from "@/services/container";
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { attrOf, render, screen, textOf } from "@/test/ui";
import { AddressSection } from "@/views/profile/AddressSection";

const home: Address = {
    streetLines: ["1 Main St"],
    city: "Victoria",
    state: "BC",
    postalCode: "V8V 1A1",
    country: "CA",
};
const box: Address = { ...home, streetLines: ["PO Box 9"] };

async function renderWith(physical?: Address, postal?: Address) {
    services.initialize(configFixture, fakeAuth());
    signInRegistered();
    updateStores(() =>
        useUserStore.setState({
            patient: {
                hdid: "HDID1",
                personalHealthNumber: "9735353315",
                physicalAddress: physical,
                postalAddress: postal,
            },
        })
    );
    await render(<AddressSection />);
}

describe("AddressSection (UserProfileAddressComponent.vue port)", () => {
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("one address when both match", async () => {
        await renderWith(home, home);
        expect(textOf(screen.getByTestId("postal-address-label"))).toBe(
            "Address"
        );
        expect(screen.queryByTestId("physical-address-section")).toBeNull();
        expect(
            screen.getByText(/If this address is incorrect, update it/)
        ).toBeTruthy();
    });

    it("mailing and physical addresses when they differ", async () => {
        await renderWith(home, box);
        expect(textOf(screen.getByTestId("postal-address-label"))).toBe(
            "Mailing Address"
        );
        expect(screen.getByTestId("physical-address-section")).toBeTruthy();
        expect(
            screen.getByText(/If either of these addresses is incorrect/)
        ).toBeTruthy();
    });

    it("no address: the add-an-address sentence links to Address Change BC", async () => {
        await renderWith(undefined, undefined);
        expect(screen.getByTestId("no-postal-address-text")).toBeTruthy();
        const link = screen.getByText("this page");
        if (Platform.OS === "web")
            expect(attrOf(link, "href")).toBe(
                "https://www.addresschange.gov.bc.ca"
            );
    });
});
