<script lang="ts">
import Vue from "vue";
import { Component, Emit, Model, Prop, Watch } from "vue-property-decorator";

interface ISelectOption {
    text: string;
    value: unknown;
}

@Component
export default class HgTimeDropdownComponent extends Vue {
    @Model("change", { type: String }) public model!: string;
    @Prop() state?: boolean;
    @Prop({ required: false, default: false }) disabled!: boolean;

    private hour: number | null = null;
    private minute: number | null = null;
    private amPm: number | null = null;
    private value: string | null = "";

    private hourValues = [
        "1",
        "2",
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9",
        "10",
        "11",
        "12",
    ];

    private minuteValues = [
        "00",
        "05",
        "10",
        "15",
        "20",
        "25",
        "30",
        "35",
        "40",
        "45",
        "50",
        "55",
    ];

    private amPmValues = ["AM", "PM"];

    private mounted() {
        this.value = this.model;
    }

    private get getHour() {
        let hourOptions: ISelectOption[] = [{ value: null, text: "Hour" }];
        for (var i = 0; i < this.hourValues.length; i++) {
            hourOptions.push({
                value: this.hourValues[i],
                text: this.hourValues[i],
            });
        }
        return hourOptions;
    }

    private get getMinute() {
        let minuteOptions: ISelectOption[] = [{ value: null, text: "Minute" }];
        for (var i = 0; i < this.minuteValues.length; i++) {
            minuteOptions.push({
                value: this.minuteValues[i],
                text: this.minuteValues[i],
            });
        }
        return minuteOptions;
    }

    private get getAmPm() {
        let amPmOptions: ISelectOption[] = [{ value: null, text: "AM/PM" }];
        for (var i = 0; i < this.amPmValues.length; i++) {
            amPmOptions.push({
                value: this.amPmValues[i],
                text: this.amPmValues[i],
            });
        }
        return amPmOptions;
    }

    private onChange() {
        if (this.hour && this.minute && this.amPm) {
            this.value = this.hour + ":" + this.minute + " " + this.amPm;
        } else {
            this.value = null;
        }

        this.updateModel();
    }

    @Emit("change")
    private updateModel() {
        return this.value;
    }

    @Watch("model")
    private onModelChanged() {
        this.value = this.model;
    }

    @Emit("blur")
    private onBlur() {
        return;
    }

    private getClass() {
        if (this.state === true) {
            return "valid";
        } else if (this.state === false) {
            return "invalid";
        } else {
            return "";
        }
    }
}
</script>

<template>
    <b-row no-gutters>
        <b-col cols="4">
            <b-form-select
                v-model="hour"
                data-testid="formSelectHour"
                :class="getClass(state)"
                :options="getHour"
                aria-label="Hour"
                :disabled="disabled"
                @change="onChange"
            />
        </b-col>
        <b-col class="px-2" cols="4">
            <b-form-select
                v-model="minute"
                data-testid="formSelectMinute"
                :class="getClass(state)"
                :options="getMinute"
                aria-label="Minute"
                :disabled="disabled"
                @change="onChange"
            />
        </b-col>
        <b-col cols="4">
            <b-form-select
                v-model="amPm"
                data-testid="formSelectAmPm"
                :class="getClass(state)"
                :options="getAmPm"
                aria-label="AM/PM"
                :disabled="disabled"
                @change="onChange"
                @blur="onBlur"
            />
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.valid {
    border-color: $success;
}
.invalid {
    border-color: $danger;
}
</style>
