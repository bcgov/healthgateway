import { useId, useRef, useState } from "react";

import { useThemeColors } from "@/hooks/useTheme";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import type { TabsProps } from "./hgTabs.shared";

export { type HgTab, HgTabPanel } from "./hgTabs.shared";

/**
 * Port of v-tabs on web: real `<button role="tab">` elements in a `tablist` (Cypress asserts `be.disabled` and finds
 * them with `cy.contains("button", …)`), with the WAI-ARIA tabs keyboard: the selected tab is the one tab stop, and
 * Left / Right move to and select the neighbouring tab. The boxes are styled inline from the theme tokens: NativeWind
 * classes do not reach raw DOM elements (the label inside is a Text and keeps its classes).
 */
export function HgTabs({
    tabs,
    value,
    onChange,
    selectedSurface = false,
    className,
}: TabsProps) {
    const id = useId();
    const colors = useThemeColors();
    const buttons = useRef(new Map<number, HTMLButtonElement | null>());
    const [hovered, setHovered] = useState<number>();
    const select = (tab: TabsProps["tabs"][number]) => {
        tab.onPress?.();
        onChange(tab.value);
    };
    const move = (from: number, step: 1 | -1) => {
        const enabled = tabs.filter((t) => !t.disabled);
        const at = enabled.findIndex((t) => t.value === from);
        const next = enabled[(at + step + enabled.length) % enabled.length];
        if (!next) return;
        select(next);
        buttons.current.get(next.value)?.focus();
    };
    return (
        <div
            role="tablist"
            className={className}
            style={{ display: "flex", flexDirection: "row", overflowX: "auto" }}
        >
            {tabs.map((tab) => {
                const selected = tab.value === value;
                const background =
                    selected && selectedSurface
                        ? colors.surface
                        : !tab.disabled && hovered === tab.value
                          ? colors["surface-hover"]
                          : "transparent";
                return (
                    <button
                        key={tab.value}
                        type="button"
                        role="tab"
                        id={tab.nativeID ?? `${id}-tab-${tab.value}`}
                        data-testid={tab.testID}
                        aria-selected={selected}
                        aria-controls={tab.panelID}
                        aria-disabled={!!tab.disabled}
                        disabled={tab.disabled}
                        tabIndex={selected ? 0 : -1}
                        ref={(node) => {
                            buttons.current.set(tab.value, node);
                        }}
                        onClick={() => select(tab)}
                        onKeyDown={(event) => {
                            if (event.key === "ArrowRight") {
                                event.preventDefault();
                                move(tab.value, 1);
                            } else if (event.key === "ArrowLeft") {
                                event.preventDefault();
                                move(tab.value, -1);
                            }
                        }}
                        onMouseEnter={() => setHovered(tab.value)}
                        onMouseLeave={() =>
                            setHovered((h) => (h === tab.value ? undefined : h))
                        }
                        // v-tab: 48px tall, 90px wide at least, 16px sides, the selected one underlined 2px in primary
                        style={{
                            display: "flex",
                            flexDirection: "column",
                            alignItems: "center",
                            justifyContent: "center",
                            minHeight: 48,
                            minWidth: 90,
                            padding: "0 16px",
                            margin: 0,
                            font: "inherit",
                            background,
                            borderStyle: "solid",
                            borderWidth: 0,
                            borderBottomWidth: 2,
                            borderBottomColor: selected
                                ? colors.primary
                                : "transparent",
                            opacity: tab.disabled ? 0.5 : 1,
                            cursor: tab.disabled ? "default" : "pointer",
                        }}
                    >
                        <AppText
                            className={cn(
                                "font-sans text-[14px] leading-5 tracking-[1.25px] uppercase",
                                selected ? "text-primary" : "text-bc-black"
                            )}
                        >
                            {tab.title}
                        </AppText>
                    </button>
                );
            })}
        </div>
    );
}
