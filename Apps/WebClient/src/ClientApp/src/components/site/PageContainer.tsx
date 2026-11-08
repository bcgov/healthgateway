import { View } from "react-native";

import { CommunicationBanner } from "./CommunicationBanner";
import { ErrorCard } from "./ErrorCard";
import { usePageChrome } from "./pageChrome";

/** App.vue's v-main: the communication banner, then the fluid container with the error alerts and the page. */
export function PageContainer({ children }: { children: React.ReactNode }) {
    const { hideChrome, hideSidebar: isVppLogin } = usePageChrome();
    if (isVppLogin) return <View className="grow">{children}</View>;
    return (
        <View>
            {!hideChrome ? <CommunicationBanner /> : null}
            {/* v-container fluid class="pt-6": full width, 16px sides at every width, 24px top; no max width,
                so the page starts 16px from the drawer however wide the window is */}
            <View className="w-full px-4 pt-6 pb-8">
                {!hideChrome ? <ErrorCard /> : null}
                {children}
            </View>
        </View>
    );
}
