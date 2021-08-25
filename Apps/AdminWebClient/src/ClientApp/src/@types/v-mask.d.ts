declare module "v-mask" {
    import { DirectiveFunction } from "vue";

    interface VueMaskDirectiveType {
        bind: DirectiveFunction;
        componentUpdated: DirectiveFunction;
        unbind: DirectiveFunction;
    }
    export const VueMaskDirective: VueMaskDirectiveType;
}
