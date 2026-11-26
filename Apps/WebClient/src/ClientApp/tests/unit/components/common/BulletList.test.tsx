import { describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import { BulletList, BulletListItem } from "@/components/common/BulletList";
import { render, screen, tagOf, textOf } from "@/test/ui";

describe("BulletList", () => {
    it("web: renders as a ul of li items, each with its bullet", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <BulletList testID="list">
                <BulletListItem testID="item">HealthLink BC</BulletListItem>
            </BulletList>
        );
        expect(tagOf(screen.getByTestId("list"))).toBe("ul");
        const item = screen.getByTestId("item");
        expect(tagOf(item)).toBe("li");
        expect(textOf(item)).toBe("•HealthLink BC");
    });
});
