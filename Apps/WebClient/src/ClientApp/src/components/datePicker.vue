<script lang="ts">
import { BFormDatepicker } from "bootstrap-vue";
import Vue from "vue";
import {
    Component,
    Emit,
    Model,
    Prop,
    Ref,
    Watch,
} from "vue-property-decorator";
import { minLength } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { DateWrapper } from "@/models/dateWrapper";

@Component
export default class DatePickerComponent extends Vue {
    @Model("change", { type: String }) public model!: string;
    @Prop() state?: boolean;
    @Ref("datePicker") datePicker: BFormDatepicker;

    private inputValue = "";

    private mounted() {
        this.inputValue = this.model;
    }

    private onFocus() {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (this.datePicker.$refs.control as any).show();
    }

    @Watch("inputValue")
    @Emit("change")
    private onInputChanged() {
        this.$v.inputValue.$touch();
        return this.isValid(this.$v.inputValue) ? this.inputValue : "";
    }

    @Emit("blur")
    private onBlur() {
        return;
    }

    private get getState() {
        let isValid = this.isValid(this.$v.inputValue);
        if (isValid) {
            return this.state;
        }
        return isValid;
    }

    private validations() {
        return {
            inputValue: {
                minLength: minLength(10),
                minValue: (value: string) =>
                    new DateWrapper(value).isAfter(this.minDate),
                maxValue: (value: string) =>
                    new DateWrapper(value).isBefore(this.maxDate),
            },
        };
    }

    private get minDate(): DateWrapper {
        return new DateWrapper("1900-01-01");
    }
    private get maxDate(): DateWrapper {
        return new DateWrapper("2100-01-01");
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }
}
</script>

<template>
    <b-input-group>
        <b-form-input
            v-model="inputValue"
            v-mask="'####-##-##'"
            type="text"
            placeholder="YYYY-MM-DD"
            autocomplete="off"
            :state="getState"
            @focus.native="onFocus"
            @blur.native="onBlur"
            @click.native.capture.stop
        ></b-form-input>
        <b-input-group-append>
            <b-form-datepicker
                ref="datePicker"
                v-model="inputValue"
                button-only
                right
                locale="en-CA"
            ></b-form-datepicker>
        </b-input-group-append>
    </b-input-group>
</template>
