<script lang="ts">
import { BCarousel } from "bootstrap-vue";
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";

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

@Component
export default class AppTourComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    webClient!: WebClientConfiguration;

    @Ref("tourCarousel")
    readonly tourCarousel!: BCarousel;

    @Getter("isMobile")
    isMobile!: boolean;

    private mobileSlides: TourSlide[] = [
        {
            title: "App Tour",
            description:
                "Take this brief tour to learn how to use Health Gateway. You can always get here again by clicking the light bulb.",
            imageUri: new URL(
                "@/assets/images/tour/mobile/app-tour-start-mobile.png",
                import.meta.url
            ).href,
            imageAlt: "App tour start mobile splash",
        },
        {
            title: "Add a Quick Link",
            description:
                "Add a quick link to easily access a health record type from your home screen.",
            imageUri: new URL(
                "@/assets/images/tour/mobile/app-tour-home-quick-link-mobile.gif",
                import.meta.url
            ).href,
            imageAlt: "Quick link mobile demo",
        },
        {
            title: "Filter your Timeline records",
            description:
                "Filter by health record type, date or keyword to find what you need.",
            imageUri: new URL(
                "@/assets/images/tour/mobile/app-tour-timeline-filter-mobile.gif",
                import.meta.url
            ).href,
            imageAlt: "Timeline filter mobile demo",
        },
        {
            title: "Notifications centre",
            description:
                "You'll be notified of updates to the app and your health records.",
            imageUri: new URL(
                "@/assets/images/tour/mobile/app-tour-notifications-mobile.gif",
                import.meta.url
            ).href,
            imageAlt: "Notifications centre mobile demo",
        },
    ];

    private desktopSlides: TourSlide[] = [
        {
            title: "App Tour",
            description:
                "Take this brief tour to learn how to use Health Gateway. You can always get here again by clicking the light bulb.",
            imageUri: new URL(
                "@/assets/images/tour/web/app-tour-start.png",
                import.meta.url
            ).href,
            imageAlt: "App tour start splash",
        },
        {
            title: "Add a Quick Link",
            description:
                "Add a quick link to easily access a health record type from your home screen.",
            imageUri: new URL(
                "@/assets/images/tour/web/app-tour-home-quick-link-web.gif",
                import.meta.url
            ).href,
            imageAlt: "Quick link web demo",
        },
        {
            title: "Filter your Timeline records",
            description:
                "Filter by health record type, date or keyword to find what you need.",
            imageUri: new URL(
                "@/assets/images/tour/web/app-tour-timeline-filter-web.gif",
                import.meta.url
            ).href,
            imageAlt: "Timeline filter web demo",
        },
        {
            title: "Notifications centre",
            description:
                "You'll be notified of updates to the app and your health records.",
            imageUri: new URL(
                "@/assets/images/tour/web/app-tour-notification-centre-web.gif",
                import.meta.url
            ).href,
            imageAlt: "Notifications centre web demo",
        },
    ];

    private slideIndex = 0;

    private isVisible = false;

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.slideIndex = 0;
        this.isVisible = false;
    }

    private next(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        if (this.slideIndex < this.slides.length - 1) {
            this.tourCarousel.next();
        }
    }

    private previous(bvModalEvt: Event): void {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        if (this.slideIndex > 0) {
            this.tourCarousel.prev();
        }
    }

    private get slides(): TourSlide[] {
        return this.isMobile ? this.mobileSlides : this.desktopSlides;
    }

    private get currentSlide(): TourSlide | undefined {
        return this.slides.length > 0
            ? this.slides[this.slideIndex]
            : undefined;
    }

    private get isFinalSlide(): boolean {
        return this.slideIndex === this.slides.length - 1;
    }

    private get isFirstSlide(): boolean {
        return this.slideIndex === 0;
    }
}
</script>

<template>
    <b-modal
        id="app-tour"
        :visible="isVisible"
        hide-footer
        hide-header
        centered
        no-close-on-backdrop
        body-class="p-0"
        content-class="overflow-hidden"
        size="lg"
    >
        <b-carousel
            id="carousel-1"
            ref="tourCarousel"
            v-model="slideIndex"
            :interval="0"
            indicators
        >
            <b-carousel-slide
                v-for="slide in slides"
                :key="slide.title"
                :img-src="slide.imageUri"
                :img-alt="slide.imageAlt ?? slide.title"
            />
        </b-carousel>
        <div class="p-3" data-testid="app-tour-modal">
            <b-row v-if="currentSlide">
                <b-col>
                    <h3>{{ currentSlide.title }}</h3>
                    <p v-if="currentSlide.description">
                        {{ currentSlide.description }}
                    </p>
                </b-col>
            </b-row>
            <b-row v-if="isFirstSlide" class="mt-3">
                <b-col cols="3">
                    <hg-button
                        variant="link"
                        data-testid="app-tour-skip"
                        @click="hideModal"
                    >
                        Skip
                    </hg-button>
                </b-col>
                <b-col cols="6" class="d-flex justify-content-center">
                    <hg-button
                        variant="primary"
                        data-testid="app-tour-start"
                        @click="next($event)"
                    >
                        Start App Tour
                    </hg-button>
                </b-col>
            </b-row>
            <b-row v-else-if="!isFirstSlide && !isFinalSlide" class="mt-3">
                <b-col>
                    <hg-button
                        variant="link"
                        data-testid="app-tour-skip"
                        @click="hideModal"
                    >
                        Skip
                    </hg-button>
                </b-col>
                <b-col class="d-flex justify-content-end">
                    <hg-button
                        data-testid="app-tour-back"
                        variant="secondary"
                        @click="previous($event)"
                    >
                        Back
                    </hg-button>
                    <hg-button
                        variant="primary"
                        class="ml-3"
                        data-testid="app-tour-next"
                        @click="next($event)"
                    >
                        Next
                    </hg-button>
                </b-col>
            </b-row>
            <b-row v-else class="mt-3">
                <b-col class="d-flex justify-content-center">
                    <hg-button
                        variant="primary"
                        class="ml-3"
                        data-testid="app-tour-done"
                        @click="hideModal"
                    >
                        Done
                    </hg-button>
                </b-col>
            </b-row>
        </div>
    </b-modal>
</template>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.carousel-indicators {
    li {
        border: 1px solid $hg-text-secondary !important;
        background-color: $hg-text-secondary !important;
        &.active {
            border: 1px solid $hg-text-primary !important;
            background-color: $hg-text-primary !important;
        }
    }
}
</style>
