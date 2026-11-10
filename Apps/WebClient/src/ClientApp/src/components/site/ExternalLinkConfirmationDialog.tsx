import { View } from "react-native";

import { AppText, Dialog, HgButton } from "@/components/common";
import { Action, Origin, type Text } from "@/constants/tracking";
import { services } from "@/services/container";
import { openExternal } from "@/utils/openExternal";

export type DialogBodyLine =
    | string
    | {
          prefix?: string;
          text: string;
          trackingText: Text;
          href: string;
          suffix?: string;
      };

interface Props {
    visible: boolean;
    origin: Origin;
    title: string;
    body: DialogBodyLine[];
    confirmLabel?: string;
    cancelLabel?: string;
    onConfirm: () => void;
    onCancel: () => void;
}

/** Port of ExternalLinkConfirmationDialog.vue */
export function ExternalLinkConfirmationDialog({
    visible,
    origin,
    title,
    body,
    confirmLabel = "Proceed",
    cancelLabel = "Cancel",
    onConfirm,
    onCancel,
}: Props) {
    return (
        <Dialog
            visible={visible}
            title={title}
            onRequestClose={onCancel}
            testID="external-link-confirmation-dialog"
            actions={
                <>
                    <HgButton
                        testID="external-link-confirmation-dialog-cancel-button"
                        text={cancelLabel}
                        variant="secondary"
                        onPress={onCancel}
                    />
                    <HgButton
                        testID="external-link-confirmation-dialog-proceed-button"
                        text={confirmLabel}
                        onPress={onConfirm}
                    />
                </>
            }
        >
            <View>
                {body.map((line, i) =>
                    typeof line === "string" ? (
                        <AppText
                            key={i}
                            className="mb-3"
                            testID={`external-link-confirmation-dialog-body-${i}`}
                        >
                            {line}
                        </AppText>
                    ) : (
                        <AppText
                            key={i}
                            className="mb-3"
                            testID={`external-link-confirmation-dialog-body-${i}`}
                        >
                            {line.prefix ?? ""}
                            <AppText
                                className="text-link underline"
                                onPress={() => {
                                    services.tracking.trackEvent({
                                        action: Action.ExternalLink,
                                        text: line.trackingText,
                                        origin,
                                        destination: line.href,
                                        url: line.href,
                                    });
                                    void openExternal(line.href);
                                }}
                            >
                                {line.text}
                            </AppText>
                            {line.suffix ?? ""}
                        </AppText>
                    )
                )}
            </View>
        </Dialog>
    );
}
