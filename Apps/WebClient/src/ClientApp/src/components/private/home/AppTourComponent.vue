﻿<script setup lang="ts">
import { computed, ref } from "vue";
import { useDisplay } from "vuetify";
import { VCarousel } from "vuetify/components";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";

interface Props {
    highlightTourChangeIndicator: boolean;
    isAvailable: boolean;
}
defineProps<Props>();

defineExpose({ showDialog });

interface TourSlide {
    // Title to be displayed for the slide.
    title: string;
    // Description to be displayed for the slide.
    description?: string;
    // Image or Gif to be displayed for the slide.
    imageUri: string;
    // Optional alt text for the image.
    imageAlt?: string;
}

const desktopSlides: TourSlide[] = [
    {
        title: "App Tour",
        description:
            "Take this brief tour to learn how to use Health Gateway. You can always get here again by clicking the light bulb.",
        imageUri: new URL(
            "@/assets/images/tour/web/at-intro.png",
            import.meta.url
        ).href,
        imageAlt: "App tour start splash",
    },
    {
        title: "Add a Quick Link",
        description:
            "Add a quick link to easily access a health record type from your home screen.",
        imageUri: new URL(
            "@/assets/images/tour/web/at-quick-link.png",
            import.meta.url
        ).href,
        imageAlt: "Quick link web demo",
    },
    {
        title: "Filter your Timeline records",
        description:
            "Filter by health record type, date or keyword to find what you need.",
        imageUri: new URL(
            "@/assets/images/tour/web/at-filter.png",
            import.meta.url
        ).href,
        imageAlt: "Timeline filter web demo",
    },
    {
        title: "Notifications centre",
        description:
            "You'll be notified of updates to the app and your health records.",
        imageUri: new URL(
            "@/assets/images/tour/web/at-notifications-web.png",
            import.meta.url
        ).href,
        imageAlt: "Notifications centre web demo",
    },
];

const { mdAndUp } = useDisplay();

const slideIndex = ref(0);
const isVisible = ref(false);

const slides = computed<TourSlide[]>(() => {
    // (MOBILE) Requires new assets for mobile, currently returning only the desktopSlides
    //    1. Reimplement checks for store's isMobile getter
    //    2. Introduce mobileSlides array
    //    3. Uncomment return statement below, naturally remove the current return desktopSlides.
    // return this.isMobile ? mobileSlides : desktopSlides;
    return desktopSlides;
});

const currentSlide = computed<TourSlide | undefined>(() => {
    return slides.value.length > 0 ? slides.value[slideIndex.value] : undefined;
});

const isFinalSlide = computed<boolean>(() => {
    return slideIndex.value === slides.value.length - 1;
});

const isFirstSlide = computed<boolean>(() => {
    return slideIndex.value === 0;
});

function showDialog(): void {
    isVisible.value = true;
}

function hideDialog(): void {
    slideIndex.value = 0;
    isVisible.value = false;
}

function next(): void {
    slideIndex.value++;
}

function previous(): void {
    slideIndex.value--;
}
</script>

<template>
    <div class="d-flex justify-center">
        <v-dialog
            id="app-tour"
            v-model="isVisible"
            body-class="p-0"
            persistent
            no-click-animation
            max-width="675px"
        >
            <template #activator="{ props }">
                <HgIconButtonComponent
                    v-if="isAvailable"
                    v-bind="props"
                    data-testid="app-tour-button"
                >
                    <v-badge
                        color="red"
                        :model-value="highlightTourChangeIndicator"
                    >
                        <v-icon icon="lightbulb" />
                    </v-badge>
                </HgIconButtonComponent>
            </template>
            <v-card data-testid="app-tour-modal">
                <v-card-text class="pa-0">
                    <v-carousel
                        id="tourCarousel"
                        v-model="slideIndex"
                        :continuous="false"
                        :show-arrows="false"
                        hide-delimiter-background
                        :height="mdAndUp ? 480 : 250"
                        class="overflow-hidden"
                    >
                        <v-carousel-item
                            v-for="slide in slides"
                            :key="slide.title"
                            :src="slide.imageUri"
                            :alt="slide.imageAlt ?? slide.title"
                            transition="scroll-x-reverse-transition"
                        />
                    </v-carousel>
                    <div v-if="currentSlide" class="pa-4">
                        <h3 class="text-h6 font-weight-bold">
                            {{ currentSlide.title }}
                        </h3>
                        <p v-if="currentSlide.description" class="text-body-1">
                            {{ currentSlide.description }}
                        </p>
                    </div>
                </v-card-text>
                <template #actions>
                    <v-row v-if="isFirstSlide" no-gutters>
                        <v-col cols="3">
                            <HgButtonComponent
                                variant="link"
                                data-testid="app-tour-skip"
                                text="Skip"
                                @click="hideDialog"
                            />
                        </v-col>
                        <v-col cols="6" class="d-flex justify-center">
                            <HgButtonComponent
                                variant="primary"
                                data-testid="app-tour-start"
                                text="Start Tour"
                                @click="next()"
                            />
                        </v-col>
                    </v-row>
                    <v-row
                        v-else-if="!isFirstSlide && !isFinalSlide"
                        no-gutters
                    >
                        <v-col>
                            <HgButtonComponent
                                variant="link"
                                data-testid="app-tour-skip"
                                text="Skip"
                                @click="hideDialog"
                            />
                        </v-col>
                        <v-col class="d-flex justify-end">
                            <HgButtonComponent
                                data-testid="app-tour-back"
                                variant="secondary"
                                text="Back"
                                @click="previous()"
                            />
                            <HgButtonComponent
                                variant="primary"
                                class="ml-4"
                                data-testid="app-tour-next"
                                text="Next"
                                @click="next()"
                            />
                        </v-col>
                    </v-row>
                    <div v-else class="d-flex flex-grow-1 justify-center">
                        <HgButtonComponent
                            variant="primary"
                            data-testid="app-tour-done"
                            text="Done"
                            @click="hideDialog"
                        />
                    </div>
                </template>
            </v-card>
        </v-dialog>
    </div>
</template>
