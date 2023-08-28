import { computed } from "vue";
import { useDisplay } from "vuetify";

export const getGridCols = computed(() => {
    const { md, lg, xlAndUp } = useDisplay();

    if (xlAndUp.value) {
        return 3;
    } else if (lg.value) {
        return 4;
    } else if (md.value) {
        return 6;
    } else {
        return 12;
    }
});
