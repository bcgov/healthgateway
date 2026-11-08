/**
 * Why a token request failed, decided here once rather than guessed at each caller.
 * - refused: Keycloak said the login is over.
 * - connection: no answer came back at all, which says nothing about the login.
 * - unexpected: anything else, such as another OAuth error or a response that was not what it should be.
 */
export type AuthFailure = "refused" | "connection" | "unexpected";

/** What fetch rejects with when no answer came back, across React Native and browsers. */
const NO_ANSWER =
    /^(network request failed|failed to fetch|fetch failed|load failed|network ?error)/i;

export function classifyAuthFailure(error: unknown): AuthFailure {
    const { code } = (error ?? {}) as { code?: unknown };
    if (code === "invalid_grant") return "refused";
    if (code !== undefined) return "unexpected";
    return error instanceof TypeError && NO_ANSWER.test(error.message)
        ? "connection"
        : "unexpected";
}
