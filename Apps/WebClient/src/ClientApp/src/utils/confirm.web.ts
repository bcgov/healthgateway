/** Web: the browser's confirm(), as the web client uses (Cypress answers it through `window:confirm`). */
export function confirmAction(message: string): Promise<boolean> {
    return Promise.resolve(globalThis.confirm(message));
}
