import type { Href } from "expo-router";

/** Narrow a runtime path (from config, params or constants) to expo-router's typed Href. */
export const toHref = (path: string): Href => path as Href;
