/** Decode a JWT payload without verification (claims are only used for display/hdid lookup). */
export function decodeJwtPayload<T = Record<string, unknown>>(
    token: string | undefined
): T | undefined {
    if (!token) return undefined;
    const parts = token.split(".");
    if (parts.length < 2) return undefined;
    try {
        const base64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
        const padded = base64 + "=".repeat((4 - (base64.length % 4)) % 4);
        const json = decodeBase64(padded);
        return JSON.parse(json) as T;
    } catch {
        return undefined;
    }
}

/** Base64 to text: both paths give one character per byte, then the bytes are read as UTF-8 ("Zoë", not "ZoÃ«"). */
function decodeBase64(input: string): string {
    const binary =
        typeof atob === "function" ? atob(input) : decodeBase64Bytes(input);
    try {
        return decodeURIComponent(
            Array.from(
                binary,
                (c) => "%" + c.charCodeAt(0).toString(16).padStart(2, "0")
            ).join("")
        );
    } catch {
        return binary;
    }
}

/** For Hermes builds without `atob`. */
function decodeBase64Bytes(input: string): string {
    const chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    let out = "";
    let buffer = 0;
    let bits = 0;
    for (const ch of input.replace(/=+$/, "")) {
        buffer = (buffer << 6) | chars.indexOf(ch);
        bits += 6;
        if (bits >= 8) {
            bits -= 8;
            out += String.fromCharCode((buffer >> bits) & 0xff);
        }
    }
    return out;
}
