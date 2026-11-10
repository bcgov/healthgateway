import { View } from "react-native";

import { AppText } from "./AppText";
import { FieldLabel } from "./FieldLabel";

interface Props {
    label: string;
    value?: string | null;
    testID?: string;
    /** the label's heading level (see FieldLabel) */
    level?: 3 | 4;
}

/** LabelComponent + the value paragraph used on the profile page (ActiveUserProfileComponent.vue). */
export function LabelValue({ label, value, testID, level }: Props) {
    return (
        <View className="mb-4">
            <FieldLabel title={label} level={level} />
            <AppText testID={testID} className="tracking-normal">
                {value ?? ""}
            </AppText>
        </View>
    );
}
