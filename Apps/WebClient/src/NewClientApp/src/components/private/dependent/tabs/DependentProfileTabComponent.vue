<script setup lang="ts">
import { computed } from "vue";

import InfoPopoverComponent from "@/components/common/InfoPopoverComponent.vue";
import InfoPopover from "@/constants/infoPopover";
import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";

interface Props {
    dependent: Dependent;
}

const props = defineProps<Props>();

const otherDelegateCount = computed(() => {
    return props.dependent.totalDelegateCount - 1;
});

function formatDate(date: StringISODate): string {
    return DateWrapper.format(date);
}
</script>

<template>
    <v-row class="text-body-1">
        <v-col xl="3" md="4" sm="6">
            <label>PHN</label>
            <v-text-field
                density="compact"
                :value="dependent.dependentInformation.PHN"
                data-testid="dependent-phn"
                readonly
                class="mt-2"
                hide-details
            />
        </v-col>
        <v-col xl="3" md="4" sm="6">
            <label>Date of Birth</label>
            <v-text-field
                density="compact"
                :value="formatDate(dependent.dependentInformation.dateOfBirth)"
                data-testid="dependent-date-of-birth"
                readonly
                class="mt-2"
                hide-details
            />
        </v-col>
        <v-col xl="3" md="4" sm="6">
            <label>How Many Others Have Access</label>
            <v-text-field
                density="compact"
                :value="otherDelegateCount"
                data-testid="dependent-other-delegate-count"
                readonly
                class="mt-2"
                hide-details
            />
            <InfoPopoverComponent
                :button-text="`What does this mean?`"
                :button-test-id="`other-delegate-info-popover-button-${dependent.ownerId}`"
                :popover-text="InfoPopover.otherDelegateInfo"
                :popover-test-id="`other-delegate-info-popover-${dependent.ownerId}`"
            />
        </v-col>
    </v-row>
</template>
