import { Path } from "@/constants/path";
import { Destination, InternalUrl, Text } from "@/constants/tracking";
import type { FeatureToggleConfiguration } from "@/models/configData";

export interface MenuItem {
    title: string;
    icon: string;
    path: string;
    testID: string;
    enabled?: (config: FeatureToggleConfiguration) => boolean;
}

/** Port of the SidebarComponent.vue list. */
export const menuItems: MenuItem[] = [
    {
        title: "Home",
        icon: "house",
        path: Path.Home,
        testID: "menu-btn-home-link",
    },
    {
        title: "Health Records",
        icon: "box-archive",
        path: Path.Timeline,
        testID: "menu-btn-health-records-link",
    },
    {
        title: "Dependents",
        icon: "user-group",
        path: Path.Dependents,
        testID: "menu-btn-dependents-link",
        enabled: (c) => c.dependents.enabled,
    },
    {
        title: "Services",
        icon: "hand-holding-medical",
        path: Path.Services,
        testID: "menu-btn-services-link",
        enabled: (c) => c.services.enabled,
    },
    {
        title: "Download",
        icon: "cloud-arrow-down",
        path: Path.Reports,
        testID: "menu-btn-reports-link",
    },
];

/** Items enabled by the feature toggles (SidebarComponent.vue v-if conditions). */
export function visibleMenuItems(
    toggles: FeatureToggleConfiguration | undefined
): MenuItem[] {
    if (!toggles) return [];
    return menuItems.filter((m) => m.enabled?.(toggles) ?? true);
}

/**
 * Route name of a menu item inside the `(tabs)` group, as the native tab triggers need it. Home is the
 * `(home)` stack group (Home, Profile, Other record sources share the tab bar); the others are plain routes.
 */
export const menuRouteName = (path: string) =>
    path === Path.Home ? "(home)" : path.replace(/^\//, "");

/** Snowplow events for main navigation (SidebarComponent.vue); Home has none. */
export const menuEvents: Record<
    string,
    { text: Text; destination: Destination; url: InternalUrl }
> = {
    [Path.Timeline]: {
        text: Text.HealthRecords,
        destination: Destination.HealthRecords,
        url: InternalUrl.Timeline,
    },
    [Path.Dependents]: {
        text: Text.Dependents,
        destination: Destination.Dependents,
        url: InternalUrl.Dependents,
    },
    [Path.Services]: {
        text: Text.Services,
        destination: Destination.Services,
        url: InternalUrl.Services,
    },
    [Path.Reports]: {
        text: Text.Download,
        destination: Destination.Download,
        url: InternalUrl.Reports,
    },
};
