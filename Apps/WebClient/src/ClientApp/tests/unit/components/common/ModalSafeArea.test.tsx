import { describe, expect, it } from "@jest/globals";
import { Platform, StyleSheet, View } from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";

import { ModalSafeArea } from "@/components/common/ModalSafeArea";
import { render, screen } from "@/test/ui";

interface HostNode {
    parent: HostNode | null;
    props: { style?: unknown };
}

/** The padding the wrapper puts around the content element. */
function paddingAround(el: unknown): { top: string; bottom: string } {
    if (Platform.OS === "web") {
        const style = (el as HTMLElement).parentElement!.style;
        return { top: style.paddingTop, bottom: style.paddingBottom };
    }
    let node = (el as HostNode).parent;
    while (node && !node.props.style) node = node.parent;
    const style = StyleSheet.flatten(node?.props.style as never) as {
        paddingTop?: number;
        paddingBottom?: number;
    };
    return { top: `${style.paddingTop}px`, bottom: `${style.paddingBottom}px` };
}

describe("ModalSafeArea", () => {
    it("pads modal content by the insets it measures itself", async () => {
        // native: the test root reports a 47pt status bar and a 34pt home indicator; web: the provider reads CSS
        // env(safe-area-inset-*), which jsdom does not have
        await render(
            <ModalSafeArea>
                <View testID="content" />
            </ModalSafeArea>
        );
        expect(paddingAround(screen.getByTestId("content"))).toEqual(
            Platform.OS === "web"
                ? { top: "", bottom: "" }
                : { top: "47px", bottom: "34px" }
        );
    });

    if (Platform.OS === "web") return;

    // an iPhone with a Dynamic Island in landscape, which has it on the left here
    const landscape = {
        insets: { top: 0, right: 62, bottom: 21, left: 62 },
        frame: { x: 0, y: 0, width: 956, height: 440 },
    };
    const sidePadding = (el: unknown) => {
        let node = (el as HostNode).parent;
        while (node && !node.props.style) node = node.parent;
        const { paddingLeft, paddingRight } = StyleSheet.flatten(
            node?.props.style as never
        ) as { paddingLeft?: number; paddingRight?: number };
        return { left: paddingLeft, right: paddingRight };
    };

    it("keeps full-screen content clear of the camera in landscape", async () => {
        await render(
            <SafeAreaProvider initialMetrics={landscape}>
                <ModalSafeArea>
                    <View testID="content" />
                </ModalSafeArea>
            </SafeAreaProvider>
        );
        expect(sidePadding(screen.getByTestId("content"))).toEqual({
            left: 62,
            right: 62,
        });
    });

    it("pads a right-docked panel on its right only", async () => {
        await render(
            <SafeAreaProvider initialMetrics={landscape}>
                <ModalSafeArea sides="right">
                    <View testID="content" />
                </ModalSafeArea>
            </SafeAreaProvider>
        );
        expect(sidePadding(screen.getByTestId("content"))).toEqual({
            left: 0,
            right: 62,
        });
    });
});
