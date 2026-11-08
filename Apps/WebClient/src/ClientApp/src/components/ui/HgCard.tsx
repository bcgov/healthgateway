import { Pressable, View, type ViewProps } from "react-native";

import { cn } from "@/utils/cn";

interface Props extends ViewProps {
    onPress?: () => void;
    className?: string;
}

/**
 * Outlined card used on the landing page (v-card variant="outlined").
 * When pressable it stays a plain container (not a button role) so it can hold buttons,
 * exactly like the clickable v-card on the web client; on web it shades on hover.
 */
export function HgCard({ onPress, className = "", children, ...rest }: Props) {
    const classes = cn(
        "rounded-md border border-border-light bg-surface p-5",
        className
    );
    if (onPress) {
        return (
            <Pressable
                accessible={false}
                // a press target for the mouse and touch; the buttons inside are the controls, so no tab stop
                tabIndex={-1}
                onPress={onPress}
                {...rest}
                className={cn(
                    classes,
                    "web:cursor-pointer web:transition-colors hover:bg-surface-hover active:bg-surface-hover"
                )}
            >
                {children}
            </Pressable>
        );
    }
    return (
        <View {...rest} className={classes}>
            {children}
        </View>
    );
}
