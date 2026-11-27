import { Platform } from "react-native";

import { ReportFormatType } from "@/models/report";

import { HgMenu } from "./HgMenu";

const formats = [
    ReportFormatType.PDF,
    ReportFormatType.CSV,
    ReportFormatType.XLSX,
];

interface Props {
    /** test id on the menu list (web) */
    testID?: string;
    /** test id on each format's menu item (web) */
    itemTestID?: (type: ReportFormatType) => string;
    onPick: (type: ReportFormatType) => void;
    /** the Download button: `onPress` opens the format menu on web and picks PDF on native; `hasMenu` is true on web */
    children: (onPress: () => void, hasMenu: boolean) => React.ReactNode;
}

/**
 * A Download button's report formats. Web: the web client's PDF / CSV / XLSX menu. Native: PDF straight away, as the
 * file opens in the platform's viewer (saveFile.ts), which a spreadsheet may have no app for.
 */
export function ReportFormatMenu({
    testID,
    itemTestID,
    onPick,
    children,
}: Props) {
    if (Platform.OS !== "web")
        return <>{children(() => onPick(ReportFormatType.PDF), false)}</>;
    return (
        <HgMenu
            testID={testID}
            items={formats.map((type) => ({
                title: type,
                testID: itemTestID?.(type),
                onPress: () => onPick(type),
            }))}
        >
            {(open) => children(open, true)}
        </HgMenu>
    );
}
