import { describeError, logger } from "@/services/logger";
import { sessionGeneration } from "@/services/sessionFence";

const EXPORT_FOLDER = "hg-exports";
const activeExports = new Set<string>();
let nextExport = 0;

/**
 * Native: write the file to the cache and preview it, as the legacy myhealthBC apps did: iOS's Quick Look (its share
 * button saves, prints or sends it), Android's app for the file type. Without a viewer (an Android with no app for
 * the type, or a dev client built before the viewer was added) it opens the system's save sheet instead ("Save to
 * Files" on iOS, "Save as" on Android). Cancelling the sheet is not an error. The packages are imported lazily so a dev
 * client built before them fails with an error instead of crashing every screen. Signing out part way through ends
 * the export: nothing is shown, and the file is deleted however far it got.
 */
export async function saveBase64File(
    base64: string,
    mimeType: string,
    fileName: string
): Promise<void> {
    // sign-out's clearSavedFiles leaves this export's folder alone while it runs, so the export clears up after itself
    const session = sessionGeneration();
    const ended = () => session !== sessionGeneration();
    const { Directory, File } = await import("expo-file-system");
    // These are health records in the clear: each export has its own folder. Other exports may still be being written,
    // so only folders nobody is using are cleared here, a preview's from before included.
    const parent = await clearExports();
    nextExport += 1;
    const folder = new Directory(parent, String(nextExport));
    activeExports.add(folder.uri);
    folder.create();
    // The viewer returns once it is shown, not when it is closed: a previewed file stays until the next export, sign-out
    // or start-up (clearSavedFiles).
    let previewed = false;
    try {
        const file = new File(folder, fileName);
        // asynchronous since expo-file-system 58: saving before it finishes finds no file
        await file.write(base64, { encoding: "base64" });
        // signed out while it was written: no viewer or save sheet over the signed-out app
        if (ended()) return;
        previewed = await preview(file.uri, mimeType, fileName);
        if (!previewed && !ended()) await save(file.uri, mimeType, fileName);
    } finally {
        activeExports.delete(folder.uri);
        // a preview from a session that has ended is not kept for the next export or start-up to clear
        if ((!previewed || ended()) && folder.exists) folder.delete();
    }
}

/**
 * Removes the files earlier exports left for their preview. Called at sign-out and start-up; a failure is logged, as
 * the next export clears them too.
 */
export async function clearSavedFiles(): Promise<void> {
    try {
        await clearExports();
    } catch (error) {
        logger.warn(`Could not clear exported files: ${describeError(error)}`);
    }
}

/** The exports folder, emptied of every export not being written or saved right now. */
async function clearExports() {
    const { Directory, Paths } = await import("expo-file-system");
    const parent = new Directory(Paths.cache, EXPORT_FOLDER);
    if (parent.exists) {
        for (const entry of parent.list())
            if (!activeExports.has(entry.uri)) entry.delete();
    } else parent.create();
    return parent;
}

/** Shows the file in the platform's viewer. False when there is none for it, so the caller saves it instead. */
async function preview(
    uri: string,
    mimeType: string,
    fileName: string
): Promise<boolean> {
    let viewer: typeof import("@react-native-documents/viewer");
    try {
        viewer = await import("@react-native-documents/viewer");
    } catch (error) {
        // a dev client built before the viewer was added has no native module for it
        logger.warn(`No document viewer: ${describeError(error)}`);
        return false;
    }
    try {
        await viewer.viewDocument({
            uri,
            mimeType,
            headerTitle: fileName,
            // the legacy iOS app's preview filled the screen too
            presentationStyle: "fullScreen",
        });
        return true;
    } catch (error) {
        if (
            viewer.isErrorWithCode(error) &&
            error.code === viewer.errorCodes.UNABLE_TO_OPEN_FILE_TYPE
        )
            return false;
        throw error;
    }
}

/** The system's save sheet. Cancelling it is not an error. */
async function save(uri: string, mimeType: string, fileName: string) {
    const { errorCodes, isErrorWithCode, saveDocuments } =
        await import("@react-native-documents/picker");
    try {
        await saveDocuments({ sourceUris: [uri], mimeType, fileName });
    } catch (error) {
        if (
            isErrorWithCode(error) &&
            error.code === errorCodes.OPERATION_CANCELED
        )
            return;
        throw error;
    }
}
