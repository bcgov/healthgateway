import { ENVIRONMENTS_URL } from "@/config/environments";

/** Start-up waits for the answer, so not for long: anything slower counts as no answer. */
export const RELEASED_BUILD_TIMEOUT_MS = 3000;

/**
 * The last build released to the public on both stores (`GET /api/mobileservice/environment`, Apps/MobileEnvironment),
 * or null when none is set, which means Production only. Throws on anything else, which the caller treats as no answer:
 * a failed or slow request, a status other than 200 (the gateway's 429 rate limit among them), or a body without a
 * whole number or null in `releasedBuild`.
 */
export async function fetchReleasedBuild(
    timeoutMs = RELEASED_BUILD_TIMEOUT_MS
): Promise<number | null> {
    const controller = new AbortController();
    const timer = setTimeout(() => controller.abort(), timeoutMs);
    try {
        const response = await fetch(ENVIRONMENTS_URL, {
            headers: { Accept: "application/json" },
            signal: controller.signal,
        });
        if (response.status !== 200)
            throw new Error(`The released build answered ${response.status}`);
        const body: unknown = await response.json();
        const value =
            typeof body === "object" && body !== null && "releasedBuild" in body
                ? body.releasedBuild
                : undefined;
        if (value === null) return null;
        if (typeof value === "number" && Number.isInteger(value) && value >= 0)
            return value;
        throw new Error("The released build's answer has no usable build");
    } finally {
        clearTimeout(timer);
    }
}
