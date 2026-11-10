import { useState } from "react";
import { Platform, View } from "react-native";

export interface HgTab {
    /** the v-tab value (a position in its v-window) */
    value: number;
    title: string;
    disabled?: boolean;
    testID?: string;
    nativeID?: string;
    /** the HTML id of the panel this tab controls (web `aria-controls`); give the panel the same `nativeID` */
    panelID?: string;
    onPress?: () => void;
}

export interface TabsProps {
    tabs: HgTab[];
    value: number;
    onChange: (value: number) => void;
    /** `selected-class="bg-white"`: the selected tab sits on the card's surface colour */
    selectedSurface?: boolean;
    className?: string;
}

interface PanelProps {
    active: boolean;
    testID?: string;
    /** the HTML id a tab's `panelID` points at */
    nativeID?: string;
    /** the HTML id of the tab that labels this panel (web `aria-labelledby`) */
    tabID?: string;
    className?: string;
    children: React.ReactNode;
}

/**
 * Port of v-window-item: the element always exists (Cypress asserts `not.be.visible` on a closed one), its content
 * mounts the first time it is shown and stays mounted afterwards. On web it is a `tabpanel`.
 */
export function HgTabPanel({
    active,
    testID,
    nativeID,
    tabID,
    className,
    children,
}: PanelProps) {
    const [seen, setSeen] = useState(active);
    if (active && !seen) setSeen(true);
    return (
        <View
            testID={testID}
            nativeID={nativeID}
            className={className}
            style={active ? undefined : { display: "none" }}
            {...(Platform.OS === "web"
                ? ({ role: "tabpanel", "aria-labelledby": tabID } as object)
                : {})}
        >
            {seen ? children : null}
        </View>
    );
}
