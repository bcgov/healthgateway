<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vue-composition-wrapper";

import BreadcrumbItem from "@/models/breadcrumbItem";

interface Props {
    items?: BreadcrumbItem[];
}
const props = withDefaults(defineProps<Props>(), {
    items: () => [],
});

const homeBreadcrumbItem: BreadcrumbItem = {
    text: "Home",
    to: "/home",
    dataTestId: "breadcrumb-home",
};

const store = useStore();

const isAuthenticated = computed(
    () => store.getters["auth/oidcIsAuthenticated"]
);
const isValidIdentityProvider = computed(
    () => store.getters["user/isValidIdentityProvider"]
);
const allBreadcrumbItems = computed(() => [homeBreadcrumbItem, ...props.items]);
const displayBreadcrumbs = computed(
    () => isAuthenticated.value && isValidIdentityProvider.value
);
</script>

<template>
    <b-breadcrumb
        v-if="displayBreadcrumbs"
        data-testid="breadcrumbs"
        class="pt-0"
        aria-label="Breadcrumb Nav"
    >
        <b-breadcrumb-item
            v-for="item in allBreadcrumbItems"
            :key="item.text"
            :to="item.to"
            :active="item.active"
            :data-testid="item.dataTestId"
        >
            {{ item.text }}
        </b-breadcrumb-item>
    </b-breadcrumb>
</template>

<style scoped>
.breadcrumb {
    padding: 0.75rem 0rem;
    margin-bottom: 0rem;
    background-color: transparent;
}

a {
    cursor: pointer !important;
}
</style>
