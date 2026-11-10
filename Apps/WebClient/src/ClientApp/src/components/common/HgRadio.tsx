import { Pressable, View } from "react-native";

import { cn } from "@/utils/cn";
import { spaceToggleProps } from "@/utils/spaceToggle";

import { AppText } from "./AppText";

interface Props {
    selected: boolean;
    onSelect: () => void;
    label: string;
    /** a smaller line under the label, also part of the accessible name */
    description?: string;
    testID?: string;
    className?: string;
}

/** A radio button: drawn with views, as FontAwesome's solid set has no empty circle. Group them in `role="radiogroup"`. */
export function HgRadio({
    selected,
    onSelect,
    label,
    description,
    testID,
    className,
}: Props) {
    return (
        <Pressable
            testID={testID}
            accessibilityRole="radio"
            aria-checked={selected}
            accessibilityLabel={
                description ? `${label}. ${description}` : label
            }
            onPress={onSelect}
            {...spaceToggleProps(onSelect)}
            className={cn(
                "flex-row items-start min-h-[44px] py-2.5 active:bg-surface-hover",
                className
            )}
        >
            <View
                className={cn(
                    "w-5 h-5 mt-0.5 mr-3 rounded-full border-2 items-center justify-center",
                    selected ? "border-primary" : "border-bc-grey"
                )}
            >
                {selected ? (
                    <View className="w-2.5 h-2.5 rounded-full bg-primary" />
                ) : null}
            </View>
            <View className="shrink">
                <AppText>{label}</AppText>
                {description ? (
                    <AppText variant="body-medium" className="mt-1">
                        {description}
                    </AppText>
                ) : null}
            </View>
        </Pressable>
    );
}
