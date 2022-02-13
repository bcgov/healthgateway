<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

@Component
export default class RadioButton extends Vue {
    @Prop() questionSequence!: string;
    @Prop() modelBinding!: string;
    @Prop() hasNotSureOption!: boolean;
    @Prop() hasAdditionalResponse!: boolean;

    private yesValue = "Yes";
    private noValue = "No";
    private notSureValue = "NotSure";
    private optionBenefit = false;
    private optionNoBenefit = false;
    private optionChange(value: string) {
        if (value === "Yes") {
            this.optionBenefit = true;
            this.optionNoBenefit = false;
        }
        if (value === "No") {
            this.optionBenefit = false;
            this.optionNoBenefit = true;
        }
        if (value === "NotSure") {
            this.optionBenefit = false;
            this.optionNoBenefit = false;
        }
    }
}
</script>

<template>
    <div>
        <input
            :v-bind="yesValue"
            type="radio"
            :name="questionSequence"
            :value="yesValue"
            class="mr-2"
            @change="optionChange('Yes')"
            @input="$emit('input', $event.target.value)"
        />
        <label class="pr-2">Yes</label>
        <input
            :v-bind="noValue"
            type="radio"
            :name="questionSequence"
            class="mr-2"
            :value="noValue"
            @change="optionChange('No')"
            @input="$emit('input', $event.target.value)"
        />
        <label class="pr-2">No</label>
        <input
            v-if="hasNotSureOption"
            :v-bind="notSureValue"
            type="radio"
            :name="questionSequence"
            class="mr-2"
            :value="notSureValue"
            @change="optionChange('NotSure')"
            @input="$emit('input', $event.target.value)"
        />
        <label v-if="hasNotSureOption" class="pr-2">Not Sure</label>

        <div v-if="hasAdditionalResponse">
            <div v-if="optionBenefit">
                <span class="font-color"
                    >Citizen may benefit from COVID-19 treatment.</span
                >
            </div>
            <div v-if="optionNoBenefit">
                <span class="font-color"
                    >Citizen would likely not benefit from COVID-19
                    treatment.</span
                >
            </div>
        </div>
    </div>
</template>
<style scoped lang="scss">
.font-color {
    color: #e49b0f;
}
</style>
