<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PageErrorComponent from "@/components/PageErrorComponent.vue";
import { PageError } from "@/models/errors";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        PageErrorComponent,
    },
};

@Component(options)
export default class NotFoundView extends Vue {
    public errorDescription = new PageError(
        "404",
        "Page Not Found",
        "The page you were looking for does not exist."
    );
}
</script>

<template>
    <PageErrorComponent :error="errorDescription" />
</template>
