import { DisplayInstance } from "vuetify";

export function getGridCols(display: DisplayInstance): number {
    const { md, lg, xlAndUp } = display;
    return xlAndUp.value ? 3 : lg.value ? 4 : md.value ? 6 : 12;
}
