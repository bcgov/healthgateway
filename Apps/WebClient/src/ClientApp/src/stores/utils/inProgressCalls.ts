/**
 * Stores know a fetch is running, but like the web client's they only skip one that finished, so a second request would
 * start the same call again and whichever answered last would win. This hands it the call already running instead.
 */
export function inProgressCalls() {
    const calls = new Map<string, Promise<unknown>>();
    return {
        /** No key: this call can't be shared with another (a protective word changes the answer). */
        share<R>(key: string | undefined, start: () => Promise<R>): Promise<R> {
            if (key === undefined) return start();
            const running = calls.get(key);
            if (running) return running as Promise<R>;
            const call = start().finally(() => {
                if (calls.get(key) === call) calls.delete(key);
            });
            calls.set(key, call);
            return call;
        },
        running: (key: string) => calls.get(key),
        /** Logged out: nothing started before that may answer for whoever comes next. */
        clear: () => calls.clear(),
    };
}
