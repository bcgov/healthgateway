<script lang="ts">
import { Component, Prop, Vue } from "vue-property-decorator";

@Component
export default class Card extends Vue {
    @Prop() questionSequence!: string;
    @Prop() value!: string;
    @Prop() haveAdditionalInfo!: boolean;
    @Prop() additionalInfo!: string;
    @Prop() haveNotSureOption!: boolean;
    @Prop() selected!: string;

    private getQuestioner(): string {
        switch (this.questionSequence) {
            case "a": {
                return "Do you identify as Indigenous";
                break;
            }
            case "b": {
                return "Do you have a family doctor or nurse practitioner?";
                break;
            }
            case "1": {
                return "1. Please confirm that you are over 12 years or older.";
                break;
            }
            case "2": {
                return "2. Have you recently tested positive for COVID-19 in the last 7 days?";
                break;
            }
            case "3": {
                return "3. Do you have any severe symptoms of COVID-19?";
                break;
            }
            case "4": {
                return "4. COVID-19 symptoms can range from mild to moderate. Mild and moderate symptoms are symptoms that can be managed at home. Do you have any symptoms of COVID-19?";
                break;
            }
            case "5": {
                return "5. When did your symptoms first start?";
                break;
            }
            case "6": {
                return "6. Do you have a medical condition or are you taking medications that suppress or weaken your immune system?";
                break;
            }
            case "7": {
                return "7. Have you had 3 doses of the vaccine?";
                break;
            }
            case "8": {
                return "8. Have you been diagnosed by a health care provider with a chronic condition?";
                break;
            }
            default:
                return "";
        }
    }
}
</script>

<template>
    <v-row>
        <v-col cols="8">
            <v-card class="h-100 p-4 d-flex flex-column">
                <div class="ml-5 pt-5">
                    {{ getQuestioner() }}
                </div>
                <div
                    v-if="haveAdditionalInfo"
                    class="ml-5 pl-6 pt-2"
                    style="color: #4a8bc8"
                >
                    {{ additionalInfo }}
                </div>
                <div class="ml-5 pl-6 pt-2 pb-4">
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
</style>
