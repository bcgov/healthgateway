import { useRouter } from "expo-router";
import { useState } from "react";
import { Platform, View } from "react-native";

import {
    AppText,
    Dialog,
    HgAlert,
    HgButton,
    HgTextField,
    Icon,
} from "@/components/ui";
import { Path } from "@/constants/path";
import { useThemeColors } from "@/hooks/useTheme";
import { isSessionEnded, ResultError } from "@/models/errors";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { useErrorStore } from "@/stores/error.store";
import { useUserStore } from "@/stores/user.store";
import { openExternal } from "@/utils/openExternal";

interface Props {
    visible: boolean;
    onClose: () => void;
}

/** constants/clientType.ts on the web client: "Web" | "Mobile" */
const CLIENT_TYPE = Platform.OS === "web" ? "Web" : "Mobile";

/**
 * Port of FeedbackComponent.vue: comment (required, 5–500 chars) → POST UserFeedback/{hdid};
 * result states: Received (with / without a verified email) and Sorry (Try Again).
 */
export function FeedbackDialog({ visible, onClose }: Props) {
    const colors = useThemeColors();
    const router = useRouter();
    const user = useUserStore((s) => s.user);
    const errorStore = useErrorStore();
    const [comment, setComment] = useState("");
    const [touched, setTouched] = useState(false);
    const [loading, setLoading] = useState(false);
    const [submitted, setSubmitted] = useState(false);
    const [success, setSuccess] = useState(false);

    const hasEmail = user.verifiedEmail && user.hasEmail;
    const errors: string[] = [];
    if (touched) {
        if (comment.length === 0) errors.push("Value is required");
        else if (comment.length < 5)
            errors.push("This field should be at least 5 characters long");
        else if (comment.length > 500)
            errors.push("The maximum length allowed is 500");
    }
    const isValid = comment.length >= 5 && comment.length <= 500;

    const [focused, setFocused] = useState(false);

    function reset() {
        setComment("");
        setTouched(false);
        setSubmitted(false);
        setSuccess(false);
    }
    function hide() {
        reset();
        onClose();
    }

    async function submit() {
        setTouched(true);
        if (!isValid) return;
        setLoading(true);
        try {
            await services.userFeedback.submitFeedback(user.hdid, {
                comment,
                clientType: CLIENT_TYPE,
            });
            setSuccess(true);
        } catch (error) {
            if (isSessionEnded(error)) return;
            logger.error(describeError(error));
            if (error instanceof ResultError && error.statusCode === 429)
                errorStore.setTooManyRequestsError();
            setSuccess(false);
        } finally {
            setSubmitted(true);
            setLoading(false);
        }
    }

    const successWithoutEmail = success && !hasEmail;
    const successWithEmail = success && hasEmail;
    const failed = submitted && !success;

    return (
        <Dialog
            visible={visible}
            title="Feedback"
            tone="toolbar"
            denseToolbar
            persistent
            maxWidth={700}
            onRequestClose={hide}
            testID="feedback-container"
            closeTestID="messageModalCloseButton"
            actions={
                !submitted ? (
                    <>
                        <HgButton
                            testID="feedback-no-need-btn"
                            text="Cancel"
                            variant="secondary"
                            onPress={hide}
                        />
                        <HgButton
                            testID="send-feedback-message-btn"
                            text="Send Message"
                            disabled={!isValid}
                            loading={loading}
                            onPress={() => void submit()}
                        />
                    </>
                ) : successWithoutEmail ? (
                    <>
                        <HgButton
                            testID="feedback-no-need-btn"
                            text="No Need!"
                            variant="secondary"
                            onPress={hide}
                        />
                        <HgButton
                            testID="feedback-update-my-email-btn"
                            text="Update my email"
                            onPress={() => {
                                hide();
                                router.push(Path.Profile);
                            }}
                        />
                    </>
                ) : successWithEmail ? (
                    <HgButton
                        testID="feedback-got-it-btn"
                        text="Got it!"
                        onPress={hide}
                    />
                ) : (
                    <HgButton
                        text="Try Again"
                        loading={loading}
                        disabled={!isValid}
                        onPress={() => void submit()}
                    />
                )
            }
        >
            {!submitted ? (
                <>
                    <AppText className="mb-3">
                        Do you have a suggestion or idea? Let us know in the
                        field below.
                    </AppText>
                    <HgTextField
                        testID="feedback-comment-input"
                        accessibilityLabel="Your suggestion or idea"
                        value={comment}
                        onChangeText={(v) => {
                            setComment(v);
                            setTouched(true);
                        }}
                        onFocus={() => setFocused(true)}
                        onBlur={() => setFocused(false)}
                        multiline
                        numberOfLines={5}
                        placeholder="Describe your suggestion or idea..."
                        editable={!loading}
                        errorMessages={errors}
                        style={{ minHeight: 120, textAlignVertical: "top" }}
                    />
                    {/* v-textarea `counter`: the length, while the field has focus */}
                    {focused ? (
                        <AppText
                            variant="body-medium"
                            className="text-right text-bc-grey"
                        >
                            {comment.length}
                        </AppText>
                    ) : null}
                </>
            ) : (
                <View>
                    <View className="items-center">
                        <Icon
                            name={success ? "circle-check" : "circle-xmark"}
                            size={48}
                            color={success ? colors.success : colors.error}
                        />
                        <AppText
                            variant="title-large"
                            className="font-bold pt-3"
                        >
                            {success ? "Received" : "Sorry"}
                        </AppText>
                        <AppText className="py-3 text-center">
                            {success
                                ? "Your message has been sent successfully!"
                                : "Your message could not be sent out!"}
                        </AppText>
                    </View>
                    {successWithoutEmail ? (
                        <HgAlert
                            type="warning"
                            live="polite"
                            text="We won't be able to respond to your message unless you have a verified email address in your profile."
                        />
                    ) : null}
                    {failed ? (
                        <HgAlert type="error" live="assertive">
                            <AppText className="text-error">
                                If the problem persists please send your
                                feedback to{" "}
                                <AppText
                                    className="text-link"
                                    onPress={() =>
                                        void openExternal(
                                            "mailto:HealthGateway@gov.bc.ca"
                                        )
                                    }
                                >
                                    HealthGateway@gov.bc.ca
                                </AppText>
                            </AppText>
                        </HgAlert>
                    ) : null}
                </View>
            )}
        </Dialog>
    );
}
