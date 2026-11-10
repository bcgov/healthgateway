import { useIsFocused } from "expo-router";
import type { ReactNode } from "react";
import { useState } from "react";
import { Platform } from "react-native";

/**
 * Native tabs render every tab at once, so each page would fetch its data, and show its loading overlay over the current
 * page, right after login. This holds a tab's content until it is first opened, as the web client's routes do; after
 * that it stays mounted, keeping its state. Web only ever mounts the current route.
 */
export function LazyTab({ children }: { children: ReactNode }) {
    const focused = useIsFocused();
    const [opened, setOpened] = useState(focused);
    if (focused && !opened) setOpened(true);
    if (Platform.OS === "web" || opened || focused) return children;
    return null;
}
