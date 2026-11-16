import { Link, usePathname, useRouter } from "expo-router";
import { useState } from "react";
import { Pressable, useWindowDimensions, View } from "react-native";

import HgLogo from "@/assets/images/gov/hg-logo-rev.svg";
import HgLogoDark from "@/assets/images/gov/hg-logo-rev-dark.svg";
import { AppText, HgButton, Icon } from "@/components/common";
import { Path } from "@/constants/path";
import { Action, InternalUrl, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useColorScheme, useThemeColors } from "@/hooks/useTheme";
import { toHref } from "@/navigation/href";
import { isFullyRegisteredUser } from "@/navigation/userState";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import {
    selectFeatureToggles,
    selectIsOffline,
    useConfigStore,
} from "@/stores/config.store";
import { selectIsSidebarOpen, useLayoutStore } from "@/stores/layout.store";
import {
    NOTIFICATION_CENTRE_SWITCHED_ON,
    useNewNotifications,
    useNotificationStore,
} from "@/stores/notification.store";
import {
    selectIsValidIdentityProvider,
    selectUserInitials,
    useUserStore,
} from "@/stores/user.store";
import { decorativeProps } from "@/utils/decorative";

import { NotificationCentre } from "./NotificationCentre";
import { RatingDialog } from "./RatingDialog";
import { useLogoLink } from "./useLogoLink";
import { UserMenu } from "./UserMenu.web";

/**
 * Port of HeaderComponent.vue (v-app-bar). Web only: native shows each stack's navigation bar (`useNativeStackOptions`).
 */
export function Header() {
    const colors = useThemeColors();
    // Dark variant: the navy wordmark, crest text and mountains become pale blue (docs/theming.md).
    const Logo = useColorScheme() === "dark" ? HgLogoDark : HgLogo;
    const router = useRouter();
    const pathname = usePathname();
    const { isMobile } = useBreakpoint();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isOffline = useConfigStore(selectIsOffline);
    const isValidIdp = useUserStore(selectIsValidIdentityProvider);
    const initials = useUserStore(selectUserInitials);
    const patientRetrievalFailed = useUserStore(
        (s) => s.patientRetrievalFailed
    );
    // subscribe so the derived state re-evaluates when the user record changes
    useUserStore((s) => s.user);
    const toggleSidebar = useLayoutStore((s) => s.toggleSidebar);
    const sidebarOpen = useLayoutStore(selectIsSidebarOpen(isMobile));

    const { width } = useWindowDimensions();
    // v-app-bar logo link: max-width 240px with 8px padding each side → 224px image, own aspect ratio
    const logoWidth = Math.min(224, width - 180);
    const logoHeight = logoWidth / 3.1924;
    const [menuOpen, setMenuOpen] = useState(false);
    const [ratingOpen, setRatingOpen] = useState(false);
    const [notificationButtonClicked, setNotificationButtonClicked] =
        useState(false);
    const isNotificationCentreEnabled = useConfigStore(
        (s) => selectFeatureToggles(s)?.notificationCentre?.enabled ?? false
    );
    const setNotificationCenterOpen = useNotificationStore(
        (s) => s.setNotificationCenterOpen
    );
    const newNotifications = useNewNotifications();

    const fullyRegistered = isFullyRegisteredUser();
    const showSidebarButton = fullyRegistered;
    const showLogin =
        !isAuthenticated && !isOffline && pathname.toLowerCase() !== Path.Login;
    const showProfileLink =
        isAuthenticated && isValidIdp && !patientRetrievalFailed;
    const logo = useLogoLink();

    const accountButton = (
        <Pressable
            testID="headerDropdownBtn"
            accessibilityRole="button"
            accessibilityLabel="Account menu"
            aria-expanded={menuOpen}
            {...({ "aria-haspopup": "menu" } as object)}
            onPress={() => setMenuOpen(true)}
            className="mx-2 w-12 h-12 rounded-full bg-surface-hover items-center justify-center"
        >
            <AppText testID="profileButtonInitials" className="font-bold">
                {initials}
            </AppText>
        </Pressable>
    );

    function handleLogout() {
        setMenuOpen(false);
        if (isValidIdp) setRatingOpen(true);
        else processLogout();
    }

    function processLogout() {
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.Logout,
            type: Type.Logout,
        });
        router.push(Path.Logout);
    }

    return (
        <View className="bg-app-bar border-b-2 border-border-light flex-row items-center h-16">
            {showSidebarButton ? (
                <Pressable
                    testID="navbar-toggle-button"
                    accessibilityRole="button"
                    accessibilityLabel="Toggle navigation"
                    aria-expanded={sidebarOpen}
                    {...({ "aria-controls": "sidenavbar" } as object)}
                    onPress={() => toggleSidebar(isMobile)}
                    className="p-3 mr-1"
                >
                    <Icon name="bars" size={24} color={colors["bc-black"]} />
                </Pressable>
            ) : null}
            {/* the web client's <router-link>: a real link on web, so the keyboard can follow it; the press tracks
                and navigates itself */}
            <Link
                href={toHref(logo.destination)}
                asChild
                onPress={(event) => {
                    event.preventDefault();
                    logo.follow();
                }}
            >
                <Pressable
                    accessibilityRole="link"
                    accessibilityLabel="Go to Health Gateway home page"
                    className="px-2 h-[62px] justify-center overflow-hidden"
                >
                    <Logo
                        width={logoWidth}
                        height={logoHeight}
                        preserveAspectRatio="xMinYMid meet"
                        // the link's label names the logo
                        {...decorativeProps()}
                        style={{
                            width: logoWidth,
                            height: logoHeight,
                            flexShrink: 0,
                        }}
                    />
                </Pressable>
            </Link>
            <View className="flex-1" />
            {NOTIFICATION_CENTRE_SWITCHED_ON &&
            isNotificationCentreEnabled &&
            !isOffline &&
            isAuthenticated &&
            isValidIdp &&
            fullyRegistered ? (
                <>
                    <Pressable
                        testID="notification-centre-button"
                        accessibilityRole="button"
                        // the badge count is part of the name
                        accessibilityLabel={
                            newNotifications.length > 0 &&
                            !notificationButtonClicked
                                ? `Notifications, ${newNotifications.length} new`
                                : "Notifications"
                        }
                        onPress={() => {
                            setNotificationButtonClicked(true);
                            setNotificationCenterOpen(true);
                        }}
                        className="w-11 h-11 items-center justify-center"
                    >
                        <Icon name="bell" size={20} color={colors["bc-grey"]} />
                        {newNotifications.length > 0 &&
                        !notificationButtonClicked ? (
                            <View className="absolute top-1 right-1 min-w-4 h-4 px-1 rounded-full bg-error items-center justify-center">
                                <AppText className="text-on-error text-[10px] leading-4">
                                    {newNotifications.length}
                                </AppText>
                            </View>
                        ) : null}
                    </Pressable>
                    <NotificationCentre />
                </>
            ) : null}
            {isAuthenticated ? (
                <>
                    {accountButton}
                    <UserMenu
                        visible={menuOpen}
                        onClose={() => setMenuOpen(false)}
                        showProfile={showProfileLink}
                        onProfile={() => {
                            setMenuOpen(false);
                            services.tracking.trackEvent({
                                action: Action.ButtonClick,
                                text: Text.Profile,
                                type: Type.Profile,
                                url: InternalUrl.Profile,
                            });
                            if (pathname !== Path.Profile)
                                router.push(Path.Profile);
                        }}
                        onLogout={handleLogout}
                    />
                    <RatingDialog
                        visible={ratingOpen}
                        onClose={() => {
                            setRatingOpen(false);
                            processLogout();
                        }}
                    />
                </>
            ) : (
                <>
                    {showLogin ? (
                        <HgButton
                            testID="loginBtn"
                            text="Log in"
                            icon="right-to-bracket"
                            uppercase={false}
                            className="mr-2 self-center"
                            onPress={() => {
                                services.tracking.trackEvent({
                                    action: Action.ButtonClick,
                                    text: Text.Login,
                                    type: Type.Login,
                                    url: InternalUrl.Login,
                                });
                                router.push(Path.Login);
                            }}
                        />
                    ) : null}
                </>
            )}
        </View>
    );
}
