import type { ReportField } from "@/models/report";
import { cn } from "@/utils/cn";

import type { HgDataTableProps } from "./hgDataTable.types";
import { TableSkeleton } from "./TableSkeleton";

export type { HgDataTableProps } from "./hgDataTable.types";

const textSize = {
    compact: "text-body-small",
    comfortable: "text-body-medium",
    default: "text-body-large",
} as const;

// v-table densities (spacer 4px × -4 / -2 / 0): header 40 / 48 / 56px, rows 36 / 44 / 52px; cells padded 0 16px
const headerHeight = { compact: 40, comfortable: 48, default: 56 } as const;
const rowHeight = { compact: 36, comfortable: 44, default: 52 } as const;

const headerAlign = (a?: string) =>
    a === "start" ? "text-left" : a === "end" ? "text-right" : "text-center";
const cellAlign = (a?: string) =>
    a === "start"
        ? "text-left"
        : a === "center"
          ? "text-center"
          : a === "end"
            ? "text-right"
            : "";

/**
 * Port of HgDataTableComponent.vue on web: a real `<table>` (v-table with `table-layout: fixed`, centred
 * text), so `[data-testid=…] tbody tr` selectors and screen readers see a table.
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
    const width = (f: ReportField) =>
        f.width !== undefined ? { width: f.width } : undefined;
    return (
        <div
            data-testid={testID}
            style={
                maxHeight !== undefined
                    ? { maxHeight, overflow: "auto" }
                    : undefined
            }
            className={cn(
                "text-center font-sans text-body-medium leading-[1.5] text-bc-black",
                className
            )}
        >
            {loading ? (
                <TableSkeleton />
            ) : (
                <table
                    className="w-[99%] border-collapse"
                    style={{ tableLayout: "fixed" }}
                >
                    {/* the table's name for the screen reader (WCAG 1.3.1); the page shows none, as the web client */}
                    {caption ? (
                        <caption className="sr-only">{caption}</caption>
                    ) : null}
                    <thead
                        style={
                            maxHeight !== undefined
                                ? { position: "sticky", top: 0, zIndex: 1 }
                                : undefined
                        }
                        className={
                            maxHeight !== undefined ? "bg-surface" : undefined
                        }
                    >
                        <tr>
                            {fields.map((f) => (
                                <th
                                    key={f.key}
                                    scope="col"
                                    data-testid={f.thTestID}
                                    // weight 500 = BC Sans Regular (the web client loads 300 / 400 / 700 only)
                                    className={cn(
                                        "px-4 font-sans font-normal text-body-medium border-b border-border-divider",
                                        headerAlign(f.thAlign)
                                    )}
                                    style={{
                                        height: headerHeight[density],
                                        ...width(f),
                                    }}
                                >
                                    {renderHeader[f.key]
                                        ? renderHeader[f.key]?.()
                                        : f.title}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody className={textSize[density]}>
                        {items.map((item, i) => (
                            <tr key={i} className="hover:bg-surface-hover">
                                {fields.map((f) => (
                                    <td
                                        key={f.key}
                                        data-testid={
                                            typeof f.tdTestID === "function"
                                                ? f.tdTestID(i)
                                                : f.tdTestID
                                        }
                                        // gridlines="horizontal": a line between rows, none under the last
                                        className={cn(
                                            "px-4",
                                            i < items.length - 1 &&
                                                "border-b border-border-divider",
                                            cellAlign(f.tdAlign)
                                        )}
                                        style={{
                                            height: rowHeight[density],
                                            whiteSpace: f.tdNoWrap
                                                ? "nowrap"
                                                : undefined,
                                        }}
                                    >
                                        {renderItem[f.key]
                                            ? renderItem[f.key]?.(item, i)
                                            : String(
                                                  (item[f.key] as
                                                      | string
                                                      | number
                                                      | null
                                                      | undefined) ?? ""
                                              )}
                                    </td>
                                ))}
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}
