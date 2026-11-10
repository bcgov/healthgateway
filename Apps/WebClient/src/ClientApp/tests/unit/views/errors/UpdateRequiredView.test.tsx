import { afterEach, describe, expect, it, jest } from "@jest/globals";

import { useAppLockStore } from "@/stores/appLock.store";
import { updateStores } from "@/test/fixtures";
import { render, screen } from "@/test/ui";
import { UpdateRequiredView } from "@/views/errors/UpdateRequiredView";

jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));

describe("UpdateRequiredView", () => {
    afterEach(() => {
        updateStores(() => useAppLockStore.setState({ locked: false }));
    });

    it("says an update must be installed, with no button past it", async () => {
        await render(<UpdateRequiredView />);
        expect(screen.getByText("Update required")).toBeTruthy();
        expect(
            screen.getByText(
                "A new version of Health Gateway is available and must be installed to continue."
            )
        ).toBeTruthy();
        expect(screen.queryByLabelText("Close")).toBeNull();
    });

    it("shows even if the app was locked", async () => {
        useAppLockStore.setState({ locked: true });
        await render(<UpdateRequiredView />);
        expect(screen.getByText("Update required")).toBeTruthy();
        expect(useAppLockStore.getState().locked).toBe(false);
    });

    it("stays shown when the app locks while it is up", async () => {
        await render(<UpdateRequiredView />);
        // back from the background after the idle timeout
        updateStores(() => useAppLockStore.setState({ locked: true }));
        expect(useAppLockStore.getState().locked).toBe(false);
        expect(screen.getByText("Update required")).toBeTruthy();
    });
});
