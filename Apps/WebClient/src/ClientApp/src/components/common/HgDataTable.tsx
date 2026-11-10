import { ScrollView, View } from "react-native";

import type { ReportField } from "@/models/report";
import { cn } from "@/utils/cn";

import { AppText } from "./AppText";
import type { HgDataTableProps } from "./hgDataTable.types";
import { VisuallyHidden } from "./LiveRegion";
import { TableSkeleton } from "./TableSkeleton";

export type { HgDataTableProps } from "./hgDataTable.types";

const textSize = {
    compact: "text-body-small",
    comfortable: "text-body-medium",
    default: "text-body-large",
} as const;

// v-table densities: header 40 / 48 / 56px, rows 36 / 44 / 52px
const headerHeight = { compact: 40, comfortable: 48, default: 56 } as const;
const rowHeight = { compact: 36, comfortable: 44, default: 52 } as const;

/** A plain cell's text */
const cellText = (item: Record<string, unknown>, f: ReportField) =>
    String((item[f.key] as string | number | null | undefined) ?? "");

const align = (a?: "start" | "end" | "center", header = false) =>
    a === "start"
        ? "text-left"
        : a === "end"
          ? "text-right"
          : a === "center" || header
            ? "text-center"
            : "";

/**
 * Port of HgDataTableComponent.vue (v-table, table-layout fixed, centred header text). Native: rows of
 * Views with the same column widths; the web variant renders a real `<table>`. Views have no table semantics, so each
 * plain cell is named "column: value" and the header cells are headings (WCAG 1.3.1); custom cells keep their own
 * content, so a button in one stays reachable.
 */
export function HgDataTable<T extends Record<string, unknown>>({
    items,
    fields,
    loading = false,
    density = "default",
    testID,
    renderHeader = {},
    renderItem = {},
    className,
    maxHeight,
    caption,
}: HgDataTableProps<T>) {
    const colStyle = (f: ReportField) =>
        f.width !== undefined
            ? { width: f.width as `${number}%` }
            : { flex: 1 };
    return (
        <View testID={testID} className={className}>
            {loading ? (
                <TableSkeleton />
            ) : (
                <>
                    {caption ? (
                        <VisuallyHidden>{caption}</VisuallyHidden>
                    ) : null}
                    <View
                        className="flex-row border-b border-border-divider"
                        style={{ minHeight: headerHeight[density] }}
                    >
                        {fields.map((f) => (
                            <View
                                key={f.key}
                                testID={f.thTestID}
                                className="px-4 justify-center"
                                style={colStyle(f)}
                            >
                                {renderHeader[f.key] ? (
                                    renderHeader[f.key]?.()
                                ) : (
                                    <AppText
                                        variant="body-medium"
                                        accessibilityRole="header"
                                        className={align(f.thAlign, true)}
                                    >
                                        {f.title}
                                    </AppText>
                                )}
                            </View>
                        ))}
                    </View>
                    <ScrollView
                        style={
                            maxHeight !== undefined ? { maxHeight } : undefined
                        }
                        scrollEnabled={maxHeight !== undefined}
                        nestedScrollEnabled
                    >
                        {items.map((item, i) => (
                            <View
                                key={i}
                                // gridlines="horizontal": a line between rows, none under the last
                                className={cn(
                                    "flex-row",
                                    i < items.length - 1 &&
                                        "border-b border-border-divider"
                                )}
                                style={{ minHeight: rowHeight[density] }}
                            >
                                {fields.map((f) => (
                                    <View
                                        key={f.key}
                                        testID={
                                            typeof f.tdTestID === "function"
                                                ? f.tdTestID(i)
                                                : f.tdTestID
                                        }
                                        className="px-4 py-2 justify-center"
                                        style={colStyle(f)}
                                    >
                                        {renderItem[f.key] ? (
                                            renderItem[f.key]?.(item, i)
                                        ) : (
                                            <AppText
                                                className={cn(
                                                    textSize[density],
                                                    align(f.tdAlign)
                                                )}
                                                numberOfLines={
                                                    f.tdNoWrap ? 1 : undefined
                                                }
                                                accessibilityLabel={`${f.title}: ${cellText(item, f)}`}
                                            >
                                                {cellText(item, f)}
                                            </AppText>
                                        )}
                                    </View>
                                ))}
                            </View>
                        ))}
                    </ScrollView>
                </>
            )}
        </View>
    );
}
