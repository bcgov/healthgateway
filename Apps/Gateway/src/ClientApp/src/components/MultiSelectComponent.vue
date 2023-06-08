<script setup lang="ts">
import { computed } from "vue";

import SelectOption from "@/models/selectOption";

interface Props {
    values: string[];
    placeholder?: string;
    options?: SelectOption[];
}
const props = withDefaults(defineProps<Props>(), {
    placeholder: "Choose a tag...",
    options: () => [] as SelectOption[],
});

const emit = defineEmits<{
    (e: "update:values", values: string[]): void;
}>();

const wrappedValues = computed<string[]>({
    get() {
        return props.values;
    },
    set(values: string[]) {
        emit("update:values", values);
    },
});

const availableOptions = computed<SelectOption[]>(() => {
    if (props.options.length === 0) {
        return [];
    }

    if (wrappedValues.value.length === 0) {
        return props.options;
    }

    return props.options.filter(
        (opt) =>
            opt.value !== null &&
            wrappedValues.value.indexOf(opt.value.toString()) === -1
    );
});

function getValueText(value: unknown): string | SelectOption | undefined {
    const option = props.options.find(
        (opt) => opt.value === value || opt === value
    );
    return option?.text || option;
}
</script>

<template>
    <div>
        <!-- Prop `add-on-change` is needed to enable adding tags via the `change` event -->
        <b-form-tags
            id="tags-component-select"
            v-model="wrappedValues"
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
                            variant="light"
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
