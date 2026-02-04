import { AccessLinkType } from "@/constants/accessLinks";
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
    CardClick = "card_click",
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
    BcVaccineCard = "BC Vaccine Card",
    BookVaccine = "Book a Vaccine",
    HealthGatewayBeta = "Health Gateway Beta",
    HealthRecords = "Health Records",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    ImmunizationSchedule = "Immunization Schedule",
    ImmunizationsHealthLinkBC = "Immunizations - HealthLink BC",
    OrganDonorRegistration = "Organ Donor Registration",
    PrimaryCare = "Primary Care",
    AccessMyHealth = "AccessMyHealth",
    AddressChangeBC = "Address Change BC",
    Dependents = "Dependents",
    Download = "Download",
    Export = "Export",
    FraserHealthRecordRequest = "Fraser Health Record Request",
    HealthElife = "HealthElife",
    HealthGateway = "Health Gateway",
    HealthLinkBC = "HealthLink BC",
    HealthLinkBC811 = "HealthLink BC 8-1-1",
    HealthConnectRegistry = "HealthLink BC Health Connect Registry",
    Home = "Home",
    ImmunizationRecordBC = "Immunization Record BC",
    Login = "Login",
    MohCovid19 = "MoH COVID-19",
    MyHealth = "MyHealth",
    MyHealthKey = "myhealthkey",
    MyHealthPortal = "MyHealthPortal",
    Profile = "Profile",
    RecordSources = "Record Sources",
    Registration = "Registration",
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
    AccessMyHealthDialog = "Access My Health Dialog",
    Breadcrumb = "Breadcrumb",
    Dependents = "Dependents",
    Download = "Download",
    Footer = "Footer",
    Home = "Home",
    ImmunizationRecommendationDialog = "Immunization Recommendations Dialog",
    Landing = "Landing",
    OtherRecordSources = "Other Record Sources",
    Profile = "Profile",
    ServicesPage = "Services Page",
    Timeline = "Timeline",
    VaccineCard = "Vaccine Card",
    VPPLoginPage = "VPP Login Page",
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
    AccessMyHealthDialogCancel = "AccessMyHealth Dialog Cancel",
    AccessMyHealthDialogUrl = "AccessMyHealth Dialog URL",
    AccessMyHealthDialogSignin = "AccessMyHealth Dialog Sign in",
    AccessMyHealthTile = "AccessMyHealth Tile",
    AccessMyHealthURL = "AccessMyHealth URL",
    AddDependent = "Add a Dependent",
    AddNote = "Add a Note",
    AppRating = "App Rating",
    BackToAccessMyHealth = "Back to AccessMyHealth",
    BcVaccineCard = "BC Vaccine Card",
    BcVaccineSchedule = "BC Vaccine Schedule",
    Cancel = "Cancel",
    Covid19VaccineInformation = "COVID-19 Vaccine Information",
    DeleteAccount = "Delete Account",
    Dependents = "Dependents",
    DependentRecords = "Dependent records",
    Download = "Download",
    DownloadClinicalDocument = "Download Clinical Document",
    DownloadDependentHistoricImmunizations = "Download Dependent Historic Immunizations",
    DownloadDependentRecommendedImmunizations = "Download Dependent Recommended Immunizations",
    DownloadImagingReport = "Download Imaging Report",
    DownloadLabResult = "Download Lab Result",
    DownloadProofOfVaccination = "Download Proof of Vaccination",
    EmailHealthGateway = "Email HealthGateway",
    Export = "Export",
    FilterHealthRecords = "Filter Health Records",
    FindDoctor = "Find family doctor",
    FindYourHealthRecords = "Find your health records",
    FraserHealthRequestTile = "Fraser Health Request Tile",
    FraserHealthRequestURL = "Fraser Health Request URL",
    HealthElifeTile = "HealthElife Tile",
    HealthElifeURL = "HealthElife URL",
    HealthGatewayHome = "Health Gateway Home",
    HealthGatewayLogo = "Health Gateway Logo",
    HealthGatewayInfo = "Health Gateway Info",
    HealthRecords = "Health Records",
    HealthLinkBC = "HealthLink BC",
    HealthLinkBC811 = "HealthLink BC 8-1-1",
    HealthServices = "Health services",
    HomeBreadcrumb = "Home Breadcrumb",
    ViewRecommendedImmunizations = "View Recommended Immunizations",
    ImmunizationBannerDownload = "Immunization Banner Download",
    ImmunizationRecord = "Immunization Record",
    ImmunizationRecordDownload = "Immunization Record Download",
    ImmunizationUpdateForm = "Immunization Update Form",
    ImmunizationsHealthLinkBC = "Immunizations - HealthLink BC",
    Login = "Log in",
    LoginBCSC = "Log in with BCSC",
    LoginVaccineCard = "Log in Vaccine Card",
    Logout = "Logout",
    MyHealthTile = "MyHealth Tile",
    MyHealthURL = "MyHealth URL",
    MyHealthKeyTile = "myhealthkey Tile",
    MyHealthKeyURL = "myhealthkey URL",
    MyHealthPortalTile = "MyHealthPortal Tile",
    MyHealthPortalURL = "MyHealthPortal URL",
    OrganDonor = "Organ Donor",
    OtherRecordSources = "Other Record Sources",
    Register = "Register",
    RegisterDependent = "Register a Dependent",
    RegisterOrganDonorDecision = "Register Organ Donor Decision",
    DownloadOrganDonorDecision = "Download Organ Donor Decision",
    Profile = "Profile",
    RemoveDependent = "Remove a Dependent",
    QuickLink = "Quicklink",
    RecoverAccount = "Recover Account",
    RecordsManagement = "Records management",
    ReleaseNotes = "Release Notes",
    SendFeedback = "Send Feedback",
    Services = "Services",
    SignIn = "Sign in",
    SupportGuide = "Support Guide",
    TermsOfService = "Terms of Service",
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
    Filter = "Filter",
    Footer = "Footer",
    Header = "Header",
    HomeTile = "Home Tile",
    InfoBanner = "Info Banner",
    ImagingReports = "Imaging Reports",
    Immunizations = "Immunizations",
    LabResults = "Lab Results",
    Landing = "Landing",
    Login = "Login",
    Logout = "Logout",
    Notes = "Notes",
    OrganDonor = "Organ Donor",
    Profile = "Profile",
    RecordSourceTile = "Record Source Tile",
    ServiceTile = "Service Tile",
    Sidebar = "Sidebar",
    VPPLogin = "VPP Login",
}

export const OriginType: Partial<Record<Origin, Type>> = {
    [Origin.OtherRecordSources]: Type.RecordSourceTile,
} as const;

export type LandingAccessLinkType =
    | AccessLinkType.Call811
    | AccessLinkType.DependentRecords
    | AccessLinkType.FindDoctor
    | AccessLinkType.HealthRecords
    | AccessLinkType.HealthLinkBC
    | AccessLinkType.HealthServices
    | AccessLinkType.RecordsManagement;

export const LandingAccessLinkText: Record<LandingAccessLinkType, string> = {
    [AccessLinkType.Call811]: Text.HealthLinkBC811,
    [AccessLinkType.DependentRecords]: Text.DependentRecords,
    [AccessLinkType.FindDoctor]: Text.FindDoctor,
    [AccessLinkType.HealthRecords]: "Health records",
    [AccessLinkType.RecordsManagement]: Text.RecordsManagement,
    [AccessLinkType.HealthLinkBC]: Text.HealthLinkBC,
    [AccessLinkType.HealthServices]: Text.HealthServices,
} as const;

export const LandingAccessLinkDestination: Record<
    LandingAccessLinkType,
    string
> = {
    [AccessLinkType.Call811]: Destination.HealthLinkBC811,
    [AccessLinkType.DependentRecords]: Destination.SupportGuide,
    [AccessLinkType.FindDoctor]: Destination.HealthConnectRegistry,
    [AccessLinkType.HealthRecords]: Destination.SupportGuide,
    [AccessLinkType.RecordsManagement]: Destination.SupportGuide,
    [AccessLinkType.HealthLinkBC]: Destination.HealthLinkBC,
    [AccessLinkType.HealthServices]: Destination.SupportGuide,
} as const;

export type ResourceLinkType =
    | AccessLinkType.AccessMyHealth
    | AccessLinkType.MyHealth
    | AccessLinkType.MyHealthPortal
    | AccessLinkType.HealthElife
    | AccessLinkType.MyHealthKey
    | AccessLinkType.FraserHealth;

export const ResourceLinkTileText: Record<ResourceLinkType, Text> = {
    [AccessLinkType.AccessMyHealth]: Text.AccessMyHealthTile,
    [AccessLinkType.MyHealth]: Text.MyHealthTile,
    [AccessLinkType.MyHealthPortal]: Text.MyHealthPortalTile,
    [AccessLinkType.HealthElife]: Text.HealthElifeTile,
    [AccessLinkType.MyHealthKey]: Text.MyHealthKeyTile,
    [AccessLinkType.FraserHealth]: Text.FraserHealthRequestTile,
} as const;

export const ResourceLinkUrlText: Record<ResourceLinkType, Text> = {
    [AccessLinkType.AccessMyHealth]: Text.AccessMyHealthURL,
    [AccessLinkType.MyHealth]: Text.MyHealthURL,
    [AccessLinkType.MyHealthPortal]: Text.MyHealthPortalURL,
    [AccessLinkType.HealthElife]: Text.HealthElifeURL,
    [AccessLinkType.MyHealthKey]: Text.MyHealthKeyURL,
    [AccessLinkType.FraserHealth]: Text.FraserHealthRequestURL,
} as const;

export const ResourceLinkDestination: Record<ResourceLinkType, Destination> = {
    [AccessLinkType.AccessMyHealth]: Destination.AccessMyHealth,
    [AccessLinkType.MyHealth]: Destination.MyHealth,
    [AccessLinkType.MyHealthPortal]: Destination.MyHealthPortal,
    [AccessLinkType.HealthElife]: Destination.HealthElife,
    [AccessLinkType.MyHealthKey]: Destination.MyHealthKey,
    [AccessLinkType.FraserHealth]: Destination.FraserHealthRecordRequest,
} as const;

export const BcCancerPrograms = ["Breast", "Cervix", "Colon", "Lung"] as const;
export type BcCancerProgram = (typeof BcCancerPrograms)[number];

export const BcCancerDestination: Record<BcCancerProgram, string> = {
    Breast: "Breast Cancer Screening",
    Cervix: "Cervix Cancer Screening",
    Colon: "Colon Cancer Screening",
    Lung: "Lung Cancer Screening",
} as const;

export function isBcCancerProgram(program: string): program is BcCancerProgram {
    return (BcCancerPrograms as readonly string[]).includes(program);
}

export const enum EmailUrl {
    HealthGatewayEmail = "healthgateway@gov.bc.ca",
}

export const enum ExternalUrl {
    AbuutUs = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway",
    AccessMyHealth = "https://www.accessmyhealth.ca",
    AddressChangeBC = "https://www.addresschange.gov.bc.ca",
    ImmunizationRecordBC = "https://www.immunizationrecord.gov.bc.ca",
    HealthConnectRegistry = "https://www.healthlinkbc.ca/find-care/health-connect-registry",
    HealthGateway = "https://www.healthgateway.gov.bc.ca",
    OrganDonorRegistration = "https://www.transplant.bc.ca/organ-donation/register-as-an-organ-donor/register-your-decision",
    ReleaseNotes = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/release-notes",
    SupportGuide = "https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide",
    TermsOfService = "https://www.healthgateway.gov.bc.ca/termsOfService",
    YourHealthInformation = "https://www.healthlinkbc.ca/health-library/health-features/your-health-information",
}

export const enum InternalUrl {
    Timeline = "./timeline",
    Dependents = "./dependents",
    Home = "./home",
    Login = "./login",
    OtherRecordSources = "./otherRecordSources",
    Profile = "./profile",
    Registration = "./Registration",
    Reports = "./reports",
    Services = "./services",
}

export type ExternalLinkConfirmationDialogType = AccessLinkType.AccessMyHealth;

export const ExternalLinkConfirmationDialogText: Record<
    ExternalLinkConfirmationDialogType,
    string
> = {
    [AccessLinkType.AccessMyHealth]: Text.AccessMyHealthDialogUrl,
} as const;

export const ExternalUrlDestination: Partial<Record<ExternalUrl, Destination>> =
    {
        [ExternalUrl.AccessMyHealth]: Destination.AccessMyHealth,
    } as const;

export function createLinkEventData(
    text: string,
    origin: Origin,
    action: Action,
    url?: ExternalUrl
): EventData {
    return {
        action,
        text,
        destination: url ? (ExternalUrlDestination[url] ?? url) : undefined,
        origin,
        type: OriginType[origin],
        url,
    };
}
