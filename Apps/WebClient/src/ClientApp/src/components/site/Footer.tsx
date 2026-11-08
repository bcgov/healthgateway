import { useRouter } from "expo-router";
import { Platform, Pressable, View } from "react-native";

import { AppText } from "@/components/ui";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { toHref } from "@/navigation/href";
import {
    followSupportLink,
    supportEmailLink,
    type SupportLink,
    supportLinks,
} from "@/navigation/supportLinks";
import { hrefProps } from "@/utils/hrefProps";

import { useSupportLinksShown } from "./useSupportLinksShown";

/** Port of FooterComponent.vue (v-footer: one row, links in BC black, email pushed to the right). */
export function Footer() {
    const router = useRouter();
    const { isMobile, width } = useBreakpoint();
    // stacked under md on web (v-footer cols=12 / md=auto); native uses the row from 600pt like the Home grid
    const stacked = isMobile && !(Platform.OS !== "web" && width >= 600);
    const shown = useSupportLinksShown();
    if (!shown) return null;

    const open = (l: SupportLink) =>
        followSupportLink(l, (path) => router.push(toHref(path)));

    const link = (l: SupportLink) => (
        <Pressable
            key={l.testID}
            testID={l.testID}
            accessibilityRole="link"
            // a real anchor on web (the web client's <a> / <router-link>), so the keyboard can follow it; the
            // press does the work (the event, the route or the new tab), so the browser must not follow it too
            {...hrefProps(l.to ?? l.href, {
                newTab: !!l.href && l.href.startsWith("http"),
            })}
            onPress={(event) => {
                event.preventDefault();
                open(l);
            }}
            className="py-2"
        >
            <AppText className="text-bc-black underline tracking-normal">
                {l.text}
            </AppText>
        </Pressable>
    );

    return (
        <View
            testID="footer"
            // v-footer's 16px plus its v-row's pa-2: links start 24px in
            className={`bg-surface border-t border-border-light px-6 py-2 ${
                stacked ? "" : "flex-row flex-wrap items-center justify-between"
            }`}
        >
            <View
                className={
                    stacked ? "" : "flex-row flex-wrap items-center gap-x-6"
                }
            >
                {supportLinks.map(link)}
            </View>
            {link(supportEmailLink)}
        </View>
    );
}
