import { type ComponentRef, useEffect, useRef, useState } from "react";
import {
    Image,
    type ImageSourcePropType,
    type NativeScrollEvent,
    type NativeSyntheticEvent,
    ScrollView,
    useWindowDimensions,
    View,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { DevBanner } from "@/components/site/DevBanner";
import { AppText, HgButton, Icon } from "@/components/ui";
import {
    type BubblePlacement,
    type OnboardingScreen,
    onboardingScreens,
} from "@/constants/onboarding";
import { useThemeColors } from "@/hooks/useTheme";
import { useAppLockStore } from "@/stores/appLock.store";
import { useOnboardingStore } from "@/stores/onboarding.store";
import { LocalAuthPanel } from "@/views/appLock/LocalAuthPanel";

const PHONE_WIDTH = 68;
const PHONE_HEIGHT = 128;

const BUBBLE_WIDTH = 92;
const BUBBLE_HEIGHT = 76;

/** The feature's navigation icon in a speech bubble pointing at the phone. */
function Bubble({
    icon,
    placement,
}: {
    icon: string;
    placement: BubblePlacement;
}) {
    const colors = useThemeColors();
    const tail: Record<BubblePlacement["tail"], string> = {
        bottom: "-bottom-1.5 self-center",
        left: "-left-1.5 top-8",
        right: "-right-1.5 top-8",
    };
    return (
        <View
            testID="onboardingBubble"
            className="absolute items-center justify-center rounded-md bg-surface border border-border-light shadow-lg"
            style={{
                width: BUBBLE_WIDTH,
                height: BUBBLE_HEIGHT,
                left: PHONE_WIDTH / 2 + placement.x - BUBBLE_WIDTH / 2,
                top: PHONE_HEIGHT / 2 + placement.y - BUBBLE_HEIGHT / 2,
            }}
        >
            <View
                className={`absolute w-3 h-3 rotate-45 bg-surface ${tail[placement.tail]}`}
            />
            <Icon name={icon} size={36} color={colors.primary} />
        </View>
    );
}

/** The phone with the feature's icon beside it, then the text (layout from the old app's OnboardingCollectionViewCell). */
function Slide({
    screen,
    width,
    showNew,
}: {
    screen: OnboardingScreen;
    width: number;
    showNew: boolean;
}) {
    return (
        <ScrollView
            style={{ width }}
            contentContainerClassName="grow items-center justify-center px-10 py-6"
        >
            <View
                className="mt-10"
                style={{ width: PHONE_WIDTH, height: PHONE_HEIGHT }}
            >
                <Image
                    source={
                        require("@/assets/images/onboarding/blank-phone.png") as ImageSourcePropType
                    }
                    style={{ width: PHONE_WIDTH, height: PHONE_HEIGHT }}
                    resizeMode="contain"
                />
                <Bubble icon={screen.icon} placement={screen.bubble} />
            </View>
            {showNew ? (
                <AppText
                    testID="onboardingNewLabel"
                    variant="label"
                    className="mt-10 text-primary"
                >
                    NEW
                </AppText>
            ) : null}
            <AppText
                variant="h3"
                accessibilityRole="header"
                className={showNew ? "mt-1 text-center" : "mt-12 text-center"}
            >
                {screen.title}
            </AppText>
            <AppText className="mt-3 text-center">{screen.description}</AppText>
        </ScrollView>
    );
}

/** Port of InitialOnboardingViewController: swipe or Next through the unseen screens, or skip them. */
function Tutorial({ onDone }: { onDone: () => void }) {
    const insets = useSafeAreaInsets();
    const { width } = useWindowDimensions();
    const screens = useOnboardingStore((s) => s.screens);
    const pager = useRef<ComponentRef<typeof ScrollView>>(null);
    const [index, setIndex] = useState(0);
    const isLast = index === screens.length - 1;
    const isFirstRun = screens.length === onboardingScreens.length;

    // a rotation changes the page width: stay on the same page
    useEffect(() => {
        pager.current?.scrollTo({ x: index * width, animated: false });
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [width]);

    const onScrollEnd = (e: NativeSyntheticEvent<NativeScrollEvent>) => {
        const page = Math.round(e.nativeEvent.contentOffset.x / width);
        setIndex(Math.max(0, Math.min(page, screens.length - 1)));
    };

    const next = () => {
        if (isLast) {
            onDone();
            return;
        }
        pager.current?.scrollTo({ x: (index + 1) * width, animated: true });
        setIndex(index + 1);
    };

    return (
        <View
            testID="onboardingTutorial"
            className="flex-1 bg-background"
            style={{ paddingBottom: insets.bottom + 16 }}
        >
            <ScrollView
                ref={pager}
                horizontal
                pagingEnabled
                showsHorizontalScrollIndicator={false}
                onMomentumScrollEnd={onScrollEnd}
                className="flex-1"
            >
                {screens.map((screen) => (
                    <Slide
                        key={screen.type}
                        screen={screen}
                        width={width}
                        showNew={!isFirstRun}
                    />
                ))}
            </ScrollView>
            {screens.length > 1 ? (
                <View
                    testID="onboardingDots"
                    className="flex-row justify-center gap-[10px] mb-6"
                >
                    {screens.map((screen, i) => (
                        <View
                            key={screen.type}
                            className={
                                i === index
                                    ? "w-5 h-[10px] rounded-full bg-primary"
                                    : "w-[10px] h-[10px] rounded-full bg-border-divider"
                            }
                        />
                    ))}
                </View>
            ) : null}
            <HgButton
                testID="onboardingNextBtn"
                text={!isLast ? "Next" : isFirstRun ? "Get started" : "OK"}
                uppercase={false}
                className="self-center min-w-[162px]"
                onPress={next}
            />
            <View className="min-h-[48px] items-center justify-center mt-2">
                {!isLast ? (
                    <HgButton
                        testID="onboardingSkipBtn"
                        text="Skip intro"
                        variant="link"
                        uppercase={false}
                        className="self-center"
                        onPress={onDone}
                    />
                ) : null}
            </View>
        </View>
    );
}

/** After the tutorial: opt in to the app lock. "Not now" is new here: the old apps forced the lock on everyone. */
function SecurityOffer() {
    const enable = useAppLockStore((s) => s.enable);
    const authenticating = useAppLockStore((s) => s.authenticating);
    const complete = useOnboardingStore((s) => s.completeSecurityOffer);
    return (
        <LocalAuthPanel
            onAuthenticate={() => {
                void enable().then((enabled) => {
                    if (enabled) void complete();
                });
            }}
            busy={authenticating}
            secondary={
                <HgButton
                    testID="localAuthNotNowBtn"
                    text="Not now"
                    variant="secondary"
                    uppercase={false}
                    block
                    className="mt-3"
                    onPress={() => void complete()}
                />
            }
        />
    );
}

/** First launch on iOS and Android: the intro, then the security step. The root shows it instead of the app. */
export function OnboardingView() {
    const insets = useSafeAreaInsets();
    const tutorialPending = useOnboardingStore((s) => s.screens.length > 0);
    const completeTutorial = useOnboardingStore((s) => s.completeTutorial);
    return (
        // the environment banner on top, as on the lock screen and in the app (none on Production)
        <View
            className="flex-1 bg-background"
            style={{ paddingTop: insets.top }}
        >
            <DevBanner />
            {tutorialPending ? (
                <Tutorial onDone={() => void completeTutorial()} />
            ) : (
                <SecurityOffer />
            )}
        </View>
    );
}
