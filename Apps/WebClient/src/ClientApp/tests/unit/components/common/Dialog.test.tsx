import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { AppText } from "@/components/common/AppText";
import { Dialog } from "@/components/common/Dialog";
import { HgButton } from "@/components/common/HgButton";
import { press, render, screen } from "@/test/ui";

describe("Dialog (v-dialog port)", () => {
    it("web: a press inside the card stays inside: it does not close the dialog", async () => {
        // only the web renderer can show this: RN Testing Library runs the nearest onPress up the tree instead of the
        // responder negotiation, in which the card claims the touch before the scrim sees it
        if (Platform.OS !== "web") return;
        const onRequestClose = jest.fn();
        await render(
            <Dialog visible title="Note" onRequestClose={onRequestClose}>
                <AppText>Body</AppText>
            </Dialog>
        );
        await press(screen.getByText("Body"));
        await press(screen.getByText("Note"));
        expect(onRequestClose).not.toHaveBeenCalled();
    });

    it("web: a press on the scrim closes it, unless the dialog is persistent", async () => {
        if (Platform.OS !== "web") return;
        const onRequestClose = jest.fn();
        await render(
            <Dialog
                visible
                title="Note"
                testID="dlg"
                onRequestClose={onRequestClose}
            >
                <AppText>Body</AppText>
            </Dialog>
        );
        const scrim = (screen.getByTestId("dlg") as unknown as HTMLElement)
            .parentElement as HTMLElement;
        await press(scrim as never);
        expect(onRequestClose).toHaveBeenCalledTimes(1);
        await render(
            <Dialog
                visible
                persistent
                title="Note"
                testID="dlg"
                onRequestClose={onRequestClose}
            >
                <AppText>Body</AppText>
            </Dialog>
        );
        await press(
            (screen.getByTestId("dlg") as unknown as HTMLElement)
                .parentElement as never
        );
        expect(onRequestClose).toHaveBeenCalledTimes(1);
    });

    it("web: the only tab stops inside the dialog are its controls, so focus never lands on a nameless box", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <Dialog
                visible
                tone="primary"
                title="Note"
                testID="dlg"
                closeTestID="close"
                onRequestClose={() => undefined}
                actions={
                    <>
                        <HgButton
                            text="Cancel"
                            variant="secondary"
                            onPress={() => undefined}
                        />
                        <HgButton text="OK" onPress={() => undefined} />
                    </>
                }
            >
                <AppText>Body</AppText>
            </Dialog>
        );
        const card = screen.getByTestId("dlg") as unknown as HTMLElement;
        const scrim = card.parentElement as HTMLElement;
        expect(scrim.hasAttribute("tabindex")).toBe(false);
        const stops = [...scrim.querySelectorAll("[tabindex]")].map(
            (el) =>
                `${el.getAttribute("tabindex")}:${
                    el.getAttribute("aria-label") ?? el.textContent
                }`
        );
        expect(stops).toEqual(["0:Close", "0:Cancel", "0:OK"]);
    });

    it("web: the dialog is named by its title, which is a level 2 heading", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <Dialog visible title="Add a note" onRequestClose={() => undefined}>
                <AppText>Body</AppText>
            </Dialog>
        );
        const heading = screen.getByRole(
            "heading",
            "Add a note"
        ) as unknown as HTMLElement;
        expect(heading.tagName).toBe("H2");
        expect(heading.id).toBeTruthy();
        // react-native-web gives the modal its dialog role once its fade-in ends, which jsdom never fires: check the
        // name wiring on the element that gets the role
        const modal = document.querySelector('[aria-modal="true"]');
        expect(modal?.getAttribute("aria-labelledby")).toBe(heading.id);
    });
});
