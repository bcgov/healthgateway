<template>
    <div>
        <b-alert show="show" variant="danger" dismissible class="no-print">
            <h4>{{ getCodeText() }} {{ title }}</h4>
            <span>{{ description }}</span>
        </b-alert>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class ErrorCardComponent extends Vue {
    @Prop() title: string;
    @Prop() description: string;
    @Prop() code: string;
    @Prop() show: boolean;

    private getCodeText(): string {
        if (!this.code) {
            return "";
        } else {
            return this.code + ": ";
        }
    }
}
</script>
