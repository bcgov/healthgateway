import { Children, isValidElement, type ReactNode, useEffect } from "react";
import {
    AccessibilityInfo,
    Platform,
    type StyleProp,
    View,
    type ViewStyle,
} from "react-native";

import { cn } from "@/utils/cn";

import type { Politeness } from "./announce";
import { AppText } from "./AppText";

interface Props {
    /** "polite" (role `status`): said when the reader is idle. "assertive" (role `alert`): said at once. */
    politeness?: Politeness;
    /** for the screen reader only: off screen on web, a point on native */
    visuallyHidden?: boolean;
    /** what iOS says (it has no live regions); default: the text of the children */
    announce?: string;
    testID?: string;
    /** the HTML id (web), for `aria-describedby` from a field to its messages */
    nativeID?: string;
    className?: string;
    style?: StyleProp<ViewStyle>;
    children?: ReactNode;
}

/** The plain text inside a tree of elements, for an announcement. */
export function textOfChildren(children: ReactNode): string {
    return Children.toArray(children)
        .map((child) => {
            if (typeof child === "string" || typeof child === "number")
                return String(child);
            if (isValidElement<{ children?: ReactNode }>(child))
                return textOfChildren(child.props.children);
            return "";
        })
        .join("")
        .replace(/\s+/g, " ")
        .trim();
}

// native: one point, kept in the tree so Android still announces it
const hiddenNative: ViewStyle = {
    position: "absolute",
    width: 1,
    height: 1,
    overflow: "hidden",
    opacity: 0,
};

/**
 * Text for the screen reader only: off screen on web (`sr-only`), a point on native. With `headingLevel` it is a
 * heading, for an outline the visible page lacks (e.g. "Records" above the Health Records list).
 */
export function VisuallyHidden({
    children,
    headingLevel,
}: {
    children: ReactNode;
    headingLevel?: 1 | 2 | 3 | 4 | 5 | 6;
}) {
    const heading = headingLevel
        ? { accessibilityRole: "header" as const, "aria-level": headingLevel }
        : {};
    return Platform.OS === "web" ? (
        <AppText className="sr-only" {...heading}>
            {children}
        </AppText>
    ) : (
        <AppText style={hiddenNative} {...heading}>
            {children}
        </AppText>
    );
}

/**
 * A status message: what appears or changes inside it is announced by the screen reader without moving focus
 * (WCAG 4.1.3). Web: `role="status"` or `"alert"` with `aria-live`. Android: `accessibilityLiveRegion`. iOS has no
 * live regions, so the text is announced through `AccessibilityInfo` whenever it appears or changes. Use it for
 * messages that follow an action; anything already on the page when it loads needs no live region.
 */
export function LiveRegion({
    politeness = "polite",
    visuallyHidden = false,
    announce: spokenText,
    testID,
    nativeID,
    className,
    style,
    children,
}: Props) {
    const spoken = spokenText ?? textOfChildren(children);
    useEffect(() => {
        if (Platform.OS === "ios" && spoken)
            AccessibilityInfo.announceForAccessibility(spoken);
    }, [spoken]);
    const web = Platform.OS === "web";
    return (
        <View
            testID={testID}
            nativeID={nativeID}
            className={cn(web && visuallyHidden && "sr-only", className)}
            style={[!web && visuallyHidden ? hiddenNative : undefined, style]}
            accessibilityLiveRegion={
                Platform.OS === "android" ? politeness : undefined
            }
            {...(web
                ? ({
                      role: politeness === "assertive" ? "alert" : "status",
                      "aria-live": politeness,
                      // react-native-web 0.21 drops `aria-atomic` (a typo in createDOMProps); the legacy name reaches the DOM
                      accessibilityAtomic: true,
                  } as object)
                : {})}
        >
            {children}
        </View>
    );
}
