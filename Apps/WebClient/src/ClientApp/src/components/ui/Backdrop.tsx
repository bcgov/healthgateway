import type { ReactNode } from "react";
import {
    Platform,
    Pressable,
    type StyleProp,
    View,
    type ViewStyle,
} from "react-native";

interface Props {
    /** a press on the scrim itself, outside whatever it holds (undefined: a persistent overlay) */
    onPress?: () => void;
    className?: string;
    style?: StyleProp<ViewStyle>;
    testID?: string;
    children?: ReactNode;
}

/**
 * The scrim behind a dialog, menu or popover: closes it when pressed. Web: a plain element, not a `Pressable`, so
 * it is not a focus stop (react-native-web gives every `Pressable` a `tabIndex`) and the Modal's focus trap lands
 * on the first real control instead of on a nameless box. Native: a `Pressable` the screen reader skips
 * (`accessible={false}` also keeps the children it holds reachable one by one). The content it holds must stop
 * the click (web) or claim the responder (native), as `Dialog` does, or a press inside counts as outside.
 */
export function Backdrop({
    onPress,
    className,
    style,
    testID,
    children,
}: Props) {
    if (Platform.OS === "web")
        return (
            <View
                testID={testID}
                className={className}
                style={style}
                {...{ onClick: onPress }}
            >
                {children}
            </View>
        );
    return (
        <Pressable
            testID={testID}
            accessible={false}
            onPress={onPress}
            className={className}
            style={style}
        >
            {children}
        </Pressable>
    );
}
