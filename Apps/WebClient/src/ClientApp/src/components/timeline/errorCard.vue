<template>
    <b-alert dismissible variant="danger" class="no-print">
        <h4>{{ code }}: {{ title }}</h4>
        <span>{{ description }}</span>
    </b-alert>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import ErrorCard from "@/models/errorCard";

@Component
export default class ErrorCardComponent extends Vue {
    @Prop() error!: ErrorCard;
    private code: string = "";
    private title: string = "";
    private description: string = "";

    private mounted() {
        this.code = this.error.code;
        this.title = this.error.title;
        this.description = this.error.description;
        console.log("Error: ", this.error);
    }
}
</script>
