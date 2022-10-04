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
export default class UnauthorizedView extends Vue {
    public errorDescription = new PageError(
        "401",
        "Unauthorized",
        "You do not have permission to view this page."
    );
}
</script>

<template>
    <PageErrorComponent :error="errorDescription" />
</template>
