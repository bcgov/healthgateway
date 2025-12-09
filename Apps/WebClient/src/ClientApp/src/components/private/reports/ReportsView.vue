<script setup lang="ts">
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import ReportsComponent from "@/components/private/reports/ReportsComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { useUserStore } from "@/stores/user";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Download",
        to: "/reports",
        active: true,
        dataTestId: "breadcrumb-download-records",
    },
];

const userStore = useUserStore();
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <PageTitleComponent title="Download" />
        <ReportsComponent :hdid="userStore.hdid" />
    </div>
</template>
