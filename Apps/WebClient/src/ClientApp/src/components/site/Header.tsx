import { Link, usePathname, useRouter } from "expo-router";
import { useState } from "react";
import { Platform, Pressable, useWindowDimensions, View } from "react-native";

import HgLogo from "@/assets/images/gov/hg-logo-rev.svg";
import HgLogoDark from "@/assets/images/gov/hg-logo-rev-dark.svg";
import { AppText, HgButton, Icon } from "@/components/ui";
import { Path } from "@/constants/path";
import {
    Action,
    Destination,
    InternalUrl,
    Text,
    Type,
} from "@/constants/tracking";
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

import { FeedbackDialog } from "./FeedbackDialog";
import { NotificationCentre } from "./NotificationCentre";
import { RatingDialog } from "./RatingDialog";
import { SettingsMenu } from "./SettingsMenu";
import { UserMenu } from "./UserMenu";
import { useSupportLinksShown } from "./useSupportLinksShown";

/** Port of HeaderComponent.vue (v-app-bar). */
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
    const [feedbackOpen, setFeedbackOpen] = useState(false);
    const [notificationButtonClicked, setNotificationButtonClicked] =
        useState(false);
    const isNotificationCentreEnabled = useConfigStore(
        (s) => selectFeatureToggles(s)?.notificationCentre?.enabled ?? false
    );
    const setNotificationCenterOpen = useNotificationStore(
        (s) => s.setNotificationCenterOpen
    );
    const newNotifications = useNewNotifications();
    // Native: the footer is suppressed; its links open from the account menu, or the gear menu when signed out
    const supportShown = useSupportLinksShown() && Platform.OS !== "web";

    const fullyRegistered = isFullyRegisteredUser();
    // Native main navigation is the bottom tab bar: no drawer, so no hamburger.
    const showSidebarButton = fullyRegistered && Platform.OS === "web";
    // Native: signed out, the header only has the gear. The way in is on the page itself (stated deviation)
    const showLogin =
        Platform.OS === "web" &&
        !isAuthenticated &&
        !isOffline &&
        pathname.toLowerCase() !== Path.Login;
    const showProfileLink =
        isAuthenticated && isValidIdp && !patientRetrievalFailed;
    // Signed in → Home (the route guard redirects non-registered states to the right page);
    // signed out → landing. Avoids the web client's gap where a signed-in user who is not yet in
    // the "registered" state (profile still loading, pending deletion) was sent to the marketing page.
    const logoDestination = isAuthenticated ? Path.Home : Path.Root;

    /**
     * Web: browser history, like the web client's link. Native: replaces the page. `dismissTo` did nothing on real devices
     * from another tab, and a push would stack another copy, leaving something to swipe back to.
     */
    function goToLogoDestination() {
        if (Platform.OS === "web") router.push(logoDestination);
        else if (pathname !== logoDestination) router.replace(logoDestination);
    }

    const accountButton = (
        <Pressable
            testID="headerDropdownBtn"
            accessibilityRole="button"
            accessibilityLabel="Account menu"
            aria-expanded={menuOpen}
            {...(Platform.OS === "web"
                ? ({ "aria-haspopup": "menu" } as object)
                : {})}
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
        // let the menu slide away first: iOS cannot present a modal over one still on screen
        setTimeout(
            () => {
                if (isValidIdp) setRatingOpen(true);
                else processLogout();
            },
            Platform.OS === "web" ? 0 : 400
        );
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
                    {...(Platform.OS === "web"
                        ? ({ "aria-controls": "sidenavbar" } as object)
                        : {})}
                    onPress={() => toggleSidebar(isMobile)}
                    className="p-3 mr-1"
                >
                    <Icon name="bars" size={24} color={colors["bc-black"]} />
                </Pressable>
            ) : null}
            {/* the web client's <router-link>: a real link on web, so the keyboard can follow it; the press tracks
                and navigates itself */}
            <Link
                href={toHref(logoDestination)}
                asChild
                onPress={(event) => {
                    event.preventDefault();
                    services.tracking.trackEvent({
                        action: Action.InternalLink,
                        text: Text.HealthGatewayLogo,
                        destination: Destination.Home,
                        url: InternalUrl.Home,
                    });
                    goToLogoDestination();
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
                        // Native: the drawer's Feedback entry lives here instead
                        onFeedback={
                            Platform.OS !== "web" && fullyRegistered
                                ? () => {
                                      setMenuOpen(false);
                                      services.tracking.trackEvent({
                                          action: Action.ButtonClick,
                                          text: Text.SendFeedback,
                                          type: Type.Sidebar,
                                      });
                                      // let the menu modal dismiss before the next one presents (iOS)
                                      setTimeout(
                                          () => setFeedbackOpen(true),
                                          Platform.OS === "ios" ? 400 : 0
                                      );
                                  }
                                : undefined
                        }
                        showSupport={supportShown}
                    />
                    <FeedbackDialog
                        visible={feedbackOpen}
                        onClose={() => setFeedbackOpen(false)}
                    />
                    <RatingDialog
                        visible={ratingOpen}
                        onClose={() => {
                            setRatingOpen(false);
                            // let the modal finish dismissing before the route changes (iOS otherwise can leave a
                            // stuck invisible modal window that blocks touches)
                            setTimeout(
                                processLogout,
                                Platform.OS === "ios" ? 400 : 0
                            );
                        }}
                    />
                </>
            ) : (
                <>
                    {Platform.OS !== "web" ? <SettingsMenu /> : null}
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
