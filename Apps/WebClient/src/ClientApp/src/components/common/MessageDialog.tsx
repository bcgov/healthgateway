import { AppText } from "./AppText";
import { Dialog } from "./Dialog";
import { HgButton } from "./HgButton";

interface Props {
    visible: boolean;
    title: string;
    message: string;
    /** Close, Cancel or OK: hides the dialog (the web client's `cancel`) */
    onClose: () => void;
    /** Continue (not okOnly): hides the dialog, then this runs (the web client's `submit`) */
    onSubmit?: () => void;
    /** one primary OK button instead of Cancel + submit (MessageModalComponent okOnly) */
    okOnly?: boolean;
    submitText?: string;
}

/**
 * Port of MessageModalComponent.vue: primary title bar with close, one message, then OK (okOnly) or
 * Cancel + Continue.
 */
export function MessageDialog({
    visible,
    title,
    message,
    onClose,
    onSubmit,
    okOnly = false,
    submitText = "Continue",
}: Props) {
    return (
        <Dialog
            visible={visible}
            title={title}
            tone="primary"
            persistent
            maxWidth={700}
            onRequestClose={onClose}
            testID="generic-message-modal"
            closeTestID="message-modal-close-button"
            actions={
                <>
                    <HgButton
                        testID="generic-message-ok-btn"
                        variant={okOnly ? "primary" : "secondary"}
                        text={okOnly ? "OK" : "Cancel"}
                        onPress={onClose}
                    />
                    {!okOnly ? (
                        <HgButton
                            testID="generic-message-submit-btn"
                            className="ml-2"
                            text={submitText}
                            onPress={() => {
                                onClose();
                                onSubmit?.();
                            }}
                        />
                    ) : null}
                </>
            }
        >
            <AppText testID="generic-message-text">{message}</AppText>
        </Dialog>
    );
}
