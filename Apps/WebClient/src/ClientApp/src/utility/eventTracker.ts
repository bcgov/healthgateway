import { EntryType } from "@/models/timelineEntry";

import SnowPlow from "./snowPlow";

export default abstract class EventTracker {
    public static loadData(loadEntryType: EntryType, entryCount: number): void {
        if (entryCount === 0) {
            return;
        }

        let loadType = "";
        switch (loadEntryType) {
            case EntryType.Medication:
                loadType = "medications";
                break;
            case EntryType.MedicationRequest:
                loadType = "special_authority";
                break;
            case EntryType.Immunization:
                loadType = "immunizations";
                break;
            case EntryType.Laboratory:
                loadType = "covid_test";
                break;
            case EntryType.Encounter:
                loadType = "health_visits";
                break;
        }

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
