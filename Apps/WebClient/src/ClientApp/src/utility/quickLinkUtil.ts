import { QuickLink } from "@/models/quickLink";

export abstract class QuickLinkUtil {
    public static toString(quickLinks: QuickLink[]): string {
        return JSON.stringify(quickLinks);
    }

    public static toQuickLinks(jsonString: string): QuickLink[] {
        return JSON.parse(jsonString);
    }
}
