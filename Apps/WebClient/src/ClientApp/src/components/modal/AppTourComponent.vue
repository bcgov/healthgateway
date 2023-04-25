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
        // TODO: Final configuration in AB#15392
        {
            title: "MB-Add a Quick Link",
            description:
                "Add a quick link to easily access a health record type from your home screen.",
            imageUri: new URL(
                "@/assets/images/tour/add-quicklink.png",
                import.meta.url
            ).href,
            imageAlt: "First slide",
        },
        {
            title: "MB-Filter Timeline Data",
            description: "Follow the demo above to filter your timeline data.",
            imageUri: new URL(
                "@/assets/images/tour/filter-timeline.gif",
                import.meta.url
            ).href,
            imageAlt: "Second slide",
        },
    ];

    private desktopSlides: TourSlide[] = [
        // TODO: Final configuration in AB#15392
        {
            title: "Add a Quick Link",
            description:
                "Add a quick link to easily access a health record type from your home screen.",
            imageUri: new URL(
                "@/assets/images/tour/add-quicklink.png",
                import.meta.url
            ).href,
            imageAlt: "First slide",
        },
        {
            title: "Filter Timeline Data",
            description: "Follow the demo above to filter your timeline data.",
            imageUri: new URL(
                "@/assets/images/tour/filter-timeline.gif",
                import.meta.url
            ).href,
            imageAlt: "Second slide",
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
            <b-row v-if="!isFinalSlide">
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
                        :disabled="isFirstSlide"
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
            <b-row v-else>
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
        border: 1px solid $hg-text-primary !important;
        &.active {
            background-color: $hg-text-primary !important;
        }
    }
}
</style>
