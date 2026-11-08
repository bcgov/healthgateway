import type { InfoTile } from "@/models/infoTile";

// Port of the landing-page tiles in Apps/WebClient/src/ClientApp/src/constants/accessLinks.ts. The Access tiles follow
// the main navigation's order (stated deviation: the web client has Records management before Health services).
export enum TileSection {
    Access = "Access",
    HealthServices = "HealthServices",
    OtherRecordSources = "OtherRecordSources",
}

const ALL_TILES: InfoTile[] = [
    {
        section: TileSection.Access,
        type: "HealthRecords",
        name: "Health records",
        description:
            "View health records including immunizations, lab results, imaging reports, medications, health and hospital visits, Special Authority requests, clinical documents, BC Cancer Screening letters and more.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords",
        active: true,
    },
    {
        section: TileSection.Access,
        type: "DependentRecords",
        name: "Dependent records",
        description: "View your children’s immunization records and schedule.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/dependents",
        active: true,
    },
    {
        section: TileSection.Access,
        type: "HealthServices",
        name: "Health services",
        description: "Check and update your Organ Donor registration status.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/services",
        active: true,
    },
    {
        section: TileSection.Access,
        type: "RecordsManagement",
        name: "Records management",
        description:
            "Add your own notes in Health Gateway to help manage your health. Download health records to save or print.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/export",
        active: true,
    },
    {
        section: TileSection.HealthServices,
        type: "HealthLinkBC",
        name: "Visit HealthLink BC",
        description:
            "Explore reliable health information and services that can help you understand and manage your health.",
        link: "https://www.healthlinkbc.ca",
        active: true,
    },
    {
        section: TileSection.HealthServices,
        type: "Call811",
        name: "Call 8-1-1 for health information and advice",
        description:
            "Speak to a navigator who can guide you to reliable health information or connect you with a registered nurse, registered dietitian, qualified exercise professional, pharmacist or physician.",
        link: "https://www.healthlinkbc.ca/find-care/healthlink-bc-8-1-1-services",
        active: true,
    },
    {
        section: TileSection.HealthServices,
        type: "FindDoctor",
        name: "Find a family doctor or nurse practioner",
        description:
            "Register with the Health Connect Registry to get matched with a family doctor or nurse practitioner in your community.",
        link: "https://www.healthlinkbc.ca/find-care/health-connect-registry",
        active: true,
    },
];

// Other record sources: `logo` names the SVG in assets/images/home/other; AccessMyHealth's URL comes from
// configuration (webClient.accessMyHealthUrl), so it has no `link`.
export interface RecordSourceTile extends InfoTile {
    logo:
        | "access-my-health"
        | "island-health"
        | "interior-health"
        | "northern-health"
        | "fraser-health";
    linkText: string;
}

export const OTHER_RECORD_SOURCES: RecordSourceTile[] = [
    {
        section: TileSection.OtherRecordSources,
        type: "AccessMyHealth",
        name: "AccessMyHealth",
        description:
            "Find health records from care received at Vancouver Coastal Health, Providence Health Care and Provincial Health Services Authority. This may include upcoming appointments, lab results, imaging reports, clinical notes and more.",
        logo: "access-my-health",
        linkText: "Open AccessMyHealth",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: "MyHealth",
        name: "MyHealth",
        description:
            "Find health records from care received at Island Health facilities across Vancouver Island and Gulf Islands.  This may include diagnostic results, upcoming appointments, and provider-authored notes.",
        link: "https://www.islandhealth.ca/our-services/virtual-care-services/myhealth",
        logo: "island-health",
        linkText: "Go to MyHealth",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: "MyHealthPortal",
        name: "MyHealthPortal",
        description:
            "Find records from care received in Kelowna, Kamloops, or other areas in the B.C. Interior. This may include lab results, diagnostic imaging reports, provider reports, appointment details and visit history.",
        link: "https://www.interiorhealth.ca/myhealthportal",
        logo: "interior-health",
        linkText: "Go to MyHealthPortal",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: "HealthElife",
        name: "HealthElife",
        description:
            "Find records from care received at a Northern Health hospital or urgent care centre. This may include lab results, imaging reports, visit summaries, and upcoming appointments.",
        link: "https://www.northernhealth.ca/services/digital-health/healthelife",
        logo: "northern-health",
        linkText: "Go to HealthElife",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: "MyHealthKey",
        name: "myhealthkey",
        description:
            "Find records from care received at a Northern Health primary care clinic, such as a family doctor or nurse practitioner. This may include appointment bookings, visit notes, and reports shared with your provider.",
        link: "https://www.northernhealth.ca/services/digital-health/myhealthkey",
        logo: "northern-health",
        linkText: "Go to myhealthkey",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: "FraserHealth",
        name: "Fraser Health",
        description:
            "Fraser Health does not offer online viewing of health records. You can request Fraser records by sending a form to the site where you were treated. You can do this online, by email/fax, or in person.",
        link: "https://www.fraserhealth.ca/patients-and-visitors/request-a-health-record",
        logo: "fraser-health",
        linkText: "Request a health record",
        active: true,
    },
];
export const getOtherRecordSourcesLinks = () =>
    OTHER_RECORD_SOURCES.filter((t) => t.active);

export const getAccessLinks = () =>
    ALL_TILES.filter(
        (t) => t.section === (TileSection.Access as string) && t.active
    );
export const getHealthServicesLinks = () =>
    ALL_TILES.filter(
        (t) => t.section === (TileSection.HealthServices as string) && t.active
    );
