import { Platform } from "react-native";

import { cn } from "@/utils/cn";
import { openExternal } from "@/utils/openExternal";

import { AppText } from "./AppText";

/** The id of the hidden "opens in a new tab" text every new-tab link is described by (web). */
export const NEW_TAB_HINT_ID = "new-tab-hint";

/**
 * Web: the one hidden element the new-tab links point their `aria-describedby` at (WCAG 3.2.5 advisory, owner's
 * choice). Mounted once in the root layout. A description rather than part of each link's text, so the visible link
 * text stays the link's whole text.
 */
export function NewTabHint() {
    if (Platform.OS !== "web") return null;
    return (
        <AppText nativeID={NEW_TAB_HINT_ID} className="sr-only">
            opens in a new tab
        </AppText>
    );
}

interface Props {
    href: string;
    children: React.ReactNode;
    /** analytics etc.; runs before the link opens */
    onPress?: () => void;
    testID?: string;
    /** web: follow the link in this window instead of a new tab */
    sameWindow?: boolean;
    className?: string;
}

/**
 * An inline `<a href target="_blank" rel="noopener">` from the web client. Nest it inside an AppText so it flows
 * with the sentence. Web: react-native-web renders Text with href as a real anchor, which opens the new tab
 * itself. Native: opens the URL in the browser.
 */
export function ExternalLink({
    href,
    children,
    onPress,
    testID,
    className,
    sameWindow = false,
}: Props) {
    const web = Platform.OS === "web";
    const anchor = web
        ? ({
              href,
              hrefAttrs: sameWindow
                  ? undefined
                  : { target: "_blank", rel: "noopener noreferrer" },
              // the screen reader hears "opens in a new tab" after the name
              "aria-describedby": sameWindow ? undefined : NEW_TAB_HINT_ID,
          } as object)
        : {};
    return (
        <AppText
            testID={testID}
            accessibilityRole="link"
            className={cn("text-link underline", className)}
            {...anchor}
            onPress={() => {
                onPress?.();
                if (!web) void openExternal(href);
            }}
        >
            {children}
        </AppText>
    );
}
