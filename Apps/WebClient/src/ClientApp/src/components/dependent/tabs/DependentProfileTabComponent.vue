<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { DateWrapper, StringISODate } from "@/models/dateWrapper";
import type { Dependent } from "@/models/dependent";

@Component
export default class DependentProfileTabComponent extends Vue {
    @Prop({ required: true })
    private dependent!: Dependent;

    private get otherDelegateCount(): number {
        return this.dependent.totalDelegateCount - 1;
    }

    private formatDate(date: StringISODate): string {
        return new DateWrapper(date).format();
    }
}
</script>
<template>
    <b-row cols="1" cols-lg="2" cols-xl="3" class="mb-n3">
        <b-col class="mb-3">
            <div>PHN</div>
            <b-form-input
                v-model="dependent.dependentInformation.PHN"
                data-testid="dependent-phn"
                readonly
            />
        </b-col>
        <b-col class="mb-3">
            <div>Date of Birth</div>
            <b-form-input
                :value="formatDate(dependent.dependentInformation.dateOfBirth)"
                data-testid="dependent-date-of-birth"
                readonly
            />
        </b-col>
        <b-col class="mb-3">
            <div>How Many Others Have Access</div>
            <b-form-input
                v-model="otherDelegateCount"
                data-testid="dependent-other-delegate-count"
                readonly
            />
            <small
                :id="`other-delegate-info-${dependent.ownerId}`"
                class="interactable-text"
            >
                What does this mean?
            </small>
            <b-popover
                :target="`other-delegate-info-${dependent.ownerId}`"
                triggers="hover focus"
                placement="bottom"
                boundary="viewport"
                :data-testid="`other-delegate-info-popover-${dependent.ownerId}`"
            >
                This shows you how many people other than you have added your
                dependent to their Health Gateway account. For privacy, we can’t
                tell you their names. If this number isn’t what you expect,
                contact us at
                <a href="mailto:HealthGateway@gov.bc.ca"
                    >HealthGateway@gov.bc.ca</a
                >.
            </b-popover>
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.interactable-text {
    color: $hg-link;
}
</style>
