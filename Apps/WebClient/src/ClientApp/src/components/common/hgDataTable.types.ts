import type { ReportField } from "@/models/report";

export interface HgDataTableProps<T> {
    items: T[];
    fields: ReportField[];
    loading?: boolean;
    density?: "default" | "comfortable" | "compact";
    testID?: string;
    /** #header-<key> slots */
    renderHeader?: Partial<Record<string, () => React.ReactNode>>;
    /** #item-<key> slots */
    renderItem?: Partial<
        Record<string, (item: T, index: number) => React.ReactNode>
    >;
    className?: string;
    /** scroll the rows past this height with the header kept in view (fixed-header + max-height) */
    maxHeight?: number;
    /** what the table lists, for the screen reader: a hidden `<caption>` on web, hidden text above the rows on native */
    caption?: string;
}
