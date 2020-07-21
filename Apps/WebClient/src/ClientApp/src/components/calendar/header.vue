<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.calendar-header {
    .btn-outline-primary {
        font-size: 2em;
        background-color: white;
    }
    .btn-outline-primary:focus {
        color: $primary;
        background-color: white;
    }
    .btn-outline-primary:hover {
        color: white;
        background-color: $primary;
    }
    .btn-outline-primary:active {
        color: white;
    }

    .arrow-icon {
        font-size: 1em;
    }
    .left-button {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }
    .right-button {
        border-radius: 0px 5px 5px 0px;
    }

    .title {
        font-size: 1.5em;
    }
}
</style>
<template>
    <b-row class="calendar-header">
        <b-col cols="auto" class="p-0">
            <b-btn
                class="arrow-icon left-button btn-outline-primary px-2 m-0"
                @click.stop="previousMonth"
            >
                <font-awesome-icon :icon="leftIcon" />
            </b-btn>
        </b-col>
        <b-col cols="auto" class="p-0">
            <b-btn
                class="arrow-icon right-button btn-outline-primary px-2 m-0"
                @click.stop="nextMonth"
            >
                <font-awesome-icon :icon="rightIcon" />
            </b-btn>
        </b-col>
        <b-col cols="auto" class="mx-4">
            <span class="title">{{ title }}</span>
        </b-col>
        <b-col class="header-right">
            <slot name="header-right"> </slot>
        </b-col>
    </b-row>
</template>
<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import moment from "moment";
import CalendarBody from "./body.vue";
import DateUtil from "@/utility/dateUtil";

@Component({
    components: {
        CalendarBody,
    },
})
export default class CalendarComponent extends Vue {
    @Prop() currentDate!: Date;
    @Prop() titleFormat!: string;
    @Prop() availiableMonths!: Date[];

    private monthIndex: number = 0;
    private headerDate: Date = new Date();
    private leftIcon: string = "chevron-left";
    private rightIcon: string = "chevron-right";

    public get title(): string {
        return moment(this.currentDate).format(this.titleFormat);
    }

    @Watch("availiableMonths")
    public onAvailiableMonthsChange() {
        if (this.monthIndex !== 0) {
            this.monthIndex = 0;
        } else {
            this.onMonthIndexChange();
        }
    }

    @Watch("monthIndex")
    public onMonthIndexChange() {
        this.headerDate = this.availiableMonths[this.monthIndex];
        this.dispatchEvent();
    }

    private created() {
        this.dispatchEvent();
    }

    private previousMonth() {
        if (this.monthIndex + 1 < this.availiableMonths.length) {
            this.monthIndex += 1;
        }
    }

    private nextMonth() {
        if (this.monthIndex > 0) {
            this.monthIndex -= 1;
        }
    }

    private dispatchEvent() {
        let startDate = DateUtil.getMonthFirstDate(this.headerDate);
        this.$emit("update:currentDate", startDate);
    }
}
</script>
