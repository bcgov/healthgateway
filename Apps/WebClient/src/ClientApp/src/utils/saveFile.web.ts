/**
 * Web: the web client's `fetch(data:…)` → Blob → file-saver `saveAs`, done with an object URL and a temporary
 * `<a download>` (what file-saver does in current browsers).
 */
export async function saveBase64File(
    base64: string,
    mimeType: string,
    fileName: string
): Promise<void> {
    const response = await fetch(`data:${mimeType};base64,${base64}`);
    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    anchor.rel = "noopener";
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    // let the download start before releasing the blob
    setTimeout(() => URL.revokeObjectURL(url), 40_000);
}
