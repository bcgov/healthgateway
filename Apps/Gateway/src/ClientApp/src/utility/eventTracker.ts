import { EntryType, entryTypeMap } from "@/constants/entryType";

import SnowPlow from "./snowPlow";

export default abstract class EventTracker {
    public static loadData(loadEntryType: EntryType, entryCount: number): void {
        if (entryCount === 0) {
            return;
        }

        const loadType = entryTypeMap.get(loadEntryType)?.eventName ?? "";

        if (loadType !== "") {
            SnowPlow.trackEvent({
                action: "load_data",
                text: loadType,
            });
        }
    }

    public static downloadReport(eventName: string): void {
        SnowPlow.trackEvent({
            action: "download_report",
            text: eventName,
        });
    }
}
