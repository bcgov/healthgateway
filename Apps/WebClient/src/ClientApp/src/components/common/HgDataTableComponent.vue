<script setup lang="ts">
import { computed, useSlots } from "vue";

import ReportField from "@/models/reportField";

interface Props {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    items: any[];
    fields: ReportField[];
    loading?: boolean;
    density?: "default" | "comfortable" | "compact";
    hover?: boolean;
    height?: string;
}
const props = withDefaults(defineProps<Props>(), {
    height: undefined,
    loading: false,
    density: "default",
    hover: true,
});

const slots = useSlots();

const densityTextSize = computed(() => {
    switch (props.density) {
        case "compact":
            return "text-caption";
        case "comfortable":
            return "text-body-2";
        case "default":
        default:
            return "text-body-1";
    }
});

function getHeaderAlignmentClass(alignment?: string): string {
    switch (alignment) {
        case "start":
            return "text-left";
        case "end":
            return "text-right";
        case "center":
        default:
            return "text-center";
    }
}

function getAlignmentClass(alignment?: string): string {
    switch (alignment) {
        case "start":
            return "text-left";
        case "center":
            return "text-center";
        case "end":
            return "text-right";
        default:
            return "";
    }
}

function hasItemSlot(key: string): boolean {
    return slots[`item-${key}`] !== undefined;
}

function hasHeaderSlot(key: string): boolean {
    return slots[`header-${key}`] !== undefined;
}
</script>

<template>
    <v-table
        v-bind="$attrs"
        :height="height"
        :density="density"
        :hover="hover"
        class="text-center table-fixed"
    >
        <template v-if="loading">
            <v-skeleton-loader
                type="table-thead, table-tbody"
                data-testid="table-skeleton-loader"
            />
        </template>
        <template v-else>
            <thead>
                <tr>
                    <th
                        v-for="field in fields"
                        :key="field.key"
                        v-bind="field.thAttr"
                        :class="getHeaderAlignmentClass(field.thAlign)"
                        :style="field.width ? { width: field.width } : {}"
                        scope="col"
                    >
                        <template v-if="hasHeaderSlot(field.key)">
                            <slot :name="`header-${field.key}`" />
                        </template>
                        <template v-else>
                            {{ field.title }}
                        </template>
                    </th>
                </tr>
            </thead>
            <tbody :class="densityTextSize">
                <tr v-for="(item, itemIndex) in items" :key="item.id">
                    <td
                        v-for="field in fields"
                        :key="field.key"
                        :class="getAlignmentClass(field.tdAlign)"
                        v-bind="field.tdAttr"
                    >
                        <template v-if="hasItemSlot(field.key)">
                            <slot
                                :name="`item-${field.key}`"
                                :item="{ ...item, index: itemIndex }"
                            />
                        </template>
                        <template v-else>
                            {{ item[field.key] }}
                        </template>
                    </td>
                </tr>
            </tbody>
        </template>
    </v-table>
</template>

<style lang="scss">
.table-fixed {
    table {
        table-layout: fixed;
        width: 99%;
    }
}
</style>
