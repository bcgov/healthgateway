<script setup lang="ts">
import { computed } from "vue";

import HgCardComponent from "@/components/common/HgCardComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { getOtherRecordSourcesLinks } from "@/constants/accessLinks";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { useGrid } from "@/utility/useGrid";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Other record sources",
        to: "/otherRecordSources",
        active: true,
        dataTestId: "breadcrumb-other-record-sources",
    },
];

const { columns } = useGrid();
const otherRecordSourcesLinks = computed(() => getOtherRecordSourcesLinks());
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <PageTitleComponent title="Other record sources" />
    <p>
        Your digital health record starts at the place where you get care.
        Health Gateway helps bring your records together in one place. It
        connects to many record sources, but not all. If your records aren’t
        showing up in Health Gateway, they may be available through other
        trusted websites. Not sure where to go?
        <a
            href="https://www.healthlinkbc.ca/health-library/health-features/your-health-information"
            target="_blank"
            rel="noopener"
            class="text-link"
        >
            Learn more about where your records can be found besides Health
            Gateway </a
        >.
    </p>
    <div class="mt-6 mt-md-8">
        <v-row>
            <v-col
                v-for="tile in otherRecordSourcesLinks"
                :key="tile.name"
                :cols="columns"
                class="d-flex"
            >
                <HgCardComponent
                    :title="tile.name"
                    variant="outlined"
                    elevation="1"
                    border="thin grey-lighten-2"
                    :data-testid="`other-record-sources-card-${tile.type}`"
                    class="flex-grow-1"
                    compact-header
                >
                    <template #icon>
                        <img v-if="tile.logoUri" :src="tile.logoUri" />
                    </template>
                    <p class="text-body-1">{{ tile.description }}</p>
                    <div class="mt-auto pt-3 text-start">
                        <a
                            rel="noopener"
                            target="_blank"
                            class="text-link"
                            :href="tile.link"
                            :data-testid="`link-${tile.type}`"
                            >{{ tile.linkText }}</a
                        >
                    </div>
                </HgCardComponent>
            </v-col>
        </v-row>
    </div>
</template>
