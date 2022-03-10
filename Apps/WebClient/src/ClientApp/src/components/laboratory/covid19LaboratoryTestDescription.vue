<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class Covid19LaboratoryTestDescriptionComponent extends Vue {
    @Prop({ required: true }) description!: string[];
    @Prop({ required: true }) link!: string;

    private isLastEntry(index: number) {
        return index + 1 === this.description.length;
    }

    private shouldDisplayLink(index: number) {
        return this.isLastEntry(index) && this.link;
    }

    private displayLink() {
        window.open(this.link, "_blank");
    }
}
</script>

<template>
    <div>
        <p
            v-for="(paragraph, index) in description"
            :key="index"
            :class="{ 'mb-0': isLastEntry(index) }"
        >
            <span>{{ paragraph }}</span>
            <span v-if="shouldDisplayLink(index)">
                <a data-testid="result-link" @click="displayLink">this page</a>.
            </span>
        </p>
    </div>
</template>

<style scoped>
a {
    color: blue !important;
    text-decoration: underline !important;
    cursor: pointer !important;
}
</style>
