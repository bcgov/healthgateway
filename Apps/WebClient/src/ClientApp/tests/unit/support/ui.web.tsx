import { jest } from "@jest/globals";
import {
    act,
    cleanup,
    fireEvent,
    render as domRender,
    renderHook as domRenderHook,
    type RenderResult,
    waitFor,
} from "@testing-library/react";
import type { ReactElement } from "react";
import { SafeAreaProvider } from "react-native-safe-area-context";

/** Web project twin of ui.ts: react-native-web renders DOM, queried with the DOM Testing Library. */
export type UiElement = HTMLElement;

let current: RenderResult | undefined;
const q = () => {
    if (!current) throw new Error("render() has not been called");
    return current;
};

const withProviders = (ui: ReactElement) => (
    <SafeAreaProvider
        initialMetrics={{
            frame: { x: 0, y: 0, width: 1366, height: 900 },
            insets: { top: 0, right: 0, bottom: 0, left: 0 },
        }}
    >
        {ui}
    </SafeAreaProvider>
);

/** See ui.tsx: let promise chains started by a render or event finish inside act(). */
export const settle = async () => {
    await act(async () => {
        for (let i = 0; i < 10; i++) await Promise.resolve();
    });
};

/** Run something that updates React outside the library's events (a gesture fired from a test) inside act(). */
export const inAct = async (fn: () => void) => {
    await act(async () => {
        fn();
        await Promise.resolve();
    });
};

export const render = async (ui: ReactElement) => {
    cleanup();
    current = domRender(withProviders(ui));
    await settle();
};
/** Unmount what render() mounted (before a test resets stores a mounted component would react to). */
export const unmount = async () => {
    cleanup();
    await settle();
};
export const screen = {
    getByText: (text: string | RegExp) => q().getByText(text),
    queryByText: (text: string | RegExp) => q().queryByText(text),
    getByTestId: (id: string) => q().getByTestId(id),
    queryByTestId: (id: string) => q().queryByTestId(id),
    getAllByTestId: (id: string | RegExp) => q().getAllByTestId(id),
    queryAllByTestId: (id: string | RegExp) => q().queryAllByTestId(id),
    getByRole: (role: string, name?: string | RegExp) =>
        q().getByRole(role, name ? { name } : undefined),
    getByLabelText: (label: string | RegExp) => q().getByLabelText(label),
    getAllByLabelText: (label: string | RegExp) => q().getAllByLabelText(label),
    queryByLabelText: (label: string | RegExp) => q().queryByLabelText(label),
};
/** Whether an element with this test id is mounted, shown or not (Cypress `exist`, not `be.visible`). */
export const existsByTestId = (id: string) => q().queryByTestId(id) !== null;
/** An element by test id even when it is hidden from the screen reader (aria-hidden does not hide it from the DOM). */
export const getHiddenByTestId = (id: string) => q().getByTestId(id);

export const press = async (el: UiElement) => {
    fireEvent.click(el);
    await settle();
};
export const changeText = async (el: UiElement, text: string) => {
    // HgTextField puts the test id on the wrapper on web; type into its input
    const input = el.matches("input, textarea")
        ? el
        : (el.querySelector("input, textarea") as HTMLElement);
    fireEvent.change(input, { target: { value: text } });
    await settle();
};
/** Switch state (the `<input type="checkbox" role="switch">` inside HgSwitch). */
export const isOn = (el: UiElement) =>
    (el as HTMLInputElement).checked === true;
/** Flip a switch as a user would. */
export const toggleSwitch = async (el: UiElement) => {
    fireEvent.click(el);
    await settle();
};
/** The HTML id a Cypress `#id` selector finds (react-native-web renders `nativeID` as `id`). */
export const idOf = (el: UiElement): string | undefined =>
    el.getAttribute("id") ?? undefined;
/** An HTML attribute (web only; native has none, so undefined). */
export const attrOf = (el: UiElement, name: string): string | undefined =>
    el.getAttribute(name) ?? undefined;
/** Whether the element casts a shadow (the port's v-card elevation: an inline `box-shadow` on web). */
export const hasShadow = (el: UiElement): boolean => el.style.boxShadow !== "";
/** Rendered text of an element and its descendants. */
export const textOf = (el: UiElement): string => el.textContent ?? "";
/** Every heading's level in document order: a check that the outline never skips a level (WCAG 1.3.1). */
export const headingLevels = (): number[] =>
    [...document.querySelectorAll("h1, h2, h3, h4, h5, h6")].map((h) =>
        Number(h.tagName[1])
    );
/** The heading level the element is exposed at (react-native-web renders a header as `<hN>`), else undefined. */
export const headingLevelOf = (el: UiElement): number | undefined => {
    const match = /^H([1-6])$/.exec(el.tagName);
    return match ? Number(match[1]) : undefined;
};
/** Whether the element contains an `<input>` (a wrapper, as Cypress `[data-testid=x] input` expects). */
export const containsInput = (el: UiElement) =>
    el.querySelector("input, textarea") !== null;
/** The current text of a text field (its wrapper carries the test id on web). */
export const inputValue = (el: UiElement): string => {
    const input = el.matches("input, textarea")
        ? el
        : el.querySelector("input, textarea");
    return (input as HTMLInputElement | null)?.value ?? "";
};
export const isDisabled = (el: UiElement) =>
    el.getAttribute("aria-disabled") === "true" ||
    (el as HTMLButtonElement).disabled === true;
export const isSelected = (el: UiElement) =>
    el.getAttribute("aria-selected") === "true";
export const isChecked = (el: UiElement) =>
    el.getAttribute("aria-checked") === "true";
export const renderHook = async <T,>(hook: () => T) => {
    cleanup();
    const r = domRenderHook(hook);
    await settle();
    return {
        result: r.result,
        rerender: async () => {
            await act(async () => r.rerender());
        },
        unmount: async () => {
            await act(async () => r.unmount());
        },
    };
};
/** Advance fake timers inside act() and let the promises they start settle. */
export const advanceTimers = async (ms: number) => {
    await act(async () => {
        jest.advanceTimersByTime(ms);
        await Promise.resolve();
    });
};
/** The HTML tag the element renders as (web only; undefined on native). */
export const tagOf = (el: UiElement): string | undefined =>
    el.tagName.toLowerCase();
/** A key pressed and released on the element, as a keyboard user does it (web only: native has no keyboard). */
export const pressKey = async (el: UiElement, key: string) => {
    fireEvent.keyDown(el, { key });
    fireEvent.keyUp(el, { key });
    await settle();
};
/**
 * Click the element and report whether the browser would still carry out its default action (follow an anchor):
 * false when a handler called `preventDefault()`. Native has no default action, so it presses and reports true.
 */
export const pressKeepsDefault = async (el: UiElement): Promise<boolean> => {
    const kept = fireEvent.click(el);
    await settle();
    return kept;
};
export { waitFor };

/** Whether an expandable entry (`aria-expanded`) is open. */
export const isExpanded = (el: UiElement) =>
    el.getAttribute("aria-expanded") === "true";
