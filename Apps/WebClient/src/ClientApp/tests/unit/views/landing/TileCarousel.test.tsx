import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Linking } from "react-native";

import { getAccessLinks } from "@/constants/accessLinks";
import { Action } from "@/constants/tracking";
import { services } from "@/services/container";
import { press, render, screen } from "@/test/ui";
import { TileCarousel } from "@/views/landing/TileCarousel";

describe("TileCarousel", () => {
    const tiles = getAccessLinks();
    let open: jest.SpiedFunction<typeof Linking.openURL>;
    let track: jest.SpiedFunction<typeof services.tracking.trackEvent>;
    beforeEach(() => {
        open = jest.spyOn(Linking, "openURL").mockResolvedValue(undefined);
        track = jest
            .spyOn(services.tracking, "trackEvent")
            .mockImplementation(() => undefined);
    });
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("renders every tile with a Read more button in the grid", async () => {
        await render(
            <TileCarousel
                tiles={tiles}
                cardTestID="access-card"
                mobileCardTestID="mobile-access-card"
                columns={4}
                carousel={false}
            />
        );
        for (const t of tiles) {
            expect(screen.getByText(t.name)).toBeTruthy();
            expect(screen.getByTestId(`access-card-${t.type}`)).toBeTruthy();
            expect(
                screen.getByTestId(`read-more-button-${t.type}`)
            ).toBeTruthy();
        }
    });

    it("opens the tile's link and tracks it as an external link / card click", async () => {
        await render(
            <TileCarousel
                tiles={tiles}
                cardTestID="access-card"
                mobileCardTestID="mobile-access-card"
                columns={4}
                carousel={false}
            />
        );
        await press(screen.getByTestId(`read-more-button-${tiles[0].type}`));
        expect(open).toHaveBeenCalledWith(tiles[0].link ?? "");
        expect(track).toHaveBeenLastCalledWith(
            expect.objectContaining({
                action: Action.ExternalLink,
                url: tiles[0].link,
            })
        );
        await press(screen.getByTestId(`access-card-${tiles[1].type}`));
        expect(track).toHaveBeenLastCalledWith(
            expect.objectContaining({
                action: Action.CardClick,
                url: tiles[1].link,
            })
        );
    });

    it("renders one delimiter per tile in carousel mode", async () => {
        await render(
            <TileCarousel
                tiles={tiles}
                cardTestID="access-card"
                mobileCardTestID="mobile-access-card"
                columns={4}
                carousel
            />
        );
        expect(screen.getByTestId("mobile-access-card-carousel")).toBeTruthy();
        expect(
            screen.getAllByTestId(/mobile-access-card-carousel-delimiter-/)
        ).toHaveLength(tiles.length);
        for (const t of tiles) {
            expect(
                screen.getByTestId(`mobile-access-card-${t.type}`)
            ).toBeTruthy();
            expect(
                screen.getByTestId(`mobile-read-more-button-${t.type}`)
            ).toBeTruthy();
        }
    });
});
