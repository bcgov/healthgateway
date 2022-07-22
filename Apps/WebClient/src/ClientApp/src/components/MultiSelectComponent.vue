<script lang="ts">
import Vue from "vue";
import { Component, Emit, Model, Prop, Watch } from "vue-property-decorator";

export interface SelectOption {
    value: string;
    text: string;
    disabled?: boolean;
}

@Component
export default class MultiSelectComponent extends Vue {
    @Prop({ default: "Choose a tag..." }) placeholder!: string;
    @Prop({ default: [] }) options!: SelectOption[];
    @Model("change", { type: Array }) public model!: string[];

    private values: string[] = [];

    private get availableOptions(): SelectOption[] {
        if (this.options.length === 0) {
            return [];
        }

        if (this.values.length === 0) {
            return this.options;
        }

        return this.options.filter(
            (opt) => this.values.indexOf(opt.value) === -1
        );
    }

    @Watch("values")
    private onValueChanged(): void {
        this.updateModel();
    }

    @Watch("model")
    private onModelChanged(): void {
        this.values = this.model;
    }

    @Emit("change")
    private updateModel(): string[] {
        return this.values;
    }

    private getValueText(value: unknown): string | SelectOption | undefined {
        const option = this.options.find(
            (opt) => opt.value === value || opt === value
        );
        return option?.text || option;
    }

    private mounted(): void {
        this.values = this.model;
    }
}
</script>

<template>
    <div>
        <!-- Prop `add-on-change` is needed to enable adding tags vie the `change` event -->
        <b-form-tags
            id="tags-component-select"
            v-model="values"
            size="lg"
            class="mb-2"
            add-on-change
            no-outer-focus
        >
            <template
                #default="{
                    tags,
                    inputAttrs,
                    inputHandlers,
                    disabled,
                    removeTag,
                }"
            >
                <ul
                    v-if="tags.length > 0"
                    class="list-inline d-inline-block mb-2"
                >
                    <li v-for="tag in tags" :key="tag" class="list-inline-item">
                        <b-form-tag
                            :disabled="disabled"
                            :title="tag"
                            :pill="true"
                            variant="danger"
                            @remove="removeTag(tag)"
                        >
                            {{ getValueText(tag) }}
                        </b-form-tag>
                    </li>
                </ul>
                <b-form-select
                    v-bind="inputAttrs"
                    :disabled="disabled || availableOptions.length === 0"
                    :options="availableOptions"
                    v-on="inputHandlers"
                >
                    <template #first>
                        <!-- This is required to prevent bugs with Safari -->
                        <option disabled value="">{{ placeholder }}</option>
                    </template>
                </b-form-select>
            </template>
        </b-form-tags>
    </div>
</template>
