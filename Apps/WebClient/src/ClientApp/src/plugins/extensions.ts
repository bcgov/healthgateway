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
    Load = "Load",
    View = "View",
    Visit = "Visit",
    ButtonClick = "button_click",
    Download = "download",
    Email = "email",
    InternalLink = "internal_link",
    ExternalLink = "external_link",
    Submit = "submit",
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
    AddressChangeBC = "Address Change BC",
    Dependents = "Dependents",
    Download = "Download",
    Export = "Export",
    HealthLinkBC = "HealthLink BC",
    Home = "Home",
    ImmunizationRecordBC = "Immunization Record BC",
    Profile = "Profile",
    Services = "Services",
    SupportEmail = "Support Email",
    SupportGuide = "Support Guide",
    TermsOfService = "Terms of Service",
    Timeline = "Timeline",
    TransplantBC = "Transplant BC",
}

export const enum Format {
    Csv = "CSV",
    Image = "Image",
    Pdf = "PDF",
    XLSX = "XLSX",
}

export const enum Origin {
    Breadcrumb = "Breadcrumb",
    Dependents = "Dependents",
    Exports = "Exports",
    Footer = "Footer",
    Home = "Home",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    Profile = "Profile",
    ServicesPage = "Services Page",
    Timeline = "Timeline",
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
    Data = "Data",
    Document = "Document",
    ExternalLink = "External Link",
    InternalLink = "Internal Link",
    Page = "Page",
    Request = "Request",
    AboutUs = "About Us",
    AddDependent = "Add a Dependent",
    AddNote = "Add a Note",
    AppRating = "App Rating",
    BcVaccineCard = "BC Vaccine Card",
    DeleteAccount = "Delete Account",
    Dependents = "Dependents",
    DownloadClinicalDocument = "Download Clinical Document",
    DownloadDependentHistoricImmunizations = "Download Dependent Historic Immunizations",
    DownloadDependentRecommendedImmunizations = "Download Dependent Recommended Immunizations",
    DownloadImagingReport = "Download Imaging Report",
    DownloadLabResult = "Download Lab Result",
    DownloadProofOfVaccination = "Download Proof of Vaccination",
    EmailHealthGateway = "Email HealthGateway",
    Export = "Export",
    Faq = "FAQ",
    HealthGatewayLogo = "Health Gateway Logo",
    HealthRecords = "Health Records",
    HealthLinkBC = "HealthLinkBC",
    HomeBreadcrumb = "Home Breadcrumb",
    ViewRecommendedImmunizations = "View Recommended Immunizations",
    ImmunizationScheduleExport = "Immunization Schedule Export",
    ImmunizationScheduleDependents = "Immunization Schedule Dependents",
    ImmunizationUpdateForm = "Immunization Update Form",
    Logout = "Logout",
    OrganDonor = "Organ Donor",
    RegisterDependent = "Register a Dependent",
    RegisterOrganDonorDecision = "Register Organ Donor Decision",
    DownloadOrganDonorDecision = "Download Organ Donor Decision",
    Profile = "Profile",
    RemoveDependent = "Remove a Dependent",
    QuickLink = "Quicklink",
    ReleaseNotes = "Release Notes",
    SendFeedback = "Send Feedback",
    Services = "Services",
    TermsOfService = "Terms of Service",
    Timeline = "Timeline",
    TimelineMissingImmunizations = "Timeline - Missing Immunizations Fill In Online Form",
    VerifyContactInformation = "Verify Contact Information",
    ViewProofOfVaccination = "View Proof of Vaccination",
    VerifyEmailAddress = "Verify Email Address",
    VerifyMobileNumber = "Verify Mobile Number",
    UpdateMailingAddress = "Update Mailing Address",
}

export const enum Type {
    Covid19ProofOfVaccination = "COVID-19 Proof of Vaccination",
    OrganDonorRegistration = "Organ Donor Registration",
    PublicCovid19ProofOfVaccination = "Public COVID-19 Proof of Vaccination",
    Recall = "Recall",
    Result = "Result",
    BcCancerScreening = "BC Cancer Screening",
    ClinicalDocuments = "Clinical Documents",
    Dependents = "Dependents",
    Footer = "Footer",
    Header = "Header",
    HomeTile = "Home Tile",
    InfoBanner = "Info Banner",
    ImagingReports = "Imaging Reports",
    Immunizations = "Immunizations",
    LabResults = "Lab Results",
    Logout = "Logout",
    Notes = "Notes",
    OrganDonor = "Organ Donor",
    Profile = "Profile",
    ServiceTile = "Service Tile",
    Sidebar = "Sidebar",
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

export function isBcCancerProgram(program: string): program is BcCancerProgram {
    return (BcCancerPrograms as readonly string[]).includes(program);
}

export const enum EmailUrl {
    HealthGatewayEmail = "healthgateway@gov.bc.ca",
}

export const enum ExternalUrl {
    AbuutUs = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway",
    AddressChangeBC = "https://www.addresschange.gov.bc.ca",
    ImmunizationRecordBC = "https://www.immunizationrecord.gov.bc.ca",
    Faq = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide",
    HealthLinkBC = "https://www.healthlinkbc.ca/find-care/health-connect-registry",
    OrganDonorRegistration = "http://www.transplant.bc.ca/organ-donation/register-as-an-organ-donor/register-your-decision",
    ReleaseNotes = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/release-notes",
    TermsOfService = "https://www.healthgateway.gov.bc.ca/termsOfService",
}

export const enum InternalUrl {
    Timeline = "./timeline",
    Dependents = "./dependents",
    Export = "./export",
    HealthRecords = Timeline,
    Home = "./home",
    ImmunizationScheduleExport = "./export-records",
    ImmunizationScheduleDependents = "./dependents",
    OrganDonor = "./services",
    Profile = "./profile",
    QuickLink = Timeline,
    Services = "./services",
}
