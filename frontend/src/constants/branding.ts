/**
 * Shared geometry for the splash → login logo morph.
 *
 * The boot screen (src/app/index.tsx) animates the logo from screen-centre down
 * onto the login screen's logo (src/app/(auth)/login.tsx). Both screens must
 * agree on the size and the login logo's top offset so the hand-off is seamless.
 */

/** Logo size at rest, on the login screen (and the morph's end state). */
export const BRAND_LOGO_SIZE = 96;

/** Logo size while the splash is loading (the morph's start state). */
export const BRAND_LOGO_SPLASH_SIZE = 200;

/** Padding from the safe-area top to the login logo. */
export const BRAND_LOGO_TOP_PADDING = 48;
