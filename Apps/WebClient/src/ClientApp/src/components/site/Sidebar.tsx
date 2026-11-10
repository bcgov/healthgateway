import { Link, usePathname, useRouter } from "expo-router";
import { useEffect, useState } from "react";
import { Platform, Pressable, View, type ViewStyle } from "react-native";
import {
    GestureDetector,
    PointerType,
    usePanGesture,
} from "react-native-gesture-handler";
import Reanimated, {
    Easing,
    useAnimatedStyle,
    useSharedValue,
    withSpring,
    withTiming,
} from "react-native-reanimated";
import { scheduleOnRN } from "react-native-worklets";

import { AppText, Icon } from "@/components/common";
import { webAttributeRef, webClassRef } from "@/components/common/webClass";
import { Action, Text, Type } from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useEscapeKey } from "@/hooks/useEscapeKey";
import { useMotion } from "@/hooks/useMotion";
import { useThemeColors } from "@/hooks/useTheme";
import { toHref } from "@/navigation/href";
import { menuEvents, menuItems, visibleMenuItems } from "@/navigation/menu";
import { isFullyRegisteredUser } from "@/navigation/userState";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { selectFeatureToggles, useConfigStore } from "@/stores/config.store";
import { selectIsSidebarOpen, useLayoutStore } from "@/stores/layout.store";
import { useUserStore } from "@/stores/user.store";
import { cn } from "@/utils/cn";

import { FeedbackDialog } from "./FeedbackDialog";

interface ItemProps {
    icon: string;
    title: string;
    active?: boolean;
    rail: boolean;
    /** v-list-item `to`: a real link on web, which Cypress checks; `onPress` still decides what a press does */
    href?: string;
    /** v-show: kept in the page, hidden */
    hidden?: boolean;
    testID: string;
    onPress: () => void;
    className?: string;
    /** the classes on the title (the menu's `list-item-title`: 13px / 16px) */
    titleClassName?: string;
}

/** v-list-item (density compact, nav): active = navHighlight, hover = surfaceHover, like the web client's .nav-hover */
function DrawerItem({
    icon,
    title,
    active,
    rail,
    href,
    hidden,
    testID,
    onPress,
    className = "",
    titleClassName,
}: ItemProps) {
    const colors = useThemeColors();
    const [hovered, setHovered] = useState(false);
    const bg = active ? "bg-nav-highlight" : hovered ? "bg-surface-hover" : "";
    const item = (
        <Pressable
            testID={testID}
            // a real link (web: `<a href>`) when it goes somewhere; Feedback opens a dialog, so it is a button
            accessibilityRole={href ? "link" : "button"}
            accessibilityLabel={title}
            // Vuetify marks the active item aria-selected (kept for parity); aria-current is the right state on a link
            aria-selected={!!active}
            {...(active ? ({ "aria-current": "page" } as object) : {})}
            aria-hidden={hidden}
            onPress={href ? undefined : onPress}
            onHoverIn={() => setHovered(true)}
            onHoverOut={() => setHovered(false)}
            className={cn("flex-row items-center", bg, className)}
            style={hidden ? { display: "none" } : undefined}
        >
            <View className="w-6 items-center justify-center mr-8">
                <Icon name={icon} size={24} color={colors["bc-black"]} />
            </View>
            {!rail ? (
                <AppText numberOfLines={1} className={titleClassName}>
                    {title}
                </AppText>
            ) : null}
        </Pressable>
    );
    if (!href) return item;
    return (
        <Link
            href={toHref(href)}
            asChild
            onPress={(event) => {
                event.preventDefault();
                onPress();
            }}
        >
            {item}
        </Link>
    );
}

/** Vuetify v-navigation-drawer metrics used by SidebarComponent.vue */
const DRAWER_WIDTH = 256;
const RAIL_WIDTH = 56;
/** v-navigation-drawer__scrim's opacity (`--v-navigation-drawer-scrim-opacity`) */
const ScrimOpacity = 0.2;

/** the scrim's box: the whole content row, under the drawer too */
const fillArea = {
    position: "absolute",
    top: 0,
    right: 0,
    bottom: 0,
    left: 0,
} as const;

/** the temporary drawer's box: the content row's left edge, its full height, over the scrim */
const drawerPlace = {
    position: "absolute",
    top: 0,
    bottom: 0,
    left: 0,
} as const;

/** v-navigation-drawer--temporary.v-navigation-drawer--active: the level-3 shadow it casts over the page */
const temporaryDrawerShadow = {
    boxShadow:
        "0px 2px 3px 0px rgba(0,0,0,0.3), 0px 6px 10px 4px rgba(0,0,0,0.15)",
} as unknown as ViewStyle;

/** v-navigation-drawer's transition timing: Material's standard curve */
const standardEasing = Easing.bezier(0.4, 0, 0.2, 1);

/**
 * v-navigation-drawer's touch support, temporary drawer: a drag pulls the drawer only when it starts near its edge,
 * from 20pt inside to 80pt outside it.
 */
export function startsDrawerDrag(x: number, width: number): boolean {
    "worklet";
    const offset = x - width;
    return offset > -20 && offset < 80;
}

/**
 * On release a sideways flick (over 400pt/s) decides, open if it went right; otherwise the drawer stays open while
 * more than half of it shows.
 */
export function drawerStaysOpen(
    dx: number,
    vx: number,
    vy: number,
    width: number
): boolean {
    "worklet";
    if (Math.abs(vx) > 400 && Math.abs(vx) > Math.abs(vy)) return vx > 0;
    return 1 + dx / width > 0.5;
}

/**
 * Port of SidebarComponent.vue:
 * - ≥ md: permanent drawer (256px); the header hamburger collapses it to a 56px icon rail,
 *   and clicking anywhere on the rail expands it again.
 * - < md: temporary drawer over the page, opened by the hamburger, closed on navigate / scrim tap / Escape, or
 *   dragged shut from near its edge (touch and pen only, as Vuetify listens for touch events). Web only: native has
 *   HeaderMenu, which has its own swipe. Not ported: Vuetify also starts a drag 25pt from the left edge of the screen
 *   to open it, but SidebarComponent.vue's `@update:model-value="visibleOnMobile = false"` turns that open back into a
 *   close, so in the web client the drawer follows the finger and then snaps shut.
 */
export function Sidebar() {
    const { isMobile } = useBreakpoint();
    const router = useRouter();
    const pathname = usePathname();
    const toggles = useConfigStore(selectFeatureToggles);
    const isOpen = useLayoutStore(selectIsSidebarOpen(isMobile));
    const setSidebar = useLayoutStore((s) => s.setSidebar);
    useAuthStore((s) => s.isAuthenticated);
    useUserStore((s) => s.user);
    useUserStore((s) => s.patientRetrievalFailed);
    const [feedbackOpen, setFeedbackOpen] = useState(false);

    const motion = useMotion();
    const reduced = motion.reduced;
    const slideMs = motion.ms(200);

    // The temporary drawer's place: 0 open, -DRAWER_WIDTH shut. v-navigation-drawer slides it in and out over 0.2 s on
    // Material's standard curve, the scrim fading with it; a swipe near its edge drags it.
    const overlayOpen = isMobile && isOpen;
    const dragX = useSharedValue(overlayOpen ? 0 : -DRAWER_WIDTH);
    const dragging = useSharedValue(false);
    // shut by any means (hamburger, scrim, Escape, navigating, a swipe), it stays on the page until it has slid away;
    // not when the window widens, where the permanent drawer takes over at once
    const [wasOverlayOpen, setWasOverlayOpen] = useState(overlayOpen);
    const [closing, setClosing] = useState(false);
    if (wasOverlayOpen !== overlayOpen) {
        setWasOverlayOpen(overlayOpen);
        setClosing(!overlayOpen && isMobile);
    }
    useEffect(() => {
        if (overlayOpen)
            dragX.set(
                withTiming(0, { duration: slideMs, easing: standardEasing })
            );
        else if (closing)
            dragX.set(
                withTiming(
                    -DRAWER_WIDTH,
                    {
                        // a swipe has already slid it away
                        duration: dragX.get() <= -DRAWER_WIDTH ? 0 : slideMs,
                        easing: standardEasing,
                    },
                    (finished) => {
                        if (finished) scheduleOnRN(setClosing, false);
                    }
                )
            );
        else dragX.set(-DRAWER_WIDTH);
    }, [overlayOpen, closing, slideMs, dragX]);
    const closeSidebar = () => setSidebar(false);
    // v-navigation-drawer's rail transition: the width eases over 0.2 s (Material's standard curve); none when the
    // user asks for reduced motion. react-native-web passes the transition properties through to CSS.
    const railTransition = {
        transitionProperty: "width",
        transitionDuration: `${slideMs}ms`,
        transitionTimingFunction: "cubic-bezier(0.4, 0, 0.2, 1)",
    } as unknown as ViewStyle;
    const swipeClose = usePanGesture({
        enabled: isMobile && isOpen,
        testID: "sidenavbar-swipe",
        activeOffsetX: [-10, 10],
        failOffsetY: [-10, 10],
        onActivate: (e) => {
            "worklet";
            dragging.set(
                e.pointerType !== PointerType.MOUSE &&
                    startsDrawerDrag(e.x - e.translationX, DRAWER_WIDTH)
            );
        },
        onUpdate: (e) => {
            "worklet";
            if (dragging.get())
                dragX.set(Math.min(0, Math.max(-DRAWER_WIDTH, e.translationX)));
        },
        onDeactivate: (e) => {
            "worklet";
            if (!dragging.get()) return;
            dragging.set(false);
            if (
                !e.canceled &&
                !drawerStaysOpen(
                    e.translationX,
                    e.velocityX,
                    e.velocityY,
                    DRAWER_WIDTH
                )
            )
                dragX.set(
                    withTiming(
                        -DRAWER_WIDTH,
                        { duration: slideMs },
                        (finished) => {
                            if (finished) scheduleOnRN(closeSidebar);
                        }
                    )
                );
            else
                dragX.set(
                    reduced
                        ? withTiming(0, { duration: 0 })
                        : withSpring(0, { overshootClamping: true })
                );
        },
    });
    const drawerDragStyle = useAnimatedStyle(() => ({
        transform: [{ translateX: dragX.get() }],
    }));
    // v-navigation-drawer__scrim: black at 20 % (`--v-navigation-drawer-scrim-opacity`), faded with the slide and drag
    const scrimDragStyle = useAnimatedStyle(() => ({
        opacity: ScrimOpacity * (1 + dragX.get() / DRAWER_WIDTH),
    }));

    // Escape closes the temporary drawer, unless a dialog (such as Feedback) is open over it
    useEscapeKey(
        isFullyRegisteredUser() && !!toggles && isMobile && isOpen,
        () => setSidebar(false)
    );

    if (!isFullyRegisteredUser() || !toggles) return null;

    const visible = visibleMenuItems(toggles);
    const rail = !isMobile && !isOpen;

    function navigate(path: string) {
        if (isMobile) setSidebar(false);
        const e = menuEvents[path];
        if (e)
            services.tracking.trackEvent({
                action: Action.InternalLink,
                type: Type.Sidebar,
                ...e,
            });
        router.push(toHref(path));
    }

    const drawer = (
        <Pressable
            ref={webClassRef({ "v-navigation-drawer--rail": rail })}
            testID="sidenavbar"
            nativeID="sidenavbar"
            // the navigation landmark (web only: Android has no such role)
            accessibilityRole={Platform.OS === "web" ? "navigation" : undefined}
            accessibilityLabel="Main"
            accessible={false}
            // a click target only: not a tab stop (react-native-web gives every Pressable one)
            tabIndex={-1}
            // v-navigation-drawer @click.stop="collapsedOnDesktop = false"
            onPress={rail ? () => setSidebar(true) : undefined}
            style={[
                { width: rail ? RAIL_WIDTH : DRAWER_WIDTH },
                railTransition,
            ]}
            className="h-full bg-surface border-r border-border-light"
        >
            {/* v-list density="compact" nav */}
            <View className="p-2">
                {menuItems.map((item) => (
                    <DrawerItem
                        key={item.path}
                        href={item.path}
                        hidden={!visible.includes(item)}
                        testID={item.testID}
                        icon={item.icon}
                        title={item.title}
                        rail={rail}
                        active={pathname
                            .toLowerCase()
                            .startsWith(item.path.toLowerCase())}
                        onPress={() =>
                            rail ? setSidebar(true) : navigate(item.path)
                        }
                        className="rounded px-2 min-h-[40px] mb-1"
                        titleClassName="text-[13px] leading-4 tracking-normal"
                    />
                ))}
            </View>
            <View className="flex-1" />
            {/* #append: Feedback */}
            <DrawerItem
                testID="menu-btn-feedback-link"
                icon="comments"
                title="Feedback"
                rail={rail}
                onPress={() => {
                    if (rail) {
                        setSidebar(true);
                        return;
                    }
                    services.tracking.trackEvent({
                        action: Action.ButtonClick,
                        text: Text.SendFeedback,
                        type: Type.Sidebar,
                    });
                    setFeedbackOpen(true);
                }}
                className="px-4 min-h-[48px] bg-nav-highlight border-t border-border-divider"
            />
            <FeedbackDialog
                visible={feedbackOpen}
                onClose={() => setFeedbackOpen(false)}
            />
        </Pressable>
    );

    if (isMobile) {
        // v-navigation-drawer temporary: closed, it waits off the left of the screen
        if (!isOpen && !closing)
            return (
                <View
                    ref={webAttributeRef("inert", "")}
                    aria-hidden
                    className="absolute top-0 bottom-0"
                    style={{ left: -DRAWER_WIDTH * 1.1, pointerEvents: "none" }}
                >
                    {drawer}
                </View>
            );
        // Temporary drawer: overlays the content row (which starts under the header), scrim beside it. Sliding shut,
        // it is out of reach already: the page under it takes presses, and the keyboard and screen reader skip it.
        return (
            <GestureDetector
                gesture={swipeClose}
                touchAction="pan-y"
                userSelect="auto"
            >
                <View
                    testID="sidenavbar-overlay"
                    ref={webAttributeRef("inert", closing ? "" : false)}
                    aria-hidden={closing}
                    className="absolute inset-0 z-10"
                    style={{
                        elevation: 10,
                        pointerEvents: closing ? "none" : "auto",
                    }}
                >
                    {/* the scrim covers the whole area, under the drawer too, as Vuetify's does: the drawer slides
                        over a page that dims evenly, not into a gap beside a dimmed strip */}
                    <Reanimated.View style={[fillArea, scrimDragStyle]}>
                        <Pressable
                            className="flex-1 bg-black"
                            accessibilityRole="button"
                            accessibilityLabel="Close navigation"
                            onPress={() => setSidebar(false)}
                        />
                    </Reanimated.View>
                    <Reanimated.View
                        style={[
                            drawerPlace,
                            temporaryDrawerShadow,
                            drawerDragStyle,
                        ]}
                    >
                        {drawer}
                    </Reanimated.View>
                </View>
            </GestureDetector>
        );
    }
    return drawer;
}
