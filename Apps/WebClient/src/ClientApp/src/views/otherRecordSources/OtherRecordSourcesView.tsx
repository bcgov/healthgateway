import { useState } from "react";
import { Image, Platform, Pressable, View } from "react-native";

import AccessMyHealthLogo from "@/assets/images/home/other/access-my-health.svg";
import IslandHealthLogo from "@/assets/images/home/other/island-health.svg";
import { Breadcrumb } from "@/components/site/Breadcrumb";
import { ExternalLinkConfirmationDialog } from "@/components/site/ExternalLinkConfirmationDialog";
import { AppText, LogoPlate, PageTitle } from "@/components/ui";
import {
    getOtherRecordSourcesLinks,
    type RecordSourceTile,
} from "@/constants/accessLinks";
import { ExternalUrl } from "@/constants/externalUrls";
import { Path } from "@/constants/path";
import {
    Action,
    Destination,
    InternalUrl,
    Origin,
    ResourceLinkDestination,
    ResourceLinkTileText,
    ResourceLinkUrlText,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { selectWebConfig, useConfigStore } from "@/stores/config.store";
import { ConfigUtil } from "@/utils/configUtil";
import { hrefProps } from "@/utils/hrefProps";
import { openExternal } from "@/utils/openExternal";
import { HomeCard } from "@/views/home/HomeCard";

// Each logo renders at its SVG's intrinsic size, exactly as the web client's <img> does. The three whose SVGs
// only wrap a bitmap use that bitmap directly at the same intrinsic size.
const logos: Record<
    RecordSourceTile["logo"],
    {
        Svg?: React.FC<{ width: number; height: number }>;
        png?: number;
        width: number;
        height: number;
    }
> = {
    "access-my-health": { Svg: AccessMyHealthLogo, width: 134, height: 22 },
    "island-health": { Svg: IslandHealthLogo, width: 47, height: 28 },
    "interior-health": {
        png: require("@/assets/images/home/other/interior-health.png") as number,
        width: 130,
        height: 28,
    },
    "northern-health": {
        png: require("@/assets/images/home/other/northern-health.png") as number,
        width: 113,
        height: 28,
    },
    "fraser-health": {
        png: require("@/assets/images/home/other/fraser-health.png") as number,
        width: 104,
        height: 28,
    },
};

function Logo({ name }: { name: RecordSourceTile["logo"] }) {
    const { Svg, png, width, height } = logos[name];
    return (
        // decorative: the card title names the source
        <View
            aria-hidden
            accessibilityElementsHidden
            importantForAccessibility="no-hide-descendants"
        >
            <LogoPlate>
                {Svg ? (
                    <Svg width={width} height={height} />
                ) : (
                    <Image
                        source={png}
                        style={{ width, height }}
                        resizeMode="contain"
                        accessibilityIgnoresInvertColors
                    />
                )}
            </LogoPlate>
        </View>
    );
}

/** useGrid.ts columns, with the native two-column rule from 600pt (as on Home) */
function useColumns() {
    const { width } = useBreakpoint();
    if (width >= 1920) return 4;
    if (width >= 1280) return 3;
    if (width >= 960) return 2;
    if (Platform.OS !== "web" && width >= 600) return 2;
    return 1;
}

/** Port of OtherRecordSourcesView.vue */
export function OtherRecordSourcesView() {
    const columns = useColumns();
    const accessMyHealthUrl = useConfigStore(
        (s) => selectWebConfig(s)?.accessMyHealthUrl
    );
    const [pending, setPending] = useState<RecordSourceTile | undefined>(
        undefined
    );

    const tiles = getOtherRecordSourcesLinks().filter((t) =>
        t.type === "AccessMyHealth"
            ? ConfigUtil.isOtherRecordSourcesCardEnabled("accessMyHealth")
            : true
    );

    const urlFor = (tile: RecordSourceTile) =>
        tile.type === "AccessMyHealth" ? accessMyHealthUrl : tile.link;

    function trackClick(tile: RecordSourceTile, text: Text, url: string) {
        const destination = ResourceLinkDestination[tile.type];
        if (!destination) {
            logger.warn(
                `trackRecordSourceClick: unsupported resource link type: ${tile.type}`
            );
            return;
        }
        services.tracking.trackEvent({
            action: Action.ExternalLink,
            text,
            origin: Origin.OtherRecordSources,
            destination,
            type: Type.RecordSourceTile,
            url,
        });
    }

    function handleClick(tile: RecordSourceTile, kind: "tile" | "link") {
        const text = (
            kind === "tile" ? ResourceLinkTileText : ResourceLinkUrlText
        )[tile.type];
        if (!text) {
            logger.warn(
                `handleRecordSourceClick(${kind}): unsupported resource link type: ${tile.type}`
            );
            return;
        }
        const url = urlFor(tile);
        if (!url) return;
        trackClick(tile, text, url);
        if (tile.type === "AccessMyHealth") {
            setPending(tile); // AccessMyHealth prompts first
            return;
        }
        void openExternal(url);
    }

    function cancelExternal() {
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.AccessMyHealthDialogCancel,
            origin: Origin.OtherRecordSources,
            type: Type.RecordSourceTile,
            url: InternalUrl.OtherRecordSources,
        });
        setPending(undefined);
    }

    function confirmExternal() {
        const url = pending ? urlFor(pending) : undefined;
        setPending(undefined);
        if (!url) return;
        void openExternal(url);
        services.tracking.trackEvent({
            action: Action.ButtonClick,
            text: Text.AccessMyHealthDialogSignin,
            origin: Origin.OtherRecordSources,
            destination: Destination.AccessMyHealth,
            type: Type.RecordSourceTile,
            url,
        });
    }

    const cardWidth =
        columns === 1
            ? "w-full"
            : columns === 2
              ? "w-1/2"
              : columns === 3
                ? "w-1/3"
                : "w-1/4";

    return (
        <View>
            <Breadcrumb
                items={[
                    {
                        text: "Other record sources",
                        to: Path.OtherRecordSources,
                        active: true,
                        testId: "breadcrumb-other-record-sources",
                    },
                ]}
            />
            <PageTitle title="Other record sources" />
            <ExternalLinkConfirmationDialog
                visible={pending !== undefined}
                origin={Origin.OtherRecordSources}
                title="Do you want to open AccessMyHealth?"
                body={[
                    {
                        text: Text.AccessMyHealth,
                        trackingText: Text.AccessMyHealthDialogUrl,
                        href: TrackedExternalUrl.AccessMyHealth,
                        suffix: " may contain more health records and information.",
                    },
                    "You are currently signed in to Health Gateway. If you wish to continue, you will be signed in to AccessMyHealth in a new window. You must have at least one record in AccessMyHealth for this to work.",
                ]}
                confirmLabel="Sign in"
                cancelLabel="Cancel"
                onConfirm={confirmExternal}
                onCancel={cancelExternal}
            />
            <AppText>
                Health Gateway helps bring your records together in one place.
                It connects to many record sources, but not all. Included below
                are trusted regional patient websites where you can find more
                records and health services.
            </AppText>
            <View className="mt-4 md:mt-6 flex-row flex-wrap -m-2 items-stretch">
                {tiles.map((tile) => {
                    return (
                        <View
                            key={tile.type}
                            className={`${cardWidth} p-2 flex`}
                        >
                            <HomeCard
                                title={tile.name}
                                testID={`other-record-sources-card-${tile.type}`}
                                icon={<Logo name={tile.logo} />}
                                onPress={() => handleClick(tile, "tile")}
                            >
                                <AppText>{tile.description}</AppText>
                                <View className="mt-auto pt-3">
                                    <Pressable
                                        // the web client's <a> never follows its href (@click.prevent): it opens the
                                        // site, or the confirmation dialog, from the click, so it is a button
                                        accessibilityRole="button"
                                        testID={`other-record-sources-link-${tile.type}`}
                                        onPress={() =>
                                            handleClick(tile, "link")
                                        }
                                    >
                                        <AppText className="text-link">
                                            {tile.linkText}
                                        </AppText>
                                    </Pressable>
                                </View>
                            </HomeCard>
                        </View>
                    );
                })}
            </View>
            <View className="mt-6 pt-2 flex-row flex-wrap">
                <AppText variant="body-medium">
                    For more information on these websites and more visit{" "}
                </AppText>
                <Pressable
                    accessibilityRole="link"
                    // a real anchor on web, so the keyboard can follow it; the press tracks and opens it itself
                    {...hrefProps(ExternalUrl.YourHealthInformation, {
                        newTab: true,
                    })}
                    onPress={(event) => {
                        event.preventDefault();
                        services.tracking.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.HealthLinkBC,
                            origin: Origin.OtherRecordSources,
                            destination: Destination.HealthLinkBC,
                            url: TrackedExternalUrl.YourHealthInformation,
                        });
                        void openExternal(ExternalUrl.YourHealthInformation);
                    }}
                >
                    <AppText
                        variant="body-medium"
                        className="text-link underline"
                    >
                        HealthLink BC
                    </AppText>
                </Pressable>
                <AppText variant="body-medium">.</AppText>
            </View>
        </View>
    );
}
