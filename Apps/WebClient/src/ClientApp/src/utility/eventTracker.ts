import { EntryType } from "@/models/timelineEntry";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

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
            case EntryType.Covid19LaboratoryOrder:
                loadType = "covid_test";
                break;
            case EntryType.LaboratoryOrder:
                loadType = "all_laboratory";
                break;
            case EntryType.Encounter:
                loadType = "health_visits";
                break;
        }

        if (loadType !== "") {
            const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
            logger.debug(`Tracking event: loadData - ${loadType}`);
            SnowPlow.trackEvent({
                action: "load_data",
                text: loadType,
            });
        }
    }

    public static downloadReport(eventName: string): void {
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        logger.debug(`Tracking event: downloadReport - ${eventName}`);
        SnowPlow.trackEvent({
            action: "download_report",
            text: eventName,
        });
    }
}
