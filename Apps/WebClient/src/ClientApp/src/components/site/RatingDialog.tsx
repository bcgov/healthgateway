import { useState } from "react";
import { Pressable, View } from "react-native";

import { AppText, Dialog, HgButton, Icon } from "@/components/common";
import { webClassRef } from "@/components/common/webClass";
import { Action, Rating, Text, Type } from "@/constants/tracking";
import { useThemeColors } from "@/hooks/useTheme";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";

interface Props {
    visible: boolean;
    onClose: () => void;
}

/** Port of RatingComponent.vue — shown before logout; posts UserFeedback/Rating. */
export function RatingDialog({ visible, onClose }: Props) {
    const colors = useThemeColors();
    const [rating, setRating] = useState(0);
    const [submitting, setSubmitting] = useState(false);

    // v-rating @update:model-value: tapping a star submits it straight away
    async function submit(value: number, skip: boolean) {
        setRating(value);
        setSubmitting(true);
        try {
            await services.userFeedback.submitRating({
                ratingValue: value,
                skip,
            });
            services.tracking.trackEvent({
                action: Action.Submit,
                text: Text.AppRating,
                rating: skip ? Rating.Skip : (String(value) as Rating),
                type: Type.Logout,
            });
        } catch (error) {
            logger.warn(`Rating submission failed: ${describeError(error)}`);
        } finally {
            setSubmitting(false);
            setRating(0);
            onClose();
        }
    }

    return (
        <Dialog
            visible={visible}
            title="Rate your experience"
            onRequestClose={() => void submit(0, true)}
            testID="ratingDialog"
            actions={
                <HgButton
                    nativeID="skipButton"
                    testID="ratingModalSkipBtn"
                    text="Skip"
                    variant="secondary"
                    onPress={() => submit(0, true)}
                    disabled={submitting}
                />
            }
        >
            <AppText className="mb-4">
                How would you rate your experience with Health Gateway today?
            </AppText>
            <View
                testID="formRating"
                className="flex-row justify-center gap-2 mb-2"
            >
                {[1, 2, 3, 4, 5].map((n) => (
                    <View
                        ref={webClassRef({ "v-rating__wrapper": true })}
                        key={n}
                    >
                        <Pressable
                            testID={`formRating-${n}`}
                            accessibilityRole="button"
                            accessibilityLabel={`${n} star${n > 1 ? "s" : ""}`}
                            disabled={submitting}
                            onPress={() => submit(n, false)}
                            hitSlop={6}
                        >
                            <Icon
                                name="star"
                                solid={n <= rating}
                                size={32}
                                color={
                                    n <= rating
                                        ? colors.accent
                                        : colors["bc-grey"]
                                }
                            />
                        </Pressable>
                    </View>
                ))}
            </View>
        </Dialog>
    );
}
