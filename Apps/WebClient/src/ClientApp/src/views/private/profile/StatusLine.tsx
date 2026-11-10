import { AppText } from "@/components/common";

/**
 * DisplayFieldComponent (horizontal) for "Status: Verified / Not Verified / Opted Out", inside the
 * `{prefix}Status` wrapper the web client gives its status block.
 */
export function StatusLine({
    verified,
    hasValue,
    testIDPrefix,
    nativeID,
}: {
    verified: boolean;
    hasValue: boolean;
    testIDPrefix: string;
    /** HTML id the web client sets (email only) */
    nativeID?: string;
}) {
    const [text, cls, id] = verified
        ? ["Verified", "text-success", `${testIDPrefix}StatusVerified`]
        : !hasValue
          ? ["Opted Out", "text-bc-grey", `${testIDPrefix}StatusOptedOut`]
          : ["Not Verified", "text-error", `${testIDPrefix}StatusNotVerified`];
    return (
        <AppText
            className="mb-4"
            testID={`${testIDPrefix}Status`}
            nativeID={nativeID}
        >
            Status:{" "}
            <AppText className={cls} testID={id}>
                {text}
            </AppText>
        </AppText>
    );
}
