import { getAccessLinks } from "@/constants/accessLinks";

export type BubbleTail = "bottom" | "left" | "right";

/** Where a screen's icon bubble sits against the phone: its centre's offset from the phone's, and the side its tail is on. */
export interface BubblePlacement {
    x: number;
    y: number;
    tail: BubbleTail;
}

export interface OnboardingScreen {
    type: string;
    /** Raise it when a screen changes: users who saw an older version are shown it again, marked NEW. */
    version: number;
    title: string;
    description: string;
    /** the matching main navigation icon */
    icon: string;
    bubble: BubblePlacement;
}

/** The landing page's "What you can access" tile types, with the icon of the navigation entry each describes. */
const visuals: Record<string, { icon: string; bubble: BubblePlacement }> = {
    HealthRecords: {
        icon: "box-archive",
        bubble: { x: -45, y: -40, tail: "bottom" },
    },
    DependentRecords: {
        icon: "user-group",
        bubble: { x: 64, y: -10, tail: "left" },
    },
    RecordsManagement: {
        icon: "cloud-arrow-down",
        bubble: { x: -60, y: 48, tail: "right" },
    },
    HealthServices: {
        icon: "hand-holding-medical",
        bubble: { x: 64, y: 48, tail: "left" },
    },
};

/** The intro: one screen per landing page tile, so its text follows them (`constants/accessLinks.ts`). */
export const onboardingScreens: OnboardingScreen[] = getAccessLinks()
    .filter((tile) => visuals[tile.type])
    .map((tile) => ({
        type: tile.type,
        version: 1,
        title: tile.name,
        description: tile.description,
        ...visuals[tile.type],
    }));
