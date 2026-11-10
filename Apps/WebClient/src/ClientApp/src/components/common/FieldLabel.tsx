import { View } from "react-native";

import { cn } from "@/utils/cn";

import { AppText } from "./AppText";

interface Props {
    title: string;
    /** content after the title on the same line (LabelComponent #append slot), e.g. an Edit link */
    append?: React.ReactNode;
    /** LabelComponent `margin` (default mb-2) */
    className?: string;
    testID?: string;
    /** heading level (default 4, as LabelComponent.vue); 3 under an h2 section with no h3 between */
    level?: 3 | 4;
}

/** Port of LabelComponent.vue: an h4 in `text-body-large font-weight-bold`, with an optional appended control. */
export function FieldLabel({
    title,
    append,
    className = "mb-2",
    testID,
    level = 4,
}: Props) {
    return (
        <View className={cn("flex-row items-center", className)}>
            <AppText
                className="font-bold"
                accessibilityRole="header"
                aria-level={level}
                testID={testID}
            >
                {title}
            </AppText>
            {append}
        </View>
    );
}
