<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

import { CovidTreatmentAssessmentOption } from "@/constants/covidTreatmentAssessmentOption";

@Component
export default class OptionDetails extends Vue {
    @Prop({ required: true }) value!: CovidTreatmentAssessmentOption;
    @Prop({ required: false, default: false }) hasNotSureOption!: boolean;
    @Prop({ required: false, default: false }) hasAdditionalResponse!: boolean;

    private get options(): CovidTreatmentAssessmentOption[] {
        let options = [
            CovidTreatmentAssessmentOption.Yes,
            CovidTreatmentAssessmentOption.No,
        ];
        if (this.hasNotSureOption) {
            options.push(CovidTreatmentAssessmentOption.NotSure);
        }
        return options;
    }

    private getLabel(option: CovidTreatmentAssessmentOption): string {
        switch (option) {
            case CovidTreatmentAssessmentOption.Yes:
                return "Yes";
            case CovidTreatmentAssessmentOption.No:
                return "No";
            case CovidTreatmentAssessmentOption.NotSure:
                return "Not Sure";
            default:
                return CovidTreatmentAssessmentOption.Unspecified;
        }
    }

    private get hasOptionBenefit(): boolean {
        return this.value === CovidTreatmentAssessmentOption.Yes;
    }

    private get hasOptionNoBenefit(): boolean {
        return this.value === CovidTreatmentAssessmentOption.No;
    }

    private optionChange(value: CovidTreatmentAssessmentOption) {
        this.$emit("update:value", value);
    }
}
</script>

<template>
    <div>
        <v-radio-group :value="value" row>
            <v-radio
                v-for="(option, index) in options"
                :key="index"
                :label="getLabel(option)"
                :value="option"
                @change="optionChange(option)"
            />
        </v-radio-group>
        <div v-if="hasAdditionalResponse">
            <div v-if="hasOptionBenefit">
                <span class="option-message-color">
                    Citizen may benefit from COVID-19 treatment.
                </span>
            </div>
            <div v-if="hasOptionNoBenefit">
                <span class="option-message-color">
                    Citizen would likely not benefit from COVID-19 treatment.
                </span>
            </div>
        </div>
    </div>
</template>
<style scoped lang="scss">
.option-message-color {
    color: #ff9800;
}
</style>
