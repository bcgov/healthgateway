import { injectable } from "inversify";
import { ICsvExportService } from "@/services/interfaces";

@injectable()
export class RestCsvExportService implements ICsvExportService {
    private readonly BASE_URI: string = "v1/api/CsvExport";
    public getUserProfilesExportUrl(): string {
        return `${this.BASE_URI}/GetUserProfiles`;
    }
    public getUserNotesExportUrl(): string {
        return `${this.BASE_URI}/GetNotes`;
    }
    public getUserCommentsExportUrl(): string {
        return `${this.BASE_URI}/GetComments`;
    }
}
