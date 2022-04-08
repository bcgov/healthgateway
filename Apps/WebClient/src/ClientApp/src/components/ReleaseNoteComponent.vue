<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class ReleaseNoteComponent extends Vue {
    @Prop({ required: true }) date!: string;
    @Prop({ required: true }) version!: string;
    @Prop({ required: true }) title!: string;

    private get hasSlot() {
        return this.$slots.default !== undefined;
    }
}
</script>

<template>
    <div>
        <b-row class="mb-2">
            <b-col>
                <strong>{{ date }} – {{ version }} – {{ title }}</strong>
            </b-col>
        </b-row>
        <b-row v-if="hasSlot">
            <b-col><slot /></b-col>
        </b-row>
    </div>
</template>
