import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { AppText } from "@/components/common/AppText";
import { InternalLink } from "@/components/common/InternalLink";
import { resetRouter, router } from "@/test/expoRouterMock";
import { attrOf, idOf, press, render, screen } from "@/test/ui";

describe("InternalLink (router-link)", () => {
    it("carries href, ids and navigates in-app on press", async () => {
        resetRouter("/home");
        const onPress = jest.fn();
        await render(
            <AppText>
                Visit your{" "}
                <InternalLink
                    href="/profile"
                    testID="profile-page-link"
                    nativeID="profilePageLink"
                    onPress={onPress}
                >
                    profile
                </InternalLink>
            </AppText>
        );
        const link = screen.getByTestId("profile-page-link");
        expect(idOf(link)).toBe("profilePageLink");
        if (Platform.OS === "web")
            expect(attrOf(link, "href")).toBe("/profile");
        await press(link);
        expect(onPress).toHaveBeenCalled();
        expect(router.push).toHaveBeenCalledWith("/profile");
    });
});
