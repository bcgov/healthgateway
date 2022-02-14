<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

@Component
export default class RadioButton extends Vue {
    @Prop() questionSequence!: string;
    @Prop() modelBinding!: string;
    @Prop() hasNotSureOption!: boolean;
    @Prop() hasAdditionalResponse!: boolean;

    //Radio buttons values
    private yesValue = "Yes";
    private noValue = "No";
    private notSureValue = "NotSure";

    private optionBenefit = false;
    private optionNoBenefit = false;

    private optionChange(value: string) {
        switch (value) {
            case this.yesValue: {
                this.optionBenefit = true;
                this.optionNoBenefit = false;
                break;
            }
            case this.noValue: {
                this.optionBenefit = false;
                this.optionNoBenefit = true;
                break;
            }
            default: {
                this.optionBenefit = false;
                this.optionNoBenefit = false;
            }
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
            @change="optionChange(yesValue)"
            @input="$emit('input', $event.target.value)"
        />
        <label class="pr-2">{{ yesValue }}</label>
        <input
            :v-bind="noValue"
            type="radio"
            :name="questionSequence"
            class="mr-2"
            :value="noValue"
            @change="optionChange(noValue)"
            @input="$emit('input', $event.target.value)"
        />
        <label class="pr-2">{{ noValue }}</label>
        <input
            v-if="hasNotSureOption"
            :v-bind="notSureValue"
            type="radio"
            :name="questionSequence"
            class="mr-2"
            :value="notSureValue"
            @change="optionChange(notSureValue)"
            @input="$emit('input', $event.target.value)"
        />
        <label v-if="hasNotSureOption" class="pr-2">{{ notSureValue }}</label>

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
