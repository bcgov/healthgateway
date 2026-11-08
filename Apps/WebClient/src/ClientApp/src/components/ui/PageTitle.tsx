import { View } from "react-native";

import { useBreakpoint } from "@/hooks/useBreakpoint";
import { usePageTitle } from "@/hooks/usePageTitle";

import { AppText } from "./AppText";

/**
 * Port of PageTitleComponent.vue: `#pageTitle` row with the `h1#subject` title (headline-small on mobile,
 * headline-large otherwise) and an optional appended control on the right.
 */
export function PageTitle({
    title,
    append,
    prepend,
    testID,
}: {
    title: string;
    append?: React.ReactNode;
    /** #prepend slot (the dependent timeline's back button) */
    prepend?: React.ReactNode;
    testID?: string;
}) {
    const { isMobile } = useBreakpoint();
    usePageTitle(title);
    return (
        <View
            nativeID="pageTitle"
            testID={testID}
            className="mb-4 flex-row items-center"
        >
            {prepend ? <View className="mr-2">{prepend}</View> : null}
            <AppText
                nativeID="subject"
                variant="h1"
                accessibilityRole="header"
                aria-level={1}
                className={`flex-1 ${isMobile ? "text-headline-small" : "text-headline-large"}`}
            >
                {title}
            </AppText>
            {append ? <View className="ml-2">{append}</View> : null}
        </View>
    );
}
