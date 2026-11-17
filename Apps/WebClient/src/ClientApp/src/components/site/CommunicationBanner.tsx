import { createElement, useMemo } from "react";
import { Platform, View } from "react-native";

import { AppText } from "@/components/common";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useCommunicationType } from "@/hooks/useCommunication";
import { useCommunicationStore } from "@/stores/communication.store";
import { breakpoints } from "@/theme";
import { parseCommunicationHtml } from "@/utils/communicationHtml";
import { openExternal } from "@/utils/openExternal";

/** v-banner body-2 metrics */
const bannerText = { fontSize: 14, lineHeight: 20 } as const;

/**
 * v-banner-text keeps room at its end for actions the banner never has: 90px, or 36px while Vuetify counts the
 * window as mobile (`v-banner--stacked`, below its lg breakpoint, 1280).
 */
const textEndPadding = (width: number) => (width < breakpoints.lg ? 36 : 90);

/** Native: the banner's HTML as styled runs of text, with links tappable. */
function NativeBannerText({ html }: { html: string }) {
    const segments = useMemo(() => parseCommunicationHtml(html), [html]);
    return (
        <AppText
            variant="body-medium"
            className="text-banner-text"
            // inline so nested spans cannot re-expand the line box to the body size
            style={bannerText}
        >
            {segments.map((segment, i) => (
                <AppText
                    key={i}
                    variant="body-medium"
                    className={[
                        segment.bold ? "font-bold" : "",
                        segment.italic ? "italic" : "",
                        // each run sets its own colour: AppText's default would override the outer one
                        segment.href
                            ? "text-banner-link underline"
                            : "text-banner-text",
                    ].join(" ")}
                    style={bannerText}
                    onPress={
                        segment.href
                            ? () => void openExternal(segment.href as string)
                            : undefined
                    }
                >
                    {segment.text}
                </AppText>
            ))}
        </AppText>
    );
}

/**
 * Port of CommunicationComponent.vue: Banner for the public site, InApp once registered. Shows what the communication
 * store holds; `useCommunicationRetrieval`, mounted once in the shell, keeps it current, so every page's banner (iOS
 * and Android keep each tab's page mounted) shares one call.
 */
export function CommunicationBanner() {
    const { width } = useBreakpoint();
    const type = useCommunicationType();
    const text = useCommunicationStore(
        (s) => s.entries[type]?.communication?.text
    );

    if (!text) return null;
    return (
        <View
            // v-banner density default: 16px vertical padding, 16px start and 8px end, body-2 text (14px / 20px);
            // justify-center centres the text's box, and the text inside it starts at the left
            className="bg-info-background border-b border-border-light pl-4 pr-2 py-4 items-center"
        >
            <View style={{ paddingRight: textEndPadding(width) }}>
                {Platform.OS === "web" ? (
                    createElement("div", {
                        "data-testid": "communicationBanner",
                        className:
                            "font-sans text-banner-text text-[14px] leading-5 [&_a]:text-banner-link [&_a]:underline",
                        dangerouslySetInnerHTML: { __html: text },
                    })
                ) : (
                    <View testID="communicationBanner">
                        <NativeBannerText html={text} />
                    </View>
                )}
            </View>
        </View>
    );
}
