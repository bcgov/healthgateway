import { injectable } from "inversify";
import { ICsvExportService } from "@/services/interfaces";

@injectable()
export class RestCsvExportService implements ICsvExportService {
    private readonly BASE_URI: string = "v1/api/CsvExport";
    public downloadUserInfoCSV(): void {
        this.downloadCsvExport("GetUserProfiles");
    }
    public downloadUserCommentsCSV(): void {
        this.downloadCsvExport("GetComments");
    }
    public downloadUserNotesCSV(): void {
        this.downloadCsvExport("GetNotes");
    }
    private downloadCsvExport(routeName: string) {
        window.open(`${this.BASE_URI}/${routeName}`);
    }
}
