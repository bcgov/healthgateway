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

    private mounted(): void {
        this.value = this.model;
    }

    private get getHour(): ISelectOption[] {
        const hourOptions: ISelectOption[] = [{ value: null, text: "Hour" }];
        for (const value of this.hourValues) {
            hourOptions.push({
                value,
                text: value,
            });
        }
        return hourOptions;
    }

    private get getMinute(): ISelectOption[] {
        const minuteOptions: ISelectOption[] = [
            { value: null, text: "Minute" },
        ];
        for (const value of this.minuteValues) {
            minuteOptions.push({
                value,
                text: value,
            });
        }
        return minuteOptions;
    }

    private get getAmPm(): ISelectOption[] {
        const amPmOptions: ISelectOption[] = [{ value: null, text: "AM/PM" }];
        for (const value of this.amPmValues) {
            amPmOptions.push({
                value,
                text: value,
            });
        }
        return amPmOptions;
    }

    private onChange(): void {
        if (this.hour && this.minute && this.amPm) {
            this.value = this.hour + ":" + this.minute + " " + this.amPm;
        } else {
            this.value = null;
        }

        this.updateModel();
    }

    @Emit("change")
    private updateModel(): string | null {
        return this.value;
    }

    @Watch("model")
    private onModelChanged(): void {
        this.value = this.model;
    }

    @Emit("blur")
    private onBlur(): void {
        return;
    }

    private getClass(): string {
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
