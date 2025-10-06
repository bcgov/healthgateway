export interface SnowplowWindow extends Window {
    snowplow(eventName: string): void;

    snowplow(eventName: string, data: SnowPlowEvent): void;
}

export interface SnowPlowEvent {
    schema: string;
    data: EventData;
}

export interface EventData {
    action: Action;
    text: Text | string;
    actor?: Actor;
    dataset?: Dataset;
    destination?: Destination | string;
    format?: Format;
    origin?: Origin;
    rating?: Rating;
    type?: Type;
    url?: string | InternalUrl | ExternalUrl;
}

export const enum Action {
    Download = "Download",
    Load = "Load",
    Submit = "Submit",
    View = "View",
    Visit = "Visit",
    ButtonClick = "button_click",
    InternalLink = "internal_link",
    ExternalLink = "external_link",
    TimelineCardClick = "timeline_card_click",
}

export const enum Actor {
    Guardian = "Guardian",
    User = "User",
}

export const enum Dataset {
    BcCancer = "BC Cancer",
    BcCancerScreening = "BC Cancer Screening",
    ClinicalDocuments = "Clinical Documents",
    Covid19Tests = "COVID-19 Tests",
    HealthVisits = "Health Visits",
    HospitalVisits = "Hospital Visits",
    ImagingReports = "Imaging Reports",
    Immunizations = "Immunizations",
    LabResults = "Lab Results",
    Notes = "Notes",
    Medications = "Medications",
    SpecialAuthorityRequests = "Special Authority Requests",
}

export const enum Destination {
    BcGovImmunizations = "BC Gov Immunizations",
    BcVaccineCard = "BC Vaccine Card",
    BookVaccine = "Book a Vaccine",
    HealthGatewayBeta = "Health Gateway Beta",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    OrganDonorRegistration = "Organ Donor Registration",
    PrimaryCare = "Primary Care",
    PublicHealthImmunizationSchedule = "Public Health Immunization Schedule",
    Dependents = "Dependents",
    Download = "Download",
    Export = "Export",
    HealthLinkBC = "HealthLink BC",
    Services = "Services",
    Timeline = "Timeline",
}

export const enum Format {
    Csv = "CSV",
    Image = "Image",
    Pdf = "PDF",
    XLSX = "XLSX",
}

export const enum Origin {
    Dependents = "Dependents",
    Exports = "Exports",
    Home = "Home",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
}

export const enum Rating {
    Skip = "Skip",
    One = "1",
    Two = "2",
    Three = "3",
    Four = "4",
    Five = "5",
}

export const enum Text {
    AppRating = "App Rating",
    Data = "Data",
    Document = "Document",
    Export = "Export",
    ExternalLink = "External Link",
    InternalLink = "Internal Link",
    Page = "Page",
    Request = "Request",
    BcVaccineCard = "BC Vaccine Card",
    HealthRecords = "Health Records",
    HealthLinkBC = "HealthLinkBC",
    Immunizations = "View Recommend Immunizations",
    ImmunizationScheduleExport = "Immunization Schedule Export",
    ImmunizationScheduleDependents = "Immunization Schedule Dependents",
    OrganDonor = "Organ Donor",
    ViewProofOfVaccination = "View Proof of Vaccination",
    DownloadProofOfVaccination = "Download Proof of Vaccination",
    QuickLink = "Quicklink",
}

export const enum Type {
    Covid19ProofOfVaccination = "COVID-19 Proof of Vaccination",
    OrganDonorRegistration = "Organ Donor Registration",
    PublicCovid19ProofOfVaccination = "Public COVID-19 Proof of Vaccination",
    Recall = "Recall",
    Result = "Result",
    BcCancerScreening = "BC Cancer Screening",
    HomeTile = "Home Tile",
    InfoBanner = "Info Banner",
}

export const BcCancerPrograms = ["Breast", "Cervix", "Colon", "Lung"] as const;
export type BcCancerProgram = (typeof BcCancerPrograms)[number];

export const BcCancerDestination: Record<BcCancerProgram, string> = {
    Breast: "Breast Cancer Screening",
    Cervix: "Cervix Cancer Screening",
    Colon: "Colon Cancer Screening",
    Lung: "Lung Cancer Screening",
} as const;

export const BcCancerUrl: Record<BcCancerProgram, string> = {
    Breast: "http://www.bccancer.bc.ca/screening/breast",
    Cervix: "http://www.bccancer.bc.ca/screening/cervix",
    Colon: "http://www.bccancer.bc.ca/screening/colon",
    Lung: "http://www.bccancer.bc.ca/screening/lung",
} as const;

export function isBcCancerProgram(x: string): x is BcCancerProgram {
    return (BcCancerPrograms as readonly string[]).includes(x);
}

export const enum ExternalUrl {
    HealthLinkBC = "https://www.healthlinkbc.ca/find-care/health-connect-registry",
}

export const enum InternalUrl {
    Timeline = "./timeline",
    HealthRecords = Timeline,
    ImmunizationScheduleExport = "./export-records",
    ImmunizationScheduleDependents = "./dependents",
    OrganDonor = "./services",
    QuickLink = Timeline,
}
