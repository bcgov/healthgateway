import { InfoTile } from "@/models/infoTile";

export enum TileSection {
    Access = "Access",
    ManageHealth = "ManageHealth",
}

export enum AccessLinkType {
    Call811 = "Call811",
    DependentRecords = "DependentRecords",
    FindDoctor = "FindDoctor",
    HealthLinkBC = "HealthLinkBC",
    HealthRecords = "HealthRecords",
    HealthServices = "HealthServices",
    RecordManagement = "RecordManagement",
}

const ALL_TILES = [
    {
        section: TileSection.Access,
        type: AccessLinkType.DependentRecords,
        name: "Dependent Records",
        description:
            "View your dependent's immunization records, including history and schedule.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/dependents",
        active: true,
    },
    {
        section: TileSection.Access,
        type: AccessLinkType.HealthRecords,
        name: "Health Records",
        description:
            "View your available health records, including dispensed medications, health visits, lab results, immunizations, and more.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords",
        active: true,
    },
    {
        section: TileSection.Access,
        type: AccessLinkType.HealthServices,
        name: "Health Services",
        description:
            "Easily check and update your organ donor registry information.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/services",
        active: true,
    },
    {
        section: TileSection.Access,
        type: AccessLinkType.RecordManagement,
        name: "Record Management",
        description:
            "Download your health records, organize them, and print them. Make your own notes to track health events.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/export",
        active: true,
    },
    {
        section: TileSection.ManageHealth,
        type: AccessLinkType.Call811,
        name: "Call 8-1-1 for Health Advice",
        description:
            "Speak to a navigator who can guide you to reliable health information or connect you with a health professional.",
        link: "https://www.healthlinkbc.ca/find-care/healthlink-bc-8-1-1-services",
        active: true,
    },
    {
        section: TileSection.ManageHealth,
        type: AccessLinkType.HealthLinkBC,
        name: "Visit HealthLink BC",
        description:
            "Explore reliable health information and services that can help you understand and manage your health.",
        link: "https://www.healthlinkbc.ca",
        active: true,
    },
    {
        section: TileSection.ManageHealth,
        type: AccessLinkType.FindDoctor,
        name: "Find a family doctor or nurse practitioner",
        description:
            "Register with the Health Connect Registry to get matched with a family doctor or nurse practitioner in your community.",
        link: "https://www.healthlinkbc.ca/find-care/health-connect-registry",
        active: true,
    },
] as const satisfies ReadonlyArray<InfoTile>;

export function getTilesBySection(section: TileSection): InfoTile[] {
    return ALL_TILES.filter((t) => t.section === section && t.active);
}

export const getAccessLinks = () => getTilesBySection(TileSection.Access);
export const getManageHealthLinks = () =>
    getTilesBySection(TileSection.ManageHealth);
