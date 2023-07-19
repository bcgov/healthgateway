<script setup lang="ts">
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();
</script>

<template>
    <div
        data-testid="hg-resource-centre"
        class="resource-centre-position text-right pa-4"
        :class="{ mobile: appStore.isMobile }"
    >
        <v-menu location="top">
            <template #activator="{ props }">
                <HgIconButtonComponent
                    v-bind="props"
                    icon="question"
                    size="x-large"
                    elevation="2"
                    color="secondary"
                />
            </template>
            <v-list>
                <v-list-item>
                    <v-list-title class="font-weight-bold"
                        >Resource Centre</v-list-title
                    >
                </v-list-item>
                <v-divider />
                <v-list-item
                    href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                    target="_blank"
                    title="FAQ"
                />
                <v-list-item to="/release-notes" title="Release Notes" />
            </v-list>
        </v-menu>
    </div>
</template>

<style scoped lang="scss">
.resource-centre-position {
    position: fixed;
    bottom: 56px;
    right: 0;
    &.mobile {
        bottom: 0;
    }
}
</style>
