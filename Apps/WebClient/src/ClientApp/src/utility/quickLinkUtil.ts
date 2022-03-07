import { QuickLink, QuickLinkInformation } from "@/models/quickLink";
import { ClientModule } from "@/router";

const medicationDescription =
    "View your prescription medication records. Vaccines received in a pharmacy may be recorded as a prescription medication.";

const healthVisitDescription =
    "View the last seven years of your health visits, consultations and procedures billed to the BC Medical Services Plan.";

const covid19TestDescription =
    "View and download your COVID‑19 test results as soon as they are available.";

const laboratoryDescription =
    "View your lab results. Most results are available within 24–48 hours.";

const immunizationDescription =
    "View your immunizations from public health. Vaccines received in a pharmacy may be recorded as a prescription medication.";

const specialAuthorityDescription =
    "View the status of your Special Authority drug coverage requests made since March 2021.";

const noteDescription =
    "View and edit notes that you added to your medical records.";

const primaryIconStyle = "module-colour-primary";
const secondaryIconStyle = "module-colour-secondary";

export abstract class QuickLinkUtil {
    public static toString(quickLinks: QuickLink[]): string {
        return JSON.stringify(quickLinks);
    }

    public static toQuickLinks(jsonString: string): QuickLink[] {
        return JSON.parse(jsonString);
    }

    public static getInformation(
        quickLink: QuickLink,
        index: number
    ): QuickLinkInformation {
        const defaultInformation = {
            index,
            title: quickLink.name,
            description: "View your filtered health records.",
            icon: "search",
            iconStyle: primaryIconStyle,
        };

        const modules = quickLink.filter.modules;
        if (modules.length === 1) {
            switch (modules[0]) {
                case ClientModule.Medication:
                    return {
                        index,
                        title: "Medications",
                        description: medicationDescription,
                        icon: "pills",
                        iconStyle: primaryIconStyle,
                    };
                case ClientModule.Encounter:
                    return {
                        index,
                        title: "Health Visits",
                        description: healthVisitDescription,
                        icon: "user-md",
                        iconStyle: primaryIconStyle,
                    };
                case ClientModule.Laboratory:
                    return {
                        index,
                        title: "COVID‑19 Tests",
                        description: covid19TestDescription,
                        icon: "flask",
                        iconStyle: primaryIconStyle,
                    };
                case ClientModule.AllLaboratory:
                    return {
                        index,
                        title: "Lab Results",
                        description: laboratoryDescription,
                        icon: "vial",
                        iconStyle: primaryIconStyle,
                    };
                case ClientModule.Immunization:
                    return {
                        index,
                        title: "Immunizations",
                        description: immunizationDescription,
                        icon: "syringe",
                        iconStyle: primaryIconStyle,
                    };
                case ClientModule.MedicationRequest:
                    return {
                        index,
                        title: "Special Authority Requests",
                        description: specialAuthorityDescription,
                        icon: "clipboard",
                        iconStyle: primaryIconStyle,
                    };
                case ClientModule.Note:
                    return {
                        index,
                        title: "My Notes",
                        description: noteDescription,
                        icon: "edit",
                        iconStyle: secondaryIconStyle,
                    };
            }
        }

        return defaultInformation;
    }
}
