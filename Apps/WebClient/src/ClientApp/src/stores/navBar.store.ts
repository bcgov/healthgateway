import { create } from "zustand";

interface NavBarState {
    /** The navigation bar's height, as the page on screen reports it (`PageScroll`); undefined until one has. */
    height?: number;
    /** The logo's distance from the window's left edge in the bar (`HeaderLogo`): the bar's own leading margin. */
    leading?: number;
}

/**
 * Native: the platform navigation bar's measurements, for `BrandHeader`, which draws a bar like it over the app (the
 * intro, the lock screen, Update required). The bar's height and margins change with the OS (iOS 26's Liquid Glass bar
 * is taller than the 44pt React Navigation assumes) and the device, so they are read from the real bar, which is
 * mounted under those screens.
 */
export const useNavBarStore = create<NavBarState>(() => ({}));
