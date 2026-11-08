import type { ComponentRef, Ref } from "react";
import { View } from "react-native";

import { CommunicationBanner } from "./CommunicationBanner";
import { ErrorCard } from "./ErrorCard";
import { usePageChrome } from "./pageChrome";

/** No box of its own (web only): the page lays out as if it were not there. */
const noBox = { display: "contents" } as object;

/**
 * App.vue's v-main: the communication banner, then the fluid container with the error alerts and the page.
 * `contentRef` (web) wraps the page alone, so the shell can tell when the page has drawn.
 */
export function PageContainer({
    children,
    contentRef,
}: {
    children: React.ReactNode;
    contentRef?: Ref<ComponentRef<typeof View>>;
}) {
    const { hideChrome, hideSidebar: isVppLogin } = usePageChrome();
    const page = contentRef ? (
        <View ref={contentRef} style={noBox}>
            {children}
        </View>
    ) : (
        children
    );
    if (isVppLogin) return <View className="grow">{page}</View>;
    return (
        <View>
            {!hideChrome ? <CommunicationBanner /> : null}
            {/* v-container fluid class="pt-6": full width, 16px sides at every width, 24px top; no max width,
                so the page starts 16px from the drawer however wide the window is */}
            <View className="w-full px-4 pt-6 pb-8">
                {!hideChrome ? <ErrorCard /> : null}
                {page}
            </View>
        </View>
    );
}
