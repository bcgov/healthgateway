<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import LoadingComponent from "@/components/loading.vue";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class DependentsView extends Vue {
    private isLoading = true;

    private mounted() {
        this.isLoading = false;
    }
}
</script>
<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <b-row class="my-3 fluid">
            <b-col class="col-12 col-lg-9 column-wrapper">
                <b-row>
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="Subject">Dependents</h1>
                            <hr />
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </div>
</template>
<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
