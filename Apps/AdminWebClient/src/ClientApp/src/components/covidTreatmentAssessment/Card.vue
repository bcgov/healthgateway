<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

@Component
export default class Card extends Vue {
    @Prop() title!: string;
    @Prop() additionalInfo!: string;
}
</script>

<template>
    <v-row>
        <v-col cols="12">
            <v-card class="h-100 pa-8 d-flex flex-column">
                <div>
                    {{ title }}
                </div>
                <div
                    v-if="additionalInfo"
                    class="pl-4 mt-3 additional-info-color"
                >
                    {{ additionalInfo }}
                </div>
                <div class="pl-4">
                    <slot />
                </div>
            </v-card>
        </v-col>
    </v-row>
</template>
<style scoped lang="scss">
.form-group {
    border: none !important;
}
.additional-info-color {
    color: #4a8bc8;
}
</style>
