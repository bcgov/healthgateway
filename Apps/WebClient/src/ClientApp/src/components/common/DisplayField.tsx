import { View } from "react-native";

import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import { FieldLabel } from "./FieldLabel";

interface Props {
    name: string;
    value?: string | null;
    nameClass?: string;
    valueClass?: string;
    /** default true, like the web client: "Name: value" on one line */
    horizontal?: boolean;
    testID?: string;
    /** #append slot: after the value (e.g. an info button) */
    append?: React.ReactNode;
}

/**
 * Port of DisplayFieldComponent.vue: "Name: value" (horizontal, the default) or a label above the value.
 * An empty value shows "N/A".
 */
export function DisplayField({
    name,
    value,
    nameClass,
    valueClass,
    horizontal = true,
    testID,
    append,
}: Props) {
    if (horizontal)
        return append ? (
            <View className="flex-row flex-wrap items-center" testID={testID}>
                <AppText>
                    <AppText className={nameClass}>{name}: </AppText>
                    <AppText className={valueClass}>{value || "N/A"}</AppText>
                </AppText>
                {append}
            </View>
        ) : (
            <AppText testID={testID}>
                <AppText className={nameClass}>{name}: </AppText>
                <AppText className={valueClass}>{value || "N/A"}</AppText>
            </AppText>
        );
    return (
        <View testID={testID}>
            <FieldLabel title={name} className={nameClass} />
            <AppText className={cn(valueClass)}>{value || "N/A"}</AppText>
        </View>
    );
}
