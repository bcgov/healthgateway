import { DateTime } from "luxon";
import { useEffect, useMemo, useState } from "react";
import { createPortal } from "react-dom";
import { Pressable, useWindowDimensions, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";
import { cn } from "@/utils/cn";
import { formatDate, fromIsoDate, pacificTimeZone } from "@/utils/dateUtil";

import { AppText } from "./AppText";
import { DateTextField } from "./DateTextField";
import {
    defaultMaxDate,
    defaultMinDate,
    type HgDatePickerProps,
} from "./hgDatePicker.shared";
import { Icon } from "./Icon";

export type { HgDatePickerProps } from "./hgDatePicker.shared";

const weekdays = ["Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"];

/** Six weeks (VueDatePicker six-weeks="append"), Monday first, covering `month`. */
function monthGrid(month: DateTime): DateTime[] {
    const first = month.startOf("month");
    const start = first.minus({ days: first.weekday - 1 });
    return Array.from({ length: 42 }, (_, i) => start.plus({ days: i }));
}

/**
 * Port of HgDatePickerComponent.vue on web: the masked "YYYY-MMM-DD" field with a calendar button that opens a
 * VueDatePicker-style month (auto-apply, Monday first, six rows, the selected day in the focus colour).
 */
export function HgDatePicker(props: HgDatePickerProps) {
    const colors = useThemeColors();
    const { width, height } = useWindowDimensions();
    const [popupHeight, setPopupHeight] = useState(0);
    const {
        value = "",
        onChange,
        minDate = defaultMinDate(),
        maxDate = defaultMaxDate(),
        onBlur,
        onValidityChange,
    } = props;
    const selected = value ? fromIsoDate(value) : undefined;
    const [open, setOpen] = useState(false);
    const [anchor, setAnchor] = useState<HTMLElement | null>(null);
    const [month, setMonth] = useState(() =>
        (selected ?? DateTime.local({ zone: pacificTimeZone })).startOf("month")
    );
    const days = useMemo(() => monthGrid(month), [month]);

    // a click outside the calendar closes it
    useEffect(() => {
        if (!open) return;
        const close = () => setOpen(false);
        const t = setTimeout(() =>
            document.addEventListener("click", close, { once: true })
        );
        return () => {
            clearTimeout(t);
            document.removeEventListener("click", close);
        };
    }, [open]);

    // Escape closes the calendar, as VueDatePicker does, and only the calendar: react-native-web closes a dialog on
    // Escape's keyup at the document, so the keyup is taken first, in the capture phase, and goes no further. Focus
    // returns to the calendar button.
    useEffect(() => {
        if (!open) return;
        function onKeyup(event: KeyboardEvent): void {
            if (event.key !== "Escape") return;
            event.stopPropagation();
            setOpen(false);
            anchor?.focus();
        }
        globalThis.addEventListener("keyup", onKeyup, true);
        return () => globalThis.removeEventListener("keyup", onKeyup, true);
    }, [open, anchor]);

    const rect = open ? anchor?.getBoundingClientRect() : undefined;
    const popupWidth = 280;
    const left = rect
        ? Math.max(8, Math.min(rect.right - popupWidth, width - popupWidth - 8))
        : 0;
    // below the field, or above it when the window has no room below (VueDatePicker flips the same way)
    const below = rect ? rect.bottom + 16 : 0;
    const top =
        rect && below + popupHeight > height - 8
            ? Math.max(8, rect.top - 16 - popupHeight)
            : below;

    return (
        <>
            <DateTextField
                {...props}
                calendar={
                    <Pressable
                        ref={(node) =>
                            setAnchor(node as unknown as HTMLElement | null)
                        }
                        accessibilityRole="button"
                        accessibilityLabel="Open calendar"
                        onPress={() => {
                            if (selected) setMonth(selected.startOf("month"));
                            setOpen((v) => !v);
                        }}
                        className="w-11 h-11 items-center justify-center rounded-full hover:bg-surface-hover"
                    >
                        <Icon
                            name="calendar"
                            size={18}
                            color={colors["bc-black"]}
                        />
                    </Pressable>
                }
            />
            {open && rect
                ? createPortal(
                      <View
                          testID="date-picker-menu"
                          onLayout={(e) =>
                              setPopupHeight(e.nativeEvent.layout.height)
                          }
                          accessibilityRole="none"
                          onStartShouldSetResponder={() => true}
                          {...({
                              onClick: (e: Event) => e.stopPropagation(),
                          } as object)}
                          className="bg-surface rounded border border-border-light shadow-lg p-2"
                          style={
                              {
                                  position: "fixed",
                                  top,
                                  left,
                                  width: popupWidth,
                                  zIndex: 2000,
                              } as object
                          }
                      >
                          <View className="flex-row items-center justify-between mb-2">
                              <Pressable
                                  accessibilityRole="button"
                                  accessibilityLabel="Previous month"
                                  onPress={() =>
                                      setMonth((m) => m.minus({ months: 1 }))
                                  }
                                  className="w-8 h-8 items-center justify-center rounded hover:bg-surface-hover"
                              >
                                  <Icon
                                      name="chevron-left"
                                      size={12}
                                      color={colors["bc-black"]}
                                  />
                              </Pressable>
                              <AppText className="font-bold">
                                  {month.toFormat("LLLL yyyy", {
                                      locale: "en-US",
                                  })}
                              </AppText>
                              <Pressable
                                  accessibilityRole="button"
                                  accessibilityLabel="Next month"
                                  onPress={() =>
                                      setMonth((m) => m.plus({ months: 1 }))
                                  }
                                  className="w-8 h-8 items-center justify-center rounded hover:bg-surface-hover"
                              >
                                  <Icon
                                      name="chevron-right"
                                      size={12}
                                      color={colors["bc-black"]}
                                  />
                              </Pressable>
                          </View>
                          <View className="flex-row">
                              {weekdays.map((d) => (
                                  <AppText
                                      key={d}
                                      variant="body-medium"
                                      className="flex-1 text-center text-bc-grey"
                                  >
                                      {d}
                                  </AppText>
                              ))}
                          </View>
                          <View className="flex-row flex-wrap">
                              {days.map((day) => {
                                  const inMonth = day.month === month.month;
                                  const disabled =
                                      day < minDate.startOf("day") ||
                                      day > maxDate.endOf("day");
                                  const isSelected =
                                      selected?.hasSame(day, "day") ?? false;
                                  return (
                                      <Pressable
                                          key={day.toISODate()}
                                          accessibilityRole="button"
                                          accessibilityLabel={formatDate(day)}
                                          disabled={disabled}
                                          onPress={() => {
                                              onChange(
                                                  formatDate(day, "yyyy-MM-dd")
                                              );
                                              onBlur?.();
                                              onValidityChange?.(true);
                                              setOpen(false);
                                          }}
                                          style={{ width: `${100 / 7}%` }}
                                          className={cn(
                                              "h-9 items-center justify-center rounded",
                                              isSelected
                                                  ? "bg-focus"
                                                  : "hover:bg-surface-hover",
                                              disabled && "opacity-40"
                                          )}
                                      >
                                          <AppText
                                              variant="body-medium"
                                              className={cn(
                                                  isSelected
                                                      ? "text-on-primary"
                                                      : !inMonth &&
                                                            "text-bc-grey"
                                              )}
                                          >
                                              {day.day}
                                          </AppText>
                                      </Pressable>
                                  );
                              })}
                          </View>
                      </View>,
                      // a transformed ancestor (react-native-web's ScrollView) would anchor and clip a fixed popup, so
                      // it renders at the modal's root, or the body outside a dialog
                      anchor?.closest('[aria-modal="true"]') ?? document.body
                  )
                : null}
        </>
    );
}
