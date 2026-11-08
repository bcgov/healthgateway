import { AppText, HgAlert } from "@/components/ui";

/** Native: says what a page can't show or do while the session banner's reason lasts. */
export function OfflineNotice({
    testID,
    children,
}: {
    testID: string;
    children: string;
}) {
    return (
        <HgAlert type="info" testID={testID}>
            <AppText>{children}</AppText>
        </HgAlert>
    );
}
