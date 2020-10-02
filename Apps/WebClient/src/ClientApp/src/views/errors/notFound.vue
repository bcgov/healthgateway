<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import PageErrorComponent from "@/components/error.vue";
import PageError from "@/models/pageError";

@Component({
    components: {
        ErrorComponent: PageErrorComponent,
    },
})
export default class NotFoundView extends Vue {
    public errorDescription: PageError = new PageError(
        "404",
        "Page not Found",
        "The page you where looking for does not exist."
    );
}
</script>

<template>
    <div>
        <ErrorComponent :error="errorDescription" />
    </div>
</template>
