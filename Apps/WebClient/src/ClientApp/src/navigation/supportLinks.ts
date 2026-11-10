import { ExternalUrl } from "@/constants/externalUrls";
import { Path } from "@/constants/path";
import {
    Action,
    Destination,
    EmailUrl,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { services } from "@/services/container";
import { openExternal } from "@/utils/openExternal";

export interface SupportLink {
    text: string;
    href?: string;
    to?: string;
    testID: string;
    action?: Action;
    event: { text: Text; destination: Destination; url: string };
}

// FooterComponent.vue links, in its order
// prettier-ignore
export const supportLinks: SupportLink[] = [
  { text: "Support Guide", href: ExternalUrl.SupportGuide, testID: "footer-faq-link", event: { text: Text.SupportGuide, destination: Destination.SupportGuide, url: TrackedExternalUrl.SupportGuide } },
  { text: "Release Notes", href: ExternalUrl.ReleaseNotes, testID: "footer-release-notes-link", event: { text: Text.ReleaseNotes, destination: Destination.SupportGuide, url: TrackedExternalUrl.ReleaseNotes } },
  { text: "Terms of Service", to: Path.TermsOfService, testID: "footer-terms-of-service-link", event: { text: Text.TermsOfService, destination: Destination.TermsOfService, url: TrackedExternalUrl.TermsOfService } },
  { text: "About Us", href: ExternalUrl.AboutUs, testID: "footer-about-us-link", event: { text: Text.AboutUs, destination: Destination.SupportGuide, url: TrackedExternalUrl.AboutUs } },
];

export const supportEmailLink: SupportLink = {
    text: "Email: HealthGateway@gov.bc.ca",
    href: ExternalUrl.HealthGatewayEmail,
    testID: "footer-email-link",
    action: Action.Email,
    event: {
        text: Text.EmailHealthGateway,
        destination: Destination.SupportEmail,
        url: EmailUrl.HealthGatewayEmail,
    },
};

/** Every footer link, email last. */
export const allSupportLinks: SupportLink[] = [
    ...supportLinks,
    supportEmailLink,
];

/**
 * Follows a footer link with its Snowplow event (the native Support menu lists the same links, so it
 * reports them as footer events too). Internal routes are returned for the caller's router.
 */
export function followSupportLink(
    link: SupportLink,
    navigate: (path: string) => void
) {
    services.tracking.trackEvent({
        action: link.action ?? Action.ExternalLink,
        origin: Origin.Footer,
        type: Type.Footer,
        ...link.event,
    });
    if (link.to) navigate(link.to);
    else if (link.href) void openExternal(link.href);
}
