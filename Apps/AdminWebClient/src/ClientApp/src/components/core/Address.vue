<script lang="ts">
import { extend, ValidationProvider } from "vee-validate";
import { regex, required } from "vee-validate/dist/rules";
import Vue from "vue";
import { Component, Prop, PropSync, Watch } from "vue-property-decorator";

import { Countries, InternationalDestinations } from "@/constants/countries";
import { Provinces } from "@/constants/provinces";
import { States } from "@/constants/states";
import type SelectItem from "@/models/selectItem";
import {
    Mask,
    postalCodeMaskTemplate,
    zipCodeMaskTemplate,
} from "@/utility/masks";

const errorMessage = "This is a required field";

extend("regex", regex);

extend("required", {
    ...required,
    message: errorMessage,
});

@Component({
    components: {
        ValidationProvider,
    },
})
export default class AddressComponent extends Vue {
    @Prop({ default: "" }) streetLines!: string[];
    @PropSync("city", { default: "" }) cityModel!: string;
    @PropSync("state", { default: "" }) stateModel!: string;
    @PropSync("postalCode", { default: "" }) postalCodeModel!: string;
    @PropSync("country", { default: "" }) countryModel!: string;
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
        if (this.selectedCountryCode === this.countryModel) {
            return;
        }

        // select destination matching first name associated with country code
        this.selectedDestination = Countries[this.countryModel]
            ? Countries[this.countryModel][0]
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

    private get postalCodeRegex(): RegExp | boolean {
        if (this.isCanadaSelected) {
            return /^[A-Z]\d[A-Z] \d[A-Z]\d$/;
        } else if (this.isUnitedStatesSelected) {
            return /^\d{5}(-\d{4})?$/;
        }
        return false;
    }

    private get streetLinesModel(): string {
        return this.streetLines.join("\n");
    }

    private set streetLinesModel(model: string) {
        this.$emit("update:streetLines", model.split("\n"));
    }

    private mounted(): void {
        this.onCountryChanged();
    }
}
</script>

<template>
    <div>
        <v-row dense>
            <v-col>
                <ValidationProvider
                    v-slot="{ errors }"
                    :rules="{ required: true }"
                >
                    <v-textarea
                        v-model="streetLinesModel"
                        label="Address"
                        :disabled="isDisabled"
                        auto-grow
                        rows="1"
                        autocomplete="chrome-off"
                    />
                    <div class="error-message">
                        {{ errors[0] }}
                    </div>
                </ValidationProvider>
            </v-col>
        </v-row>
        <v-row dense>
            <v-col cols sm="6" md="4">
                <ValidationProvider
                    v-slot="{ errors }"
                    :rules="{ required: true }"
                >
                    <v-text-field
                        v-model="cityModel"
                        label="City"
                        :disabled="isDisabled"
                        autocomplete="chrome-off"
                    />
                    <div class="error-message">
                        {{ errors[0] }}
                    </div>
                </ValidationProvider>
            </v-col>
            <v-col cols sm="6" md="4">
                <ValidationProvider
                    v-slot="{ errors }"
                    :rules="{ required: true }"
                >
                    <v-select
                        v-if="provinceStateList.length > 0"
                        v-model="stateModel"
                        :items="provinceStateList"
                        label="Province/State"
                        :disabled="isDisabled"
                        autocomplete="chrome-off"
                    />
                    <v-text-field
                        v-else
                        v-model="stateModel"
                        label="Province/State"
                        :disabled="isDisabled"
                        autocomplete="chrome-off"
                    />
                    <div class="error-message">
                        {{ errors[0] }}
                    </div>
                </ValidationProvider>
            </v-col>
            <v-col cols md="4">
                <ValidationProvider
                    v-slot="{ errors }"
                    :rules="{ required: true, regex: postalCodeRegex }"
                    name="Postal Code"
                >
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
                    <div class="error-message">
                        {{ errors[0] }}
                    </div>
                </ValidationProvider>
            </v-col>
            <v-col cols md="8" xl="4">
                <ValidationProvider
                    v-slot="{ errors }"
                    :rules="{ required: true }"
                >
                    <v-select
                        v-model="selectedDestination"
                        :items="internationalDestinations"
                        label="Country"
                        :disabled="isDisabled"
                        autocomplete="chrome-off"
                    />
                    <div class="error-message">
                        {{ errors[0] }}
                    </div>
                </ValidationProvider>
            </v-col>
        </v-row>
    </div>
</template>

<style scoped lang="scss">
.error-message {
    color: #ff5252 !important;
}
</style>
