import { jest } from "@jest/globals";
import {
    act,
    cleanup,
    fireEvent,
    render as rnRender,
    renderHook as rnRenderHook,
    type RenderResult,
    waitFor,
} from "@testing-library/react-native";
import type { ReactElement } from "react";
import { StyleSheet } from "react-native";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import { SafeAreaProvider } from "react-native-safe-area-context";

/**
 * One API for component tests on both Jest projects: this file drives React Native Testing Library on the
 * native project; `ui.web.ts` drives the DOM Testing Library on the web project (react-native-web renders DOM).
 * Keep tests to the queries both share: text, test id, role, label.
 */
export type UiElement = ReturnType<RenderResult["getByTestId"]>;

// The library's global `screen` does not rebind reliably across renders; keep the latest result here.
let current: RenderResult | undefined;
const q = () => {
    if (!current) throw new Error("render() has not been called");
    return current;
};

/**
 * Components under test may read safe-area insets; a provider with fixed metrics stands in for the app root, as does
 * the gesture handler root view.
 */
const withProviders = (ui: ReactElement) => (
    <GestureHandlerRootView>
        <SafeAreaProvider
            initialMetrics={{
                frame: { x: 0, y: 0, width: 390, height: 844 },
                insets: { top: 47, right: 0, bottom: 34, left: 0 },
            }}
        >
            {ui}
        </SafeAreaProvider>
    </GestureHandlerRootView>
);

/**
 * Let promise chains started by the last render or event (mocked service calls resolving, then setState)
 * finish inside act(), so React does not warn that an update "was not wrapped in act".
 */
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
    // unmount any earlier render first; the library's own after-each cleanup is not enough for `screen`
    // to rebind when a test renders more than once or files mix globals
    await cleanup();
    current = await rnRender(withProviders(ui));
    await settle();
};
/** Unmount what render() mounted (before a test resets stores a mounted component would react to). */
export const unmount = async () => {
    await cleanup();
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
export const existsByTestId = (id: string) =>
    q().queryByTestId(id, { includeHiddenElements: true }) !== null;
/** An element by test id even when it is hidden from the screen reader (a decorative icon, a closed drawer). */
export const getHiddenByTestId = (id: string) =>
    q().getByTestId(id, { includeHiddenElements: true });

export const press = async (el: UiElement) => {
    await fireEvent.press(el);
    await settle();
};
export const changeText = async (el: UiElement, text: string) => {
    await fireEvent.changeText(el, text);
    await settle();
};
/** Switch state (RN Switch: `value`). */
export const isOn = (el: UiElement) => el.props.value === true;
/** Flip a switch as a user would. */
export const toggleSwitch = async (el: UiElement) => {
    await fireEvent(el, "valueChange", !el.props.value);
    await settle();
};
/** The HTML id a Cypress `#id` selector finds (RN `nativeID`). */
export const idOf = (el: UiElement) => el.props.nativeID as string | undefined;
/** An HTML attribute (web only; native has none, so undefined). */
export const attrOf = (_el: UiElement, _name: string): string | undefined =>
    undefined;
/** Every heading's level in tree order: a check that the outline never skips a level (WCAG 1.3.1). */
export const headingLevels = (): number[] =>
    q()
        .getAllByRole("header", { includeHiddenElements: true })
        .map((h) => Number(h.props["aria-level"] ?? 1));
/** The heading level a header role is exposed at (`aria-level`), undefined when it is not a header. */
export const headingLevelOf = (el: UiElement): number | undefined =>
    el.props.accessibilityRole === "header" || el.props.role === "heading"
        ? (el.props["aria-level"] as number | undefined)
        : undefined;
/** Rendered text of an element and its descendants. */
/** Whether the element casts a shadow (the port's v-card elevation: `shadow*` / Android `elevation` on native). */
export const hasShadow = (el: UiElement): boolean => {
    const style = (StyleSheet.flatten(el.props.style as never) ?? {}) as {
        shadowOpacity?: number;
        elevation?: number;
    };
    return (style.shadowOpacity ?? 0) > 0 && (style.elevation ?? 0) > 0;
};
export const textOf = (el: UiElement): string => {
    const walk = (node: unknown): string =>
        typeof node === "string" || typeof node === "number"
            ? String(node)
            : Array.isArray(node)
              ? node.map(walk).join("")
              : node && typeof node === "object" && "props" in node
                ? walk(
                      (node as { props: { children?: unknown } }).props.children
                  )
                : "";
    return walk(el.props.children);
};
/** Whether the element (or, on web, its subtree) contains a native input. */
export const containsInput = (el: UiElement) =>
    typeof el.props.onChangeText === "function";
/** The current text of a text field (the input itself carries the test id on native). */
export const inputValue = (el: UiElement): string =>
    String(el.props.value ?? "");
const accessibilityState = (el: UiElement) =>
    el.props.accessibilityState as
        | { disabled?: boolean; selected?: boolean; checked?: boolean }
        | undefined;
export const isDisabled = (el: UiElement) =>
    el.props.disabled === true ||
    accessibilityState(el)?.disabled === true ||
    el.props["aria-disabled"] === true;
export const isSelected = (el: UiElement) =>
    accessibilityState(el)?.selected === true ||
    el.props["aria-selected"] === true;
export const isChecked = (el: UiElement) =>
    accessibilityState(el)?.checked === true ||
    el.props["aria-checked"] === true;
export const renderHook = async <T,>(hook: () => T) => {
    await cleanup();
    const r = await rnRenderHook(hook);
    await settle();
    return {
        result: r.result as { current: T },
        rerender: async () => {
            await r.rerender(undefined);
            await settle();
        },
        unmount: async () => {
            await r.unmount();
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
export const tagOf = (_el: UiElement): string | undefined => undefined;
/** A key pressed and released on the element (web only: native has no keyboard, so nothing happens). */
export const pressKey = async (_el: UiElement, _key: string) => {
    await settle();
};
/** Web: click and report whether the browser would still follow the element; native has no default, so true. */
export const pressKeepsDefault = async (el: UiElement): Promise<boolean> => {
    await press(el);
    return true;
};
export { waitFor };

/** Whether an expandable entry (`aria-expanded`) is open. */
export const isExpanded = (el: UiElement) =>
    el.props["aria-expanded"] === true ||
    (el.props.accessibilityState as { expanded?: boolean } | undefined)
        ?.expanded === true;
