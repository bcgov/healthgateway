import { Pressable, View, type ViewProps } from "react-native";

import { usePressHighlight } from "@/hooks/usePressHighlight";
import { cn } from "@/utils/cn";

interface Props extends ViewProps {
    onPress?: () => void;
    className?: string;
}

/**
 * Outlined card used on the landing page (v-card variant="outlined").
 * When pressable it stays a plain container (not a button role) so it can hold buttons,
 * exactly like the clickable v-card on the web client; it shades on hover and on a press.
 */
export function HgCard({ onPress, className, ...rest }: Props) {
    return onPress ? (
        <PressableCard {...rest} onPress={onPress} className={className} />
    ) : (
        <View {...rest} className={cardClasses(className)} />
    );
}

function cardClasses(className = "") {
    return cn(
        "rounded-md border border-border-light bg-surface p-5",
        className
    );
}

function PressableCard({
    onPress,
    className,
    children,
    ...rest
}: Props & { onPress: () => void }) {
    const { highlighted, pressed, handlers } = usePressHighlight();
    return (
        <Pressable
            accessible={false}
            // a press target for the mouse and touch; the buttons inside are the controls, so no tab stop
            tabIndex={-1}
            onPress={onPress}
            {...handlers}
            {...rest}
            className={cn(
                cardClasses(className),
                "web:cursor-pointer",
                // a press shades at once; the hover, and the end of a press, fade
                !pressed && "web:transition-colors",
                highlighted && "bg-surface-hover"
            )}
        >
            {children}
        </Pressable>
    );
}
