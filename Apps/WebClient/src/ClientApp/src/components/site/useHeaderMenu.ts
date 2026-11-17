import { usePathname, useRouter } from "expo-router";
import { Platform } from "react-native";

import { appVersionText } from "@/components/common";
import { Path } from "@/constants/path";
import { Action, InternalUrl, Text, Type } from "@/constants/tracking";
import type {
    HeaderBell,
    HeaderMenu,
    HeaderMenuAction,
    HeaderMenuEntry,
    HeaderRefresh,
} from "@/navigation/headerMenu";
import { toHref } from "@/navigation/href";
import { allSupportLinks, followSupportLink } from "@/navigation/supportLinks";
import { isFullyRegisteredUser } from "@/navigation/userState";
import { services } from "@/services/container";
import { sessionGeneration } from "@/services/sessionFence";
import { useAuthStore } from "@/stores/auth.store";
import {
    selectFeatureToggles,
    selectIsOffline,
    useConfigStore,
} from "@/stores/config.store";
import {
    type HeaderMenuDialog,
    useHeaderMenuStore,
} from "@/stores/headerMenu.store";
import {
    NOTIFICATION_CENTRE_SWITCHED_ON,
    useNewNotifications,
    useNotificationStore,
} from "@/stores/notification.store";
import { usePageRefreshStore } from "@/stores/pageRefresh.store";
import { type ThemePreference, useThemeStore } from "@/stores/theme.store";
import {
    selectIsValidIdentityProvider,
    selectUserInitials,
    selectUserName,
    useUserStore,
} from "@/stores/user.store";

import { useSupportLinksShown } from "./useSupportLinksShown";

/** iOS cannot present a dialog while the menu is still going away. */
export const MENU_DISMISS_MS = Platform.OS === "ios" ? 400 : 0;

/** Dark Mode's choices, in the web menu's order (`DarkModeMenuItem`). */
const darkModeChoices: {
    value: ThemePreference;
    label: string;
    subtitle?: string;
}[] = [
    { value: "dark", label: "On" },
    { value: "light", label: "Off" },
    {
        value: "system",
        label: "System",
        subtitle:
            "We'll adjust your appearance based on your device's system settings",
    },
];

/**
 * Runs `action` once the menu has gone, unless the session ended meanwhile (logged out, or timed out): a dialog opened
 * then would wait for the next sign-in, and the rating's dismissal would log that session out.
 */
export function afterMenu(action: () => void) {
    const session = sessionGeneration();
    setTimeout(() => {
        if (
            session !== sessionGeneration() ||
            !useAuthStore.getState().isAuthenticated
        )
            return;
        action();
    }, MENU_DISMISS_MS);
}

const showDialogAfterMenu = (dialog: HeaderMenuDialog) =>
    afterMenu(() => useHeaderMenuStore.getState().showDialog(dialog));

/** The Snowplow event and route of Log Out, shared with the rating dialog that may come first. */
export function logOut(push: (path: string) => void) {
    services.tracking.trackEvent({
        action: Action.ButtonClick,
        text: Text.Logout,
        type: Type.Logout,
    });
    push(Path.Logout);
}

/** The entries both menus share. `overApp`: see `useSettingsMenu`. */
function useSharedEntries(overApp: boolean) {
    const router = useRouter();
    const preference = useThemeStore((s) => s.preference);
    const setPreference = useThemeStore((s) => s.setPreference);
    const supportShown = useSupportLinksShown();
    const push = (path: string) => router.push(toHref(path));

    const darkMode: HeaderMenuEntry = {
        type: "submenu",
        key: "menu-btn-dark-mode-link",
        label: "Dark Mode",
        glyph: "darkMode",
        items: darkModeChoices.map(({ value, label, subtitle }) => ({
            type: "action",
            key: `dark-mode-${label.toLowerCase()}`,
            label,
            subtitle,
            checked: preference === value,
            onPress: () => void setPreference(value),
        })),
    };
    const security: HeaderMenuAction[] = overApp
        ? []
        : [
              {
                  type: "action",
                  key: "menu-btn-security-link",
                  label: "Security",
                  glyph: "security",
                  onPress: () => push(Path.Security),
              },
          ];
    const support: HeaderMenuEntry[] = supportShown
        ? [
              {
                  type: "submenu",
                  key: "menu-btn-support-link",
                  label: "Support",
                  glyph: "support",
                  items: allSupportLinks
                      .filter((link) => !overApp || !link.to)
                      .map((link) => ({
                          type: "action",
                          key: link.testID,
                          label: link.text,
                          glyph: link.to
                              ? "document"
                              : link.action === Action.Email
                                ? "email"
                                : "external",
                          onPress: () => followSupportLink(link, push),
                      })),
              },
          ]
        : [];
    const version = appVersionText();
    const versionSection: HeaderMenuAction[] = version
        ? [
              {
                  type: "action",
                  key: "appVersion",
                  label: version,
                  disabled: true,
              },
          ]
        : [];
    return { push, darkMode, security, support, versionSection };
}

function settingsMenu({
    darkMode,
    security,
    support,
    versionSection,
}: ReturnType<typeof useSharedEntries>): HeaderMenu {
    return {
        kind: "settings",
        buttonLabel: "Settings",
        sections: [[darkMode, ...security, ...support], versionSection],
    };
}

/**
 * The signed-out settings menu (gear). `overApp`: for a screen that covers the app (the intro, the lock screen, Update
 * required: `BrandHeader`), so it leaves out what opens a page of the app, which would open behind the screen: Security
 * (also not to be reached past the lock) and Terms of Service.
 */
export function useSettingsMenu({
    overApp = false,
}: { overApp?: boolean } = {}): HeaderMenu {
    return settingsMenu(useSharedEntries(overApp));
}

/**
 * The navigation bar's menu, drawn by each platform's own menu. Signed in: the account menu, titled with the user's
 * name. Signed out: the settings menu. Both end with the app's version. Also the notification centre's bell, when it is
 * on (HeaderComponent.vue's conditions), and the page's pull to refresh as a button, on a page that has one.
 */
export function useHeaderMenu(): {
    menu: HeaderMenu;
    bell?: HeaderBell;
    refresh?: HeaderRefresh;
} {
    const pathname = usePathname();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isOffline = useConfigStore(selectIsOffline);
    const isNotificationCentreEnabled = useConfigStore(
        (s) => selectFeatureToggles(s)?.notificationCentre?.enabled ?? false
    );
    const isValidIdp = useUserStore(selectIsValidIdentityProvider);
    const patientRetrievalFailed = useUserStore(
        (s) => s.patientRetrievalFailed
    );
    const userName = useUserStore(selectUserName);
    const initials = useUserStore(selectUserInitials);
    // subscribe so the derived state re-evaluates when the user record changes
    useUserStore((s) => s.user);
    const bellSeen = useHeaderMenuStore((s) => s.bellSeen);
    const setBellSeen = useHeaderMenuStore((s) => s.setBellSeen);
    const setNotificationCenterOpen = useNotificationStore(
        (s) => s.setNotificationCenterOpen
    );
    const newNotifications = useNewNotifications();
    const pageRefresh = usePageRefreshStore((s) => s.refresh);
    const refreshing = usePageRefreshStore((s) => s.refreshing);
    const refresh: HeaderRefresh | undefined = pageRefresh
        ? { disabled: refreshing, onPress: pageRefresh }
        : undefined;
    const fullyRegistered = isFullyRegisteredUser();
    const shared = useSharedEntries(false);
    const { push, darkMode, security, support, versionSection } = shared;

    if (!isAuthenticated) return { menu: settingsMenu(shared), refresh };

    const showProfile = isValidIdp && !patientRetrievalFailed;
    const profile: HeaderMenuAction[] = showProfile
        ? [
              {
                  type: "action",
                  key: "profileBtn",
                  label: "Profile",
                  glyph: "profile",
                  onPress: () => {
                      services.tracking.trackEvent({
                          action: Action.ButtonClick,
                          text: Text.Profile,
                          type: Type.Profile,
                          url: InternalUrl.Profile,
                      });
                      if (pathname !== Path.Profile) push(Path.Profile);
                  },
              },
          ]
        : [];
    const feedback: HeaderMenuAction[] = fullyRegistered
        ? [
              {
                  type: "action",
                  key: "menu-btn-feedback-link",
                  label: "Feedback",
                  glyph: "feedback",
                  onPress: () => {
                      services.tracking.trackEvent({
                          action: Action.ButtonClick,
                          text: Text.SendFeedback,
                          type: Type.Sidebar,
                      });
                      showDialogAfterMenu("feedback");
                  },
              },
          ]
        : [];
    const logout: HeaderMenuAction = {
        type: "action",
        key: "logoutBtn",
        label: "Log Out",
        glyph: "logout",
        destructive: true,
        onPress: () => {
            if (isValidIdp) showDialogAfterMenu("rating");
            else logOut(push);
        },
    };

    const bellShown =
        NOTIFICATION_CENTRE_SWITCHED_ON &&
        isNotificationCentreEnabled &&
        !isOffline &&
        isValidIdp &&
        fullyRegistered;
    const badge = bellSeen ? 0 : newNotifications.length;

    return {
        menu: {
            kind: "account",
            // iOS and Android only (web's initials button names itself, Header.web.tsx): the same gear as signed out,
            // named the same (owner, 2026-11-16)
            buttonLabel: "Settings",
            initials,
            title: userName,
            sections: [
                [...profile, darkMode, ...security],
                [...support, ...feedback],
                [logout],
                versionSection,
            ],
        },
        bell: bellShown
            ? {
                  count: badge,
                  // the badge count is part of the name
                  accessibilityLabel:
                      badge > 0
                          ? `Notifications, ${badge} new`
                          : "Notifications",
                  onPress: () => {
                      setBellSeen(true);
                      setNotificationCenterOpen(true);
                  },
              }
            : undefined,
        refresh,
    };
}
