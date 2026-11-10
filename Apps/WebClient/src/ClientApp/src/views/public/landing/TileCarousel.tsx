import { useRef, useState } from "react";
import {
    type GestureResponderEvent,
    type LayoutChangeEvent,
    type NativeScrollEvent,
    type NativeSyntheticEvent,
    Platform,
    Pressable,
    ScrollView,
    type ScrollViewInstance,
    useWindowDimensions,
    View,
} from "react-native";

import { AppText, HgButton } from "@/components/common";
import {
    Action,
    LandingAccessLinkDestination,
    LandingAccessLinkText,
    Origin,
    Type,
} from "@/constants/tracking";
import {
    notifyScrollDragEnd,
    usePressHighlight,
} from "@/hooks/usePressHighlight";
import type { InfoTile } from "@/models/infoTile";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { cn } from "@/utils/cn";
import { openExternal } from "@/utils/openExternal";

import {
    CAROUSEL_CONTROLS_HEIGHT,
    carouselHeight,
    gridColumnClass,
    pageIndex,
    wrapPage,
} from "./carouselLayout";

interface Props {
    tiles: InfoTile[];
    /** The web client's `data-testid`s, which Cypress reads: the grid's cards... */
    cardTestID: string;
    /** ...and the carousel's cards, which also name the carousel */
    mobileCardTestID: string;
    /** columns on wide screens */
    columns: 3 | 4;
    carousel: boolean;
}

function Tile({
    tile,
    cardTestID,
    carousel,
}: {
    tile: InfoTile;
    cardTestID: string;
    carousel: boolean;
}) {
    const { highlighted, pressed, handlers } = usePressHighlight();
    const open = (action: Action) => {
        if (!tile.link) return;
        void openExternal(tile.link);
        const text = LandingAccessLinkText[tile.type];
        if (!text) {
            logger.warn(
                `openExternalLink: unsupported landing access link type: ${tile.type}`
            );
            return;
        }
        services.tracking.trackEvent({
            action,
            text,
            origin: Origin.Landing,
            destination: LandingAccessLinkDestination[tile.type],
            type: Type.Landing,
            url: tile.link,
        });
    };
    return (
        // v-card variant="outlined": a press target, not a button role, so it can hold the Read more button
        <Pressable
            accessible={false}
            tabIndex={-1}
            onPress={() => open(Action.CardClick)}
            {...handlers}
            testID={`${cardTestID}-${tile.type}`}
            className={cn(
                // v-card: h-100 d-flex flex-column pa-5 pa-lg-4; the carousel card keeps pb-12 clear for the delimiters
                "grow justify-between rounded-md border border-border-light bg-surface p-5 lg:p-4 web:cursor-pointer",
                carousel && "pb-12",
                // a press shades at once; the hover, and the end of a press, fade
                !pressed && "web:transition-colors",
                highlighted && "bg-surface-hover"
            )}
        >
            <View
                // the min-height is the carousel's only (minHeight 120px on xs); the grid's flex-grow-1 has none
                className={`grow ${carousel ? "min-h-[120px]" : ""}`}
            >
                <AppText
                    variant="h3"
                    className="text-bc-black mb-2"
                    accessibilityRole="header"
                >
                    {tile.name}
                </AppText>
                <AppText>{tile.description}</AppText>
            </View>
            {/* v-card-actions: min-height 52, padding 8 (pt-4 in the grid, pb-1 in the carousel), centred */}
            <View
                className={`min-h-[52px] flex-row items-center justify-center px-2 ${
                    carousel ? "pt-2 pb-1" : "pt-4 pb-2"
                }`}
            >
                <HgButton
                    text="Read more"
                    variant="secondary"
                    uppercase={false}
                    slim
                    className="self-center"
                    accessibilityLabel={`Read more about ${tile.name}`}
                    testID={`${carousel ? "mobile-" : ""}read-more-button-${tile.type}`}
                    onPress={() => open(Action.ExternalLink)}
                />
            </View>
        </Pressable>
    );
}

/**
 * Where a touch event happened. Native touch events carry it at the top level as well; on web react-native-web passes
 * the DOM event, which has it only in `changedTouches`.
 */
function touchPoint(e: GestureResponderEvent) {
    const touch = e.nativeEvent.changedTouches[0] ?? e.nativeEvent;
    return { x: touch.pageX, y: touch.pageY };
}

/** v-carousel on xs, responsive grid otherwise. */
export function TileCarousel({
    tiles,
    cardTestID,
    mobileCardTestID,
    columns,
    carousel,
}: Props) {
    const { width } = useWindowDimensions();
    const [page, setPage] = useState(0);
    const [measuredWidth, setMeasuredWidth] = useState(0);
    const scrollRef = useRef<ScrollViewInstance>(null);
    // where a touch started, and on which page, for wrapping past the ends
    const touchStart = useRef<{ x: number; y: number; page: number }>(
        undefined
    );

    if (carousel) {
        // Slides fill the carousel (v-carousel class="w-100"); until measured, assume the shell's px-4.
        const pageWidth = measuredWidth || width - 32;
        const height = carouselHeight(columns, width);
        const onLayout = (e: LayoutChangeEvent) =>
            setMeasuredWidth(Math.round(e.nativeEvent.layout.width));
        const onScroll = (e: NativeSyntheticEvent<NativeScrollEvent>) =>
            setPage(
                pageIndex(
                    e.nativeEvent.contentOffset.x,
                    pageWidth,
                    tiles.length
                )
            );
        // Observes touches only (no responder), so the native paging scroll keeps the swipe itself.
        const onTouchStart = (e: GestureResponderEvent) => {
            touchStart.current = { ...touchPoint(e), page };
        };
        const onTouchEnd = (e: GestureResponderEvent) => {
            const start = touchStart.current;
            touchStart.current = undefined;
            if (!start) return;
            const end = touchPoint(e);
            const target = wrapPage(
                start.page,
                tiles.length,
                end.x - start.x,
                end.y - start.y
            );
            if (target !== undefined)
                scrollRef.current?.scrollTo({
                    x: target * pageWidth,
                    animated: true,
                });
        };
        return (
            <View
                testID={`${mobileCardTestID}-carousel`}
                style={{ height }}
                onLayout={onLayout}
                onTouchStart={onTouchStart}
                onTouchEnd={onTouchEnd}
            >
                <ScrollView
                    ref={scrollRef}
                    horizontal
                    pagingEnabled
                    showsHorizontalScrollIndicator={false}
                    onScroll={onScroll}
                    scrollEventThrottle={16}
                    // a tile touched as the swipe began stays shaded until the finger lifts (usePressHighlight)
                    onScrollEndDrag={notifyScrollDragEnd}
                >
                    {tiles.map((tile) => (
                        <View
                            key={tile.type}
                            style={{ width: pageWidth, height }}
                        >
                            <Tile
                                tile={tile}
                                cardTestID={mobileCardTestID}
                                carousel
                            />
                        </View>
                    ))}
                </ScrollView>
                {/* v-carousel__controls (hide-delimiter-background): a 50pt strip overlaid on the bottom of the
                    slide; x-small icon buttons 32pt with 8pt side margins, 15pt circle at half opacity,
                    the active one with the 12% primary overlay. */}
                <View
                    // v-carousel__controls: not a style, but Cypress finds the delimiters by it
                    className="v-carousel__controls absolute bottom-0 left-0 right-0 flex-row items-center justify-center"
                    style={{
                        height: CAROUSEL_CONTROLS_HEIGHT,
                        pointerEvents: "box-none",
                    }}
                >
                    {tiles.map((t, i) => (
                        <Pressable
                            key={t.type}
                            accessibilityRole="button"
                            accessibilityLabel={`Carousel slide ${i + 1} of ${tiles.length}`}
                            // the slide on screen: web `aria-current` (aria-selected is not allowed on a button,
                            // Lighthouse's "ARIA attributes do not match their roles"); native reads "selected"
                            {...(Platform.OS === "web"
                                ? ((i === page
                                      ? { "aria-current": "true" }
                                      : {}) as object)
                                : { "aria-selected": i === page })}
                            testID={`${mobileCardTestID}-carousel-delimiter-${i}`}
                            className="w-8 h-8 mx-2 rounded-full items-center justify-center"
                            onPress={() =>
                                scrollRef.current?.scrollTo({
                                    x: i * pageWidth,
                                    animated: true,
                                })
                            }
                        >
                            {i === page ? (
                                <View className="absolute inset-0 rounded-full bg-primary opacity-[0.12]" />
                            ) : null}
                            <View className="w-[15px] h-[15px] rounded-full bg-primary opacity-50" />
                        </Pressable>
                    ))}
                </View>
            </View>
        );
    }

    const basis = gridColumnClass(columns, Platform.OS === "web");
    return (
        // v-row gap="32" class="mb-6 mb-md-8": 32pt gutters (px-4 on each column, -mx-4 on the row) and 32pt
        // between rows (mb-8); the row's -mb-2 below md leaves the last row Vuetify's 24pt, its 32pt from md
        <View className="flex-row flex-wrap -mx-4 -mb-2 md:mb-0">
            {tiles.map((tile) => (
                <View key={tile.type} className={`${basis} px-4 mb-8 flex`}>
                    <Tile
                        tile={tile}
                        cardTestID={cardTestID}
                        carousel={false}
                    />
                </View>
            ))}
        </View>
    );
}
