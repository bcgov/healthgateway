import { describeError, logger } from "@/services/logger";

/**
 * Copies text with expo-clipboard. Imported lazily: a dev client built before the package was added has no
 * native module, and a top-level import would crash every screen that imports this file.
 */
export async function copyToClipboard(text: string): Promise<boolean> {
    try {
        const Clipboard = await import("expo-clipboard");
        return await Clipboard.setStringAsync(text);
    } catch (error) {
        logger.error(
            `Unable to copy to the clipboard: ${describeError(error)}`
        );
        return false;
    }
}
