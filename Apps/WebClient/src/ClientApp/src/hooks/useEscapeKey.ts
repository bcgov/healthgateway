/**
 * Native: nothing to do. The web client's useEscapeKey closes the temporary navigation drawer, which native replaces
 * with the tab bar, and native modals already close through onRequestClose (Android back). Web: `useEscapeKey.web.ts`.
 */
export function useEscapeKey(_isOpen: boolean, _onEscape: () => void): void {
    // intentionally empty
}
