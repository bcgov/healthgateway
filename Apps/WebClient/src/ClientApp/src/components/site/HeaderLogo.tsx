import { Pressable, View } from "react-native";

import HgLogo from "@/assets/images/gov/hg-logo-rev.svg";
import HgLogoDark from "@/assets/images/gov/hg-logo-rev-dark.svg";
import { useColorScheme } from "@/hooks/useTheme";
import { decorativeProps } from "@/utils/decorative";
import { hrefProps } from "@/utils/hrefProps";

import { useLogoLink } from "./useLogoLink";

/**
 * The artwork's own bounds inside both logo files (their viewBox is 0 0 2423 759, with a wide empty margin): drawn
 * cropped to it, the logo fills the bar's height, which makes "Health Gateway" about the size of the breadcrumbs' text
 * (16px) instead of seven tenths of it.
 */
const ARTWORK = { x: 140, y: 145, width: 2150, height: 530 };
/** Fits iOS's 44pt navigation bar (Android's top app bar is taller); the same on both, so the text matches. */
const LOGO_HEIGHT = 38;
const LOGO_WIDTH = Math.round((LOGO_HEIGHT * ARTWORK.width) / ARTWORK.height);
const VIEW_BOX = `${ARTWORK.x} ${ARTWORK.y} ${ARTWORK.width} ${ARTWORK.height}`;

/** The bar's item height: iOS's navigation bar; the logo is centred in it. */
const ITEM_HEIGHT = 44;

/**
 * The logo as the navigation bar draws it, at the bar's size. Decorative: `HeaderLogo`'s link names it, and on the
 * screens over the app (`StaticHeaderLogo`) their own text names the app.
 */
function HeaderLogoImage() {
    // Dark variant: the navy wordmark, crest text and mountains become pale blue (docs/theming.md).
    const Logo = useColorScheme() === "dark" ? HgLogoDark : HgLogo;
    return (
        <Logo
            width={LOGO_WIDTH}
            height={LOGO_HEIGHT}
            viewBox={VIEW_BOX}
            preserveAspectRatio="xMinYMid meet"
            {...decorativeProps()}
            style={{ width: LOGO_WIDTH, height: LOGO_HEIGHT }}
        />
    );
}

/**
 * Native: the Health Gateway logo on the left of a navigation bar, on each tab's first page and every page outside the
 * tabs; the link home, as in the web header.
 */
export function HeaderLogo() {
    const logo = useLogoLink();
    return (
        <Pressable
            testID="headerLogo"
            accessibilityRole="link"
            accessibilityLabel="Go to Health Gateway home page"
            // native only today; a real link should this ever render on web
            {...hrefProps(logo.destination)}
            onPress={(event) => {
                event.preventDefault();
                logo.follow();
            }}
            hitSlop={4}
            style={{ height: ITEM_HEIGHT, justifyContent: "center" }}
        >
            <HeaderLogoImage />
        </Pressable>
    );
}

/**
 * Native: the logo on the navigation bar of a screen that covers the app (`OverAppScreen`: the intro, the lock screen,
 * Update required). Not a link: the app's pages are behind the screen.
 */
export function StaticHeaderLogo() {
    return (
        <View
            testID="headerLogo"
            style={{ height: ITEM_HEIGHT, justifyContent: "center" }}
        >
            <HeaderLogoImage />
        </View>
    );
}
