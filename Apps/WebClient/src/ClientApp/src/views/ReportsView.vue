<script setup lang="ts">
import { computed, ref } from "vue";
import { useStore } from "vue-composition-wrapper";

import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import ReportsComponent from "@/components/report/ReportsComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import User from "@/models/user";

const store = useStore();

const user = computed<User>(() => store.getters["user/user"]);

const breadcrumbItems = ref<BreadcrumbItem[]>([
    {
        text: "Export Records",
        to: "/reports",
        active: true,
        dataTestId: "breadcrumb-export-records",
    },
]);
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <page-title title="Export Records" />
        <ReportsComponent :hdid="user.hdid" />
    </div>
</template>
