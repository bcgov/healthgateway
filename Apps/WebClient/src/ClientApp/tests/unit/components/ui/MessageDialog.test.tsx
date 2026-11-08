import { describe, expect, it, jest } from "@jest/globals";

import { MessageDialog } from "@/components/ui/MessageDialog";
import { press, render, screen } from "@/test/ui";

describe("MessageDialog (MessageModalComponent.vue port)", () => {
    it("default: Cancel closes, Continue closes then submits", async () => {
        const onClose = jest.fn();
        const onSubmit = jest.fn();
        await render(
            <MessageDialog
                visible
                title="Sensitive Document"
                message="The file contains personal information."
                onClose={onClose}
                onSubmit={onSubmit}
            />
        );
        expect(screen.getByText("Sensitive Document")).toBeTruthy();
        expect(screen.getByText("Cancel")).toBeTruthy();
        await press(screen.getByTestId("generic-message-submit-btn"));
        expect(onClose).toHaveBeenCalledTimes(1);
        expect(onSubmit).toHaveBeenCalledTimes(1);
        await press(screen.getByTestId("generic-message-ok-btn"));
        expect(onClose).toHaveBeenCalledTimes(2);
        expect(onSubmit).toHaveBeenCalledTimes(1);
    });

    it("okOnly: a single OK button", async () => {
        await render(
            <MessageDialog
                visible
                okOnly
                title="Copy to Clipboard"
                message="Copied Successfully"
                onClose={() => undefined}
            />
        );
        expect(screen.getByText("OK")).toBeTruthy();
        expect(screen.queryByTestId("generic-message-submit-btn")).toBeNull();
    });
});
