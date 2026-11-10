const EXPORT_FOLDER = "hg-exports";
const activeExports = new Set<string>();
let nextExport = 0;

/**
 * Native: write the file to the cache and open the system's save sheet ("Save to Files" on iOS, "Save as" on
 * Android), the platform's equivalent of the web client's download. Cancelling the sheet is not an error. Imported
 * lazily so a dev client built before these packages were added fails with an error instead of crashing every
 * screen.
 */
export async function saveBase64File(
    base64: string,
    mimeType: string,
    fileName: string
): Promise<void> {
    const { Directory, File, Paths } = await import("expo-file-system");
    const { errorCodes, isErrorWithCode, saveDocuments } =
        await import("@react-native-documents/picker");
    // These are health records in the clear: each export has its own folder, removed when its sheet is done. Other
    // exports may still be open, so only folders nobody is using, left by an app that died, are cleared here.
    const parent = new Directory(Paths.cache, EXPORT_FOLDER);
    if (parent.exists) {
        for (const entry of parent.list())
            if (!activeExports.has(entry.uri)) entry.delete();
    } else parent.create();
    nextExport += 1;
    const folder = new Directory(parent, String(nextExport));
    activeExports.add(folder.uri);
    folder.create();
    try {
        const file = new File(folder, fileName);
        // asynchronous since expo-file-system 58: saving before it finishes finds no file
        await file.write(base64, { encoding: "base64" });
        await saveDocuments({ sourceUris: [file.uri], mimeType, fileName });
    } catch (error) {
        if (
            isErrorWithCode(error) &&
            error.code === errorCodes.OPERATION_CANCELED
        )
            return;
        throw error;
    } finally {
        activeExports.delete(folder.uri);
        if (folder.exists) folder.delete();
    }
}
