import { HttpError, SessionEndedError } from "@/models/errors";
import {
    endSessionGeneration,
    sessionGeneration,
} from "@/services/sessionFence";
import {
    selectCanFetch,
    useConnectivityStore,
} from "@/stores/connectivity.store";

export interface IHttpClient {
    setAuthorizationHeader(accessToken: string): void;
    unsetAuthorizationHeader(): void;
    /** Signing out: cancels the calls made with this login. They end with a `SessionEndedError`, and no answer that still arrives is used. */
    endSession(): void;
    get<T>(url: string, headers?: Record<string, string>): Promise<T>;
    /** Needs no login, so it still goes out while a login is on hold, and carries no token. */
    getPublic<T>(url: string): Promise<T>;
    post<T>(
        url: string,
        body?: unknown,
        headers?: Record<string, string>
    ): Promise<T>;
    put<T>(
        url: string,
        body?: unknown,
        headers?: Record<string, string>
    ): Promise<T>;
    delete<T>(
        url: string,
        body?: unknown,
        headers?: Record<string, string>
    ): Promise<T>;
}

type Method = "GET" | "POST" | "PUT" | "DELETE";

/**
 * fetch-based HTTP client. Mirrors the responsibilities of the web client's HttpDelegate:
 * JSON (de)serialisation, bearer token injection, and translation of non-2xx responses to HttpError.
 */
export class HttpClient implements IHttpClient {
    private authorization?: string;
    private readonly running = new Set<AbortController>();

    setAuthorizationHeader(accessToken: string) {
        this.authorization = `Bearer ${accessToken}`;
    }

    unsetAuthorizationHeader() {
        this.authorization = undefined;
    }

    endSession() {
        endSessionGeneration();
        this.running.forEach((controller) => controller.abort());
        this.running.clear();
    }

    get<T>(url: string, headers?: Record<string, string>) {
        return this.request<T>("GET", url, undefined, headers);
    }
    getPublic<T>(url: string) {
        return this.request<T>("GET", url, undefined, undefined, true);
    }
    post<T>(url: string, body?: unknown, headers?: Record<string, string>) {
        return this.request<T>("POST", url, body, headers);
    }
    put<T>(url: string, body?: unknown, headers?: Record<string, string>) {
        return this.request<T>("PUT", url, body, headers);
    }
    delete<T>(url: string, body?: unknown, headers?: Record<string, string>) {
        return this.request<T>("DELETE", url, body, headers);
    }

    private async request<T>(
        method: Method,
        url: string,
        body?: unknown,
        extraHeaders?: Record<string, string>,
        isPublic = false
    ): Promise<T> {
        // offline, or a login on hold: fail at once, the banner already says why
        const connectivity = useConnectivityStore.getState();
        if (isPublic ? !connectivity.isOnline : !selectCanFetch(connectivity))
            throw new HttpError("Network request failed");
        const headers: Record<string, string> = {
            Accept: "application/json",
            ...extraHeaders,
        };
        let payload: string | undefined;
        if (body !== undefined) {
            // Callers may pass a pre-serialised JSON string (e.g. JSON.stringify(email)) like the web client does.
            payload = typeof body === "string" ? body : JSON.stringify(body);
            headers["Content-Type"] ??= "application/json; charset=utf-8";
        }
        if (this.authorization && !isPublic)
            headers.Authorization = this.authorization;

        const session = sessionGeneration();
        const ended = () => !isPublic && session !== sessionGeneration();
        const controller = new AbortController();
        if (!isPublic) this.running.add(controller);

        let response: Response;
        try {
            response = await fetch(url, {
                method,
                headers,
                body: payload,
                signal: controller.signal,
            });
        } catch (error) {
            this.running.delete(controller);
            if (ended()) throw new SessionEndedError();
            // no answer at all, as opposed to an error response from a service that is up
            useConnectivityStore.getState().setServiceUnavailable(true);
            throw new HttpError(
                error instanceof Error
                    ? error.message
                    : "Network request failed"
            );
        }

        this.running.delete(controller);
        if (ended()) throw new SessionEndedError();
        useConnectivityStore.getState().setServiceUnavailable(false);

        if (!response.ok) {
            let message = response.statusText || `HTTP ${response.status}`;
            let traceId: string | undefined;
            try {
                const problem = (await response.json()) as Record<
                    string,
                    unknown
                >;
                message =
                    (problem.detail as string) ??
                    (problem.title as string) ??
                    (problem.message as string) ??
                    message;
                traceId = problem.traceId as string | undefined;
            } catch {
                // body was not JSON
            }
            if (ended()) throw new SessionEndedError();
            throw new HttpError(message, response.status, traceId);
        }

        if (response.status === 204) return undefined as T;
        const text = await response.text();
        if (ended()) throw new SessionEndedError();
        return (text ? JSON.parse(text) : undefined) as T;
    }
}
