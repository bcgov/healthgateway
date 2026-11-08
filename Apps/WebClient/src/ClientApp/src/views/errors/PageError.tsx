import { View } from "react-native";

import { AppText } from "@/components/ui";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { usePageTitle } from "@/hooks/usePageTitle";

interface Props {
    title: string;
    subtitle?: string;
    testID?: string;
    children?: React.ReactNode;
}

/**
 * Port of PageErrorComponent.vue: centred column (12 / md 8 / lg 6), bold primary headline,
 * title-large subtitle, slot content, then a divider.
 */
export function PageError({ title, subtitle, testID, children }: Props) {
    const { mdAndUp, lgAndUp } = useBreakpoint();
    usePageTitle([title, subtitle].filter(Boolean).join(" "));
    const widthClass = lgAndUp ? "w-1/2" : mdAndUp ? "w-8/12" : "w-full";
    return (
        <View className="items-center">
            <View testID={testID} className={`${widthClass} items-center`}>
                <View className="mb-4 items-center">
                    <AppText
                        variant="h1"
                        accessibilityRole="header"
                        aria-level={1}
                        className="mb-2 text-center"
                    >
                        {title}
                    </AppText>
                    {subtitle ? (
                        <AppText
                            variant="title-large"
                            accessibilityRole="header"
                            aria-level={2}
                            className="mt-1 text-center"
                        >
                            {subtitle}
                        </AppText>
                    ) : null}
                </View>
                <View className="items-center">{children}</View>
                <View className="mt-4 h-px w-full bg-border-divider" />
            </View>
        </View>
    );
}
