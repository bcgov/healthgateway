<script setup lang="ts">
import { computed } from "vue";

import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import {
    Action,
    Destination,
    ExternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useUserStore } from "@/stores/user";

const userStore = useUserStore();

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const physicalAddress = computed(() => userStore.patient.physicalAddress);
const postalAddress = computed(() => userStore.patient.postalAddress);
const hasAddress = computed(
    () => Boolean(physicalAddress.value) || Boolean(postalAddress.value)
);
const isSameAddress = computed(() => {
    if (!physicalAddress.value || !postalAddress.value) {
        return !hasAddress.value;
    }

    const arrayEqual = (a: string[], b: string[]) =>
        a.length === b.length && a.every((v, i) => v === b[i]);

    const streetLinesMatch = arrayEqual(
        postalAddress.value.streetLines,
        physicalAddress.value.streetLines
    );
    const cityMatches =
        postalAddress.value.city === physicalAddress.value?.city;
    const stateMatches =
        postalAddress.value.state === physicalAddress.value?.state;
    const postalCodeMatches =
        postalAddress.value.postalCode === physicalAddress.value?.postalCode;

    return streetLinesMatch && cityMatches && stateMatches && postalCodeMatches;
});
const postalAddressLabel = computed(() =>
    !isSameAddress.value || (physicalAddress.value && !postalAddress.value)
        ? "Mailing Address"
        : "Address"
);
function onAddressChangeClick() {
    trackingService.trackEvent({
        action: Action.ExternalLink,
        text: Text.UpdateMailingAddress,
        origin: Origin.Profile,
        destination: Destination.AddressChangeBC,
        type: Type.Profile,
        url: ExternalUrl.AddressChangeBC,
    });
}
</script>

<template>
    <div class="mb-4">
        <SectionHeaderComponent
            data-testid="postal-address-label"
            :title="postalAddressLabel"
        />
        <div
            v-if="postalAddress"
            data-testid="postal-address-div"
            class="text-body-1"
        >
            <div
                v-for="(item, index) in postalAddress.streetLines"
                :key="index"
            >
                {{ item }}
            </div>
            <div>
                {{ postalAddress.city }}, {{ postalAddress.state }},
                {{ postalAddress.postalCode }}
            </div>
        </div>
        <div
            v-else
            data-testid="no-postal-address-text"
            class="text-body-1 font-italic"
        >
            No address on record
        </div>
    </div>
    <div
        v-if="!isSameAddress"
        data-testid="physical-address-section"
        class="mb-4"
    >
        <SectionHeaderComponent
            data-testid="physical-address-label"
            title="Physical Address"
        />
        <div
            v-if="physicalAddress"
            data-testid="physical-address-div"
            class="text-body-1"
        >
            <div
                v-for="(item, index) in physicalAddress.streetLines"
                :key="index"
            >
                {{ item }}
            </div>
            <div>
                {{ physicalAddress.city }}, {{ physicalAddress.state }},
                {{ physicalAddress.postalCode }}
            </div>
        </div>
        <div
            v-else
            data-testid="no-physical-address-text"
            class="text-body-1 font-italic"
        >
            No address on record
        </div>
    </div>
    <div v-if="hasAddress && isSameAddress" class="mb-4 text-body-1">
        If this address is incorrect, update it
        <a
            href="https://www.addresschange.gov.bc.ca/"
            target="_blank"
            rel="noopener"
            class="text-link"
            @click="onAddressChangeClick"
            >here</a
        >.
    </div>
    <div v-if="!isSameAddress" class="mb-4 text-body-1">
        If either of these addresses is incorrect, update them
        <a
            href="https://www.addresschange.gov.bc.ca/"
            target="_blank"
            rel="noopener"
            class="text-link"
            @click="onAddressChangeClick"
            >here</a
        >.
    </div>
    <div v-if="!hasAddress" class="mb-4 text-body-1">
        To add an address, visit
        <a
            href="https://www.addresschange.gov.bc.ca/"
            target="_blank"
            rel="noopener"
            class="text-link"
            @click="onAddressChangeClick"
            >this page</a
        >.
    </div>
</template>
