import { View } from "react-native";

import { AppText } from "./AppText";

interface Props {
    title: string;
    includeDivider?: boolean;
    testID?: string;
    /** heading level (default 3, as SectionHeadingComponent.vue); 2 on a page with no h2 above it, such as Profile */
    level?: 2 | 3 | 4;
}

/**
 * Port of SectionHeadingComponent.vue: optional v-divider (my-4), then an h3 in `text-title-large
 * font-weight-bold my-4` — 22/28 bold in the body text colour (not the primary headline style). CSS collapses those
 * margins with each other and with the content above; React Native does not, so only the bottom ones are kept here.
 */
export function SectionHeading({
    title,
    includeDivider,
    testID,
    level = 3,
}: Props) {
    return (
        <View>
            {includeDivider ? (
                <View className="h-px bg-border-divider mb-4" />
            ) : null}
            <AppText
                variant="title-large"
                className="font-bold mb-4"
                accessibilityRole="header"
                aria-level={level}
                testID={testID}
            >
                {title}
            </AppText>
        </View>
    );
}
