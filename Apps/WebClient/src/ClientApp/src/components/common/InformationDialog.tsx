import { Action } from "@/constants/tracking";
import type {
    InformationModalContent,
    InformationModalLinkSegment,
} from "@/models/informationModal";
import { services } from "@/services/container";

import { AppText } from "./AppText";
import { Dialog } from "./Dialog";
import { ExternalLink } from "./ExternalLink";
import { HgButton } from "./HgButton";

interface Props {
    visible: boolean;
    content: InformationModalContent;
    onClose: () => void;
    /** ok-only: one primary OK button (the only mode the web client uses today) */
    okText?: string;
}

function track(segment: InformationModalLinkSegment) {
    if (!segment.analyticsText) return;
    services.tracking.trackEvent({
        action: Action.ExternalLink,
        text: segment.analyticsText,
        origin: segment.analyticsOrigin,
        destination: segment.analyticsDestination,
        type: segment.analyticsType,
        url: segment.analysticsUrl,
    });
}

/**
 * Port of InformationModalComponent.vue (ok-only): primary title bar with a close button, paragraphs of
 * text / bold / link segments, and an OK button. Persistent like the v-dialog: only Close / OK dismiss it.
 */
export function InformationDialog({
    visible,
    content,
    onClose,
    okText = "OK",
}: Props) {
    return (
        <Dialog
            visible={visible}
            title={content.title}
            tone="primary"
            persistent
            maxWidth={700}
            onRequestClose={onClose}
            testID="information-modal"
            closeTestID="message-modal-close-button"
            actions={
                <HgButton
                    testID="generic-message-ok-btn"
                    text={okText}
                    onPress={onClose}
                />
            }
        >
            {content.paragraphs.map((paragraph, p) => (
                <AppText
                    key={p}
                    testID={`information-modal-paragraph-${p}`}
                    className={
                        p === content.paragraphs.length - 1 ? "" : "mb-4"
                    }
                >
                    {paragraph.segments.map((segment, s) =>
                        segment.type === "text" ? (
                            <AppText key={s}>{segment.value}</AppText>
                        ) : segment.type === "bold" ? (
                            <AppText key={s} className="font-bold">
                                {segment.value}
                            </AppText>
                        ) : (
                            <ExternalLink
                                key={s}
                                testID={`information-modal-link-${p}-${s}`}
                                href={segment.href}
                                onPress={() => track(segment)}
                            >
                                {segment.text}
                            </ExternalLink>
                        )
                    )}
                </AppText>
            ))}
        </Dialog>
    );
}
