/* eslint-disable @typescript-eslint/no-unsafe-assignment -- @expo/vector-icons does not type its props */
import FontAwesome6 from "@expo/vector-icons/FontAwesome6";
import type { ComponentProps } from "react";
import { View } from "react-native";

type Props = Omit<ComponentProps<typeof FontAwesome6>, "name"> & {
    name: string;
    /**
     * v-icon's box: the glyph `size` tall at its own width, centred in a box 1.25 × size wide (Font Awesome's
     * fixed width), as the web client draws a Font Awesome icon in a v-icon.
     */
    fixedWidth?: boolean;
};

/**
 * FontAwesome 6 icon, solid by default: the web client registers the `fas` set for all its icons. Decorative unless it
 * has an `accessibilityLabel`: the text or the labelled button beside it says what it means, so the screen reader
 * skips the glyph; with a label it is an image with that name.
 */
export function Icon({
    solid = true,
    fixedWidth,
    size = 12,
    accessibilityLabel,
    ...rest
}: Props) {
    const glyph = (
        <FontAwesome6
            solid={solid}
            size={size}
            {...rest}
            {...(accessibilityLabel
                ? { accessibilityRole: "image", accessibilityLabel }
                : // not an element of its own for VoiceOver (`accessible`), skipped by TalkBack (`importantForAccessibility`)
                  { accessible: false, importantForAccessibility: "no" })}
        />
    );
    if (!fixedWidth) return glyph;
    return (
        <View
            style={{
                width: size * 1.25,
                height: size,
                alignItems: "center",
                justifyContent: "center",
            }}
        >
            {glyph}
        </View>
    );
}
