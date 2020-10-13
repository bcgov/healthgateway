<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import LoadingComponent from "@/components/loading.vue";
import NewDependentComponent from "@/components/modal/newDependent.vue";

@Component({
    components: {
        LoadingComponent,
        NewDependentComponent,
    },
})
export default class DependentsView extends Vue {
    @Ref("newDependentModal")
    readonly newDependentModal!: NewDependentComponent;
    private isLoading = true;

    private mounted() {
        this.isLoading = false;
    }

    private showModal() {
        this.newDependentModal.showModal();
    }

    private hideModal() {
        this.newDependentModal.hideModal();
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
                        <b-row id="pageTitle">
                            <b-col cols="8">
                                <h1 id="Subject">Dependents</h1>
                            </b-col>
                            <b-col cols="4" align-self="end">
                                <b-btn
                                    variant="primary"
                                    class="float-right"
                                    @click="showModal()"
                                >
                                    <font-awesome-icon
                                        icon="user-plus"
                                        class="mr-2"
                                    >
                                    </font-awesome-icon
                                    >Add a new dependent</b-btn
                                >
                            </b-col>
                        </b-row>
                        <b-row id="pageTitle">
                            <b-col>
                                <hr />
                            </b-col>
                        </b-row>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <NewDependentComponent ref="newDependentModal" @show="showModal" />
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
