import { beforeEach, describe, expect, it, jest } from "@jest/globals";

import { RatingDialog } from "@/components/site/RatingDialog";
import { services } from "@/services/container";
import { configFixture, fakeAuth } from "@/test/fixtures";
import { press, render, screen, waitFor } from "@/test/ui";

describe("RatingDialog", () => {
    const onClose = jest.fn();
    let submitRating: ReturnType<typeof jest.fn>;

    beforeEach(() => {
        onClose.mockClear();
        services.initialize(configFixture, fakeAuth());
        submitRating = jest
            .spyOn(services.userFeedback, "submitRating")
            .mockResolvedValue(undefined) as never;
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });

    it("submits as soon as a star is pressed", async () => {
        await render(<RatingDialog visible onClose={onClose} />);
        await press(screen.getByTestId("formRating-5"));
        await waitFor(() => expect(onClose).toHaveBeenCalled());
        expect(submitRating).toHaveBeenCalledWith({
            ratingValue: 5,
            skip: false,
        });
    });

    it("skips with no rating", async () => {
        await render(<RatingDialog visible onClose={onClose} />);
        await press(screen.getByTestId("ratingModalSkipBtn"));
        await waitFor(() => expect(onClose).toHaveBeenCalled());
        expect(submitRating).toHaveBeenCalledWith({
            ratingValue: 0,
            skip: true,
        });
    });
});
