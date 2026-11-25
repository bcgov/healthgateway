import { Platform } from "react-native";

export type MobileBrowserOs = "ios" | "android";

/**
 * On web, the phone or tablet OS the browser runs on, from its user agent; undefined on a desktop and in the native app
 * (where `Platform.OS` already says which it is).
 */
export function mobileBrowserOs(): MobileBrowserOs | undefined {
    if (Platform.OS !== "web" || typeof navigator === "undefined")
        return undefined;
    const ua = navigator.userAgent;
    if (/Android/i.test(ua)) return "android";
    if (/iPhone|iPad|iPod/i.test(ua)) return "ios";
    // iPadOS Safari asks for the desktop site by default, so it reports itself as a Mac; a Mac has no touch screen
    if (/Macintosh/i.test(ua) && navigator.maxTouchPoints > 1) return "ios";
    return undefined;
}
