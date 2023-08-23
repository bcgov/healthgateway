<script setup lang="ts">
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { Path } from "@/constants/path";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();
</script>

<template>
    <div
        data-testid="hg-resource-centre"
        class="pa-4"
        :class="
            appStore.isMobile
                ? 'resource-centre-position--mobile'
                : 'resource-centre-position'
        "
    >
        <v-menu location="top">
            <template #activator="{ props }">
                <HgIconButtonComponent
                    v-bind="props"
                    icon="question"
                    size="large"
                    elevation="8"
                    color="info"
                />
            </template>
            <v-list>
                <v-list-item>
                    <v-list-item-title class="font-weight-bold">
                        Resource Centre
                    </v-list-item-title>
                </v-list-item>
                <v-divider />
                <v-list-item
                    href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                    target="_blank"
                    title="FAQ"
                />
                <v-list-item :to="Path.ReleaseNotes" title="Release Notes" />
            </v-list>
        </v-menu>
    </div>
</template>

<style scoped lang="scss">
.resource-centre-position {
    position: absolute;
    bottom: 56px;
    right: 0;
    &--mobile {
        @extend .resource-centre-position;
        bottom: 0;
    }
}
</style>
