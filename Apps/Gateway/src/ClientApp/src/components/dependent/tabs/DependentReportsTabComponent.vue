<script setup lang="ts">
import ReportsComponent from "@/components/report/ReportsComponent.vue";
import type { Dependent } from "@/models/dependent";

interface Props {
    dependent: Dependent;
}
defineProps<Props>();
</script>

<template>
    <div>
        <ReportsComponent
            :hdid="dependent.dependentInformation.hdid"
            :is-dependent="true"
        />
    </div>
</template>
