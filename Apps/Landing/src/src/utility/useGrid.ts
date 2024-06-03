import { computed } from "vue";
import { useDisplay } from "vuetify";

export function useGrid() {
    const { md, lg, xlAndUp } = useDisplay();

    const columns = computed(() => {
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

    return { columns };
}
