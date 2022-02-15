<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";

import { Countries, InternationalDestinations } from "@/constants/countries";
import { Provinces } from "@/constants/provinces";
import { States } from "@/constants/states";
import type SelectItem from "@/models/selectItem";
import {
    Mask,
    postalCodeMaskTemplate,
    zipCodeMaskTemplate,
} from "@/utility/masks";

@Component
export default class AddressComponent extends Vue {
    @Prop({ default: "" }) streetLines!: string[];
    @Prop({ default: "" }) city!: string;
    @Prop({ default: "" }) state!: string;
    @Prop({ default: "" }) postalCode!: string;
    @Prop({ default: "" }) country!: string;
    @Prop() isDisabled!: boolean;

    private selectedDestination = "";

    private get internationalDestinations(): SelectItem[] {
        // sort destinations alphabetically except place Canada and US at the top
        const destinations = Object.keys(InternationalDestinations)
            .filter(
                (destination) =>
                    destination !== Countries.CA[0] &&
                    destination !== Countries.US[0]
            )
            .sort();
        destinations.unshift(Countries.CA[0], Countries.US[0]);

        return destinations.map((destination) => ({
            text: destination,
            value: destination,
        }));
    }

    @Watch("selectedDestination")
    private onSelectedDestinationChanged(): void {
        this.$emit("update:country", this.selectedCountryCode);
    }

    @Watch("country")
    private onCountryChanged(): void {
        if (this.selectedCountryCode === this.country) {
            return;
        }

        // select destination matching first name associated with country code
        this.selectedDestination = Countries[this.country]
            ? Countries[this.country][0]
            : "";
    }

    private get selectedCountryCode(): string {
        return InternationalDestinations[this.selectedDestination];
    }

    private get provinceStateList(): SelectItem[] {
        if (this.isCanadaSelected) {
            return Object.keys(Provinces).map((provinceCode) => {
                return {
                    text: Provinces[provinceCode],
                    value: provinceCode,
                };
            });
        } else if (this.isUnitedStatesSelected) {
            return Object.keys(States).map((stateCode) => {
                return {
                    text: States[stateCode],
                    value: stateCode,
                };
            });
        } else {
            return [];
        }
    }

    private get isCanadaSelected(): boolean {
        return this.selectedDestination === Countries.CA[0];
    }

    private get isUnitedStatesSelected(): boolean {
        return this.selectedDestination === Countries.US[0];
    }

    private get postalCodeMask(): Mask | undefined {
        if (this.isCanadaSelected) {
            return postalCodeMaskTemplate;
        } else if (this.isUnitedStatesSelected) {
            return zipCodeMaskTemplate;
        }
        return undefined;
    }

    private get streetLinesModel(): string {
        return this.streetLines.join("\n");
    }

    private set streetLinesModel(model: string) {
        this.streetLines = model.split("\n");
        this.$emit("update:streetLines", this.streetLines);
    }

    private get postalCodeModel(): string {
        return this.postalCode;
    }

    private set postalCodeModel(model: string) {
        this.$emit("update:postalCode", model);
    }

    private mounted(): void {
        this.onCountryChanged();
    }
}
</script>

<template>
    <div>
        <v-row align="center" dense>
            <v-col>
                <v-textarea
                    v-model="streetLinesModel"
                    label="Address"
                    :disabled="isDisabled"
                    auto-grow
                    rows="1"
                    autocomplete="chrome-off"
                />
            </v-col>
        </v-row>
        <v-row align="center" dense>
            <v-col cols sm="6" md="4">
                <v-text-field
                    :value="city"
                    label="City"
                    :disabled="isDisabled"
                    autocomplete="chrome-off"
                    @change="$emit('update:city', $event)"
                />
            </v-col>
            <v-col v-if="provinceStateList.length > 0" cols sm="6" md="4">
                <v-select
                    :value="state"
                    :items="provinceStateList"
                    label="Province/State"
                    :disabled="isDisabled"
                    autocomplete="chrome-off"
                    @change="$emit('update:state', $event)"
                />
            </v-col>
            <v-col v-else cols sm="6" md="4">
                <v-text-field
                    :value="state"
                    label="Province/State"
                    :disabled="isDisabled"
                    autocomplete="chrome-off"
                    @change="$emit('update:state', $event)"
                />
            </v-col>
            <v-col cols md="4">
                <v-text-field
                    v-if="postalCodeMask !== undefined"
                    v-model="postalCodeModel"
                    v-mask="postalCodeMask"
                    label="Postal Code"
                    :disabled="isDisabled"
                    autocomplete="chrome-off"
                />
                <v-text-field
                    v-else
                    v-model="postalCodeModel"
                    label="Postal Code"
                    :disabled="isDisabled"
                    autocomplete="chrome-off"
                />
            </v-col>
            <v-col cols md="8" xl="4">
                <v-select
                    v-model="selectedDestination"
                    :items="internationalDestinations"
                    label="Country"
                    :disabled="isDisabled"
                    autocomplete="chrome-off"
                />
            </v-col>
        </v-row>
    </div>
</template>
