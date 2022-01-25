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
                <a :href="link" data-testid="result-link" target="blank_"
                    >this page</a
                >.
            </span>
        </p>
    </div>
</template>
