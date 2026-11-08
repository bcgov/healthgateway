const alphabet =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

/** base64 of a byte array (the PatientData file API sends bytes; files are saved from base64). */
export function bytesToBase64(bytes: ArrayLike<number>): string {
    let out = "";
    let i = 0;
    for (; i + 2 < bytes.length; i += 3) {
        const n = (bytes[i] << 16) | (bytes[i + 1] << 8) | bytes[i + 2];
        out +=
            alphabet[(n >> 18) & 63] +
            alphabet[(n >> 12) & 63] +
            alphabet[(n >> 6) & 63] +
            alphabet[n & 63];
    }
    const rest = bytes.length - i;
    if (rest === 1) {
        const n = bytes[i] << 16;
        out += alphabet[(n >> 18) & 63] + alphabet[(n >> 12) & 63] + "==";
    } else if (rest === 2) {
        const n = (bytes[i] << 16) | (bytes[i + 1] << 8);
        out +=
            alphabet[(n >> 18) & 63] +
            alphabet[(n >> 12) & 63] +
            alphabet[(n >> 6) & 63] +
            "=";
    }
    return out;
}
