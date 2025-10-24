import { InfoTile } from "@/models/infoTile";

export enum TileSection {
    Access = "Access",
    ManageHealth = "ManageHealth",
    OtherRecordSources = "OtherRecordSources",
}

export enum AccessLinkType {
    AccessMyHealth = "AccessMyHealth",
    Call811 = "Call811",
    DependentRecords = "DependentRecords",
    FindDoctor = "FindDoctor",
    FraserHealth = "FraserHealth",
    HealthElife = "HealthElife",
    HealthLinkBC = "HealthLinkBC",
    HealthRecords = "HealthRecords",
    HealthServices = "HealthServices",
    MyHealth = "MyHealth",
    MyHealthKey = "MyHealthKey",
    MyHealthPortal = "MyHealthPortal",
    RecordManagement = "RecordManagement",
}

const ALL_TILES = [
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
        type: AccessLinkType.DependentRecords,
        name: "Dependent Records",
        description:
            "View your dependent's immunization records, including history and schedule.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/dependents",
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
        section: TileSection.Access,
        type: AccessLinkType.HealthServices,
        name: "Health Services",
        description:
            "Easily check and update your organ donor registry information.",
        link: "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/services",
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
    {
        section: TileSection.OtherRecordSources,
        type: AccessLinkType.AccessMyHealth,
        name: "AccessMyHealth",
        description:
            "Check your health records from care received through Provincial Health Services, Vancouver Coastal Health, or Providence Health. This includes visit summaries, test results and more.",
        link: "https://www.phsa.ca/",
        logoUri: new URL(
            "@/assets/images/home/other/access-my-health.svg",
            import.meta.url
        ).href,
        linkText: "Open Access My Health",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: AccessLinkType.MyHealth,
        name: "My Health",
        description:
            "Find health records from care received at Island Health facilities across Vancouver Island and Gulf Islands.  This may include diagnostics results, upcoming appointments, and provider-authored notes.",
        link: "https://www.islandhealth.ca/our-services/virtual-care-services/myhealth",
        logoUri: new URL(
            "@/assets/images/home/other/island-health.svg",
            import.meta.url
        ).href,
        linkText: "Go to MyHeath",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: AccessLinkType.MyHealthPortal,
        name: "MyHealth Portal",
        description:
            "Find records from care you received in Kelowna, Kamloops, or other areas in the BC Interior. This may include lab results, diagnostic imaging reports, provider reports, appointment details and visit history.",
        link: "https://www.interiorhealth.ca/myhealthportal",
        logoUri: new URL(
            "@/assets/images/home/other/interior-health.svg",
            import.meta.url
        ).href,
        linkText: "Go to MyHealth Portal",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: AccessLinkType.HealthElife,
        name: "Health Elife",
        description:
            "Find records from care you received at a Northern Health hospital or urgent care centre. This may include lab results, imaging reports, visit summaries, and upcoming appointments.",
        link: "https://www.northernhealth.ca/services/digital-health/healthelife",
        logoUri: new URL(
            "@/assets/images/home/other/northern-health.svg",
            import.meta.url
        ).href,
        linkText: "Go to HealthEfile",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: AccessLinkType.MyHealthKey,
        name: "myhealthkey",
        description:
            "Find records from care you received at a Northern Health primary care clinic, such as a family doctor or nurse practitioner. This may include appointment bookings, visit notes, and lab shared with your provider.",
        link: "https://www.northernhealth.ca/services/digital-health/healthelife",
        logoUri: new URL(
            "@/assets/images/home/other/northern-health.svg",
            import.meta.url
        ).href,
        linkText: "Go to myhealthkey",
        active: true,
    },
    {
        section: TileSection.OtherRecordSources,
        type: AccessLinkType.FraserHealth,
        name: "Fraser Health",
        description:
            "Fraser Health does not offer online viewing of health records. You can request Fraser records by sending a form to the site where you were treated. You can do this online, by email/fax, or in person.",
        link: "https://www.fraserhealth.ca/patients-and-visitors/request-a-health-record",
        logoUri: new URL(
            "@/assets/images/home/other/fraser-health.svg",
            import.meta.url
        ).href,
        linkText: "Request a health record",
        active: true,
    },
] as const satisfies ReadonlyArray<InfoTile>;

export function getTilesBySection(section: TileSection): InfoTile[] {
    return ALL_TILES.filter((t) => t.section === section && t.active);
}

export const getAccessLinks = () => getTilesBySection(TileSection.Access);
export const getManageHealthLinks = () =>
    getTilesBySection(TileSection.ManageHealth);
export const getOtherRecordSourcesLinks = () =>
    getTilesBySection(TileSection.OtherRecordSources);
