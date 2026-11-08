import { View } from "react-native";

import {
    AppText,
    ExternalLink,
    FieldLabel,
    SectionHeading,
} from "@/components/ui";
import {
    Action,
    Destination,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import type { Address } from "@/models/patient";
import { services } from "@/services/container";
import { useUserStore } from "@/stores/user.store";

function AddressBlock({
    address,
    testIDPrefix,
}: {
    address?: Address | null;
    testIDPrefix: string;
}) {
    return address ? (
        <View testID={`${testIDPrefix}-address-div`}>
            {address.streetLines.map((line, i) => (
                <AppText key={i}>{line}</AppText>
            ))}
            <AppText>
                {address.city}, {address.state}, {address.postalCode}
            </AppText>
        </View>
    ) : (
        <AppText className="italic" testID={`no-${testIDPrefix}-address-text`}>
            No address on record
        </AppText>
    );
}

function onAddressChangeClick() {
    services.tracking.trackEvent({
        action: Action.ExternalLink,
        text: Text.UpdateMailingAddress,
        origin: Origin.Profile,
        destination: Destination.AddressChangeBC,
        type: Type.Profile,
        url: TrackedExternalUrl.AddressChangeBC,
    });
}

function AddressChangeLink({ text }: { text: string }) {
    return (
        <ExternalLink
            href={TrackedExternalUrl.AddressChangeBC}
            onPress={onAddressChangeClick}
        >
            {text}
        </ExternalLink>
    );
}

/** Port of UserProfileAddressComponent.vue */
export function AddressSection() {
    const physical = useUserStore((s) => s.patient?.physicalAddress);
    const postal = useUserStore((s) => s.patient?.postalAddress);
    const hasAddress = Boolean(physical) || Boolean(postal);

    let same: boolean;
    if (!physical || !postal) same = !hasAddress;
    else
        same =
            physical.streetLines.length === postal.streetLines.length &&
            physical.streetLines.every((l, i) => l === postal.streetLines[i]) &&
            physical.city === postal.city &&
            physical.state === postal.state &&
            physical.postalCode === postal.postalCode;

    const postalLabel =
        !same || (physical && !postal) ? "Mailing Address" : "Address";

    return (
        <View>
            <SectionHeading title="Address" includeDivider level={2} />
            <View className="mb-4">
                <FieldLabel
                    testID="postal-address-label"
                    title={postalLabel}
                    level={3}
                />
                <AddressBlock address={postal} testIDPrefix="postal" />
            </View>
            {!same ? (
                <View className="mb-4" testID="physical-address-section">
                    <FieldLabel
                        testID="physical-address-label"
                        title="Physical Address"
                        level={3}
                    />
                    <AddressBlock address={physical} testIDPrefix="physical" />
                </View>
            ) : null}
            {hasAddress && same ? (
                <AppText className="mb-4">
                    If this address is incorrect, update it{" "}
                    <AddressChangeLink text="here" />.
                </AppText>
            ) : null}
            {!same ? (
                <AppText className="mb-4">
                    If either of these addresses is incorrect, update them{" "}
                    <AddressChangeLink text="here" />.
                </AppText>
            ) : null}
            {!hasAddress ? (
                <AppText className="mb-4">
                    To add an address, visit{" "}
                    <AddressChangeLink text="this page" />.
                </AppText>
            ) : null}
        </View>
    );
}
