// the close function of the tooltip shown now, so only one is shown at a time and a modal can close it
let closeActive: (() => void) | null = null;

/**
 * Records a tooltip as the one shown, closing whichever tooltip was shown before it.
 * @param close Closes the tooltip being shown.
 */
export function activateInfoTooltip(close: () => void): void {
    closeActive?.();
    closeActive = close;
}

/**
 * Forgets a tooltip that has closed, if it is still the one recorded as shown.
 * @param close The close function it was recorded with.
 */
export function deactivateInfoTooltip(close: () => void): void {
    if (closeActive === close) closeActive = null;
}

/** Closes the tooltip shown now, if any. */
export function closeActiveInfoTooltip(): void {
    closeActive?.();
}
