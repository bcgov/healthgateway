import { Pressable, ScrollView } from "react-native";

import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import type { TabsProps } from "./hgTabs.shared";

export { type HgTab, HgTabPanel } from "./hgTabs.shared";

/**
 * Port of v-tabs (color primary, uppercase labels with the web client's `.v-tab.text-uppercase` 1.25px
 * letter-spacing, 48px): a scrolling row with the selected tab underlined. Native; `HgTabs.web.tsx` renders real
 * `tab` buttons in a `tablist`.
 */
export function HgTabs({
    tabs,
    value,
    onChange,
    selectedSurface = false,
    className,
}: TabsProps) {
    return (
        <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            className={cn("grow-0", className)}
            accessibilityRole="tablist"
        >
            {tabs.map((tab) => {
                const selected = tab.value === value;
                return (
                    <Pressable
                        key={tab.value}
                        testID={tab.testID}
                        nativeID={tab.nativeID}
                        accessibilityRole="tab"
                        aria-selected={selected}
                        aria-disabled={!!tab.disabled}
                        disabled={tab.disabled}
                        onPress={() => {
                            tab.onPress?.();
                            onChange(tab.value);
                        }}
                        className={cn(
                            "min-h-[48px] min-w-[90px] px-4 items-center justify-center border-b-2",
                            selected ? "border-primary" : "border-transparent",
                            selected && selectedSurface && "bg-surface",
                            tab.disabled
                                ? "opacity-50"
                                : "hover:bg-surface-hover active:bg-surface-hover"
                        )}
                    >
                        <AppText
                            className={cn(
                                "font-sans text-[14px] leading-5 tracking-[1.25px] uppercase",
                                selected ? "text-primary" : "text-bc-black"
                            )}
                        >
                            {tab.title}
                        </AppText>
                    </Pressable>
                );
            })}
        </ScrollView>
    );
}
