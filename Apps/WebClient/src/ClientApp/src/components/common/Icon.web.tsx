/* eslint-disable @typescript-eslint/no-unsafe-assignment -- @expo/vector-icons does not type its props */
import FontAwesome6 from "@expo/vector-icons/FontAwesome6";
import type { IconDefinition } from "@fortawesome/fontawesome-common-types";
import { faCircleQuestion as faCircleQuestionRegular } from "@fortawesome/free-regular-svg-icons/faCircleQuestion";
import { faComment as faCommentRegular } from "@fortawesome/free-regular-svg-icons/faComment";
import { faSquare as faSquareRegular } from "@fortawesome/free-regular-svg-icons/faSquare";
import { faSquareCheck as faSquareCheckRegular } from "@fortawesome/free-regular-svg-icons/faSquareCheck";
import { faStar as faStarRegular } from "@fortawesome/free-regular-svg-icons/faStar";
import { faAddressCard } from "@fortawesome/free-solid-svg-icons/faAddressCard";
import { faAnglesRight } from "@fortawesome/free-solid-svg-icons/faAnglesRight";
import { faArrowLeft } from "@fortawesome/free-solid-svg-icons/faArrowLeft";
import { faArrowRotateLeft } from "@fortawesome/free-solid-svg-icons/faArrowRotateLeft";
import { faArrowUp } from "@fortawesome/free-solid-svg-icons/faArrowUp";
import { faArrowUpRightFromSquare } from "@fortawesome/free-solid-svg-icons/faArrowUpRightFromSquare";
import { faBars } from "@fortawesome/free-solid-svg-icons/faBars";
import { faBell } from "@fortawesome/free-solid-svg-icons/faBell";
import { faBoxArchive } from "@fortawesome/free-solid-svg-icons/faBoxArchive";
import { faCalendar } from "@fortawesome/free-solid-svg-icons/faCalendar";
import { faCalendarCheck } from "@fortawesome/free-solid-svg-icons/faCalendarCheck";
import { faCaretDown } from "@fortawesome/free-solid-svg-icons/faCaretDown";
import { faCheck } from "@fortawesome/free-solid-svg-icons/faCheck";
import { faChevronDown } from "@fortawesome/free-solid-svg-icons/faChevronDown";
import { faChevronLeft } from "@fortawesome/free-solid-svg-icons/faChevronLeft";
import { faChevronRight } from "@fortawesome/free-solid-svg-icons/faChevronRight";
import { faChevronUp } from "@fortawesome/free-solid-svg-icons/faChevronUp";
import { faCircleCheck } from "@fortawesome/free-solid-svg-icons/faCircleCheck";
import { faCircleHalfStroke } from "@fortawesome/free-solid-svg-icons/faCircleHalfStroke";
import { faCircleInfo } from "@fortawesome/free-solid-svg-icons/faCircleInfo";
import { faCircleQuestion } from "@fortawesome/free-solid-svg-icons/faCircleQuestion";
import { faCircleXmark } from "@fortawesome/free-solid-svg-icons/faCircleXmark";
import { faCloudArrowDown } from "@fortawesome/free-solid-svg-icons/faCloudArrowDown";
import { faComments } from "@fortawesome/free-solid-svg-icons/faComments";
import { faCopy } from "@fortawesome/free-solid-svg-icons/faCopy";
import { faDownload } from "@fortawesome/free-solid-svg-icons/faDownload";
import { faEllipsisVertical } from "@fortawesome/free-solid-svg-icons/faEllipsisVertical";
import { faEye } from "@fortawesome/free-solid-svg-icons/faEye";
import { faFileLines } from "@fortawesome/free-solid-svg-icons/faFileLines";
import { faFileMedical } from "@fortawesome/free-solid-svg-icons/faFileMedical";
import { faFileWaveform } from "@fortawesome/free-solid-svg-icons/faFileWaveform";
import { faFilter } from "@fortawesome/free-solid-svg-icons/faFilter";
import { faGear } from "@fortawesome/free-solid-svg-icons/faGear";
import { faHandHoldingMedical } from "@fortawesome/free-solid-svg-icons/faHandHoldingMedical";
import { faHouse } from "@fortawesome/free-solid-svg-icons/faHouse";
import { faHouseMedical } from "@fortawesome/free-solid-svg-icons/faHouseMedical";
import { faLock } from "@fortawesome/free-solid-svg-icons/faLock";
import { faMagnifyingGlass } from "@fortawesome/free-solid-svg-icons/faMagnifyingGlass";
import { faMicroscope } from "@fortawesome/free-solid-svg-icons/faMicroscope";
import { faMoon } from "@fortawesome/free-solid-svg-icons/faMoon";
import { faPaperclip } from "@fortawesome/free-solid-svg-icons/faPaperclip";
import { faPenToSquare } from "@fortawesome/free-solid-svg-icons/faPenToSquare";
import { faPersonDigging } from "@fortawesome/free-solid-svg-icons/faPersonDigging";
import { faPills } from "@fortawesome/free-solid-svg-icons/faPills";
import { faPlus } from "@fortawesome/free-solid-svg-icons/faPlus";
import { faRibbon } from "@fortawesome/free-solid-svg-icons/faRibbon";
import { faRightFromBracket } from "@fortawesome/free-solid-svg-icons/faRightFromBracket";
import { faRightToBracket } from "@fortawesome/free-solid-svg-icons/faRightToBracket";
import { faShield } from "@fortawesome/free-solid-svg-icons/faShield";
import { faSquare } from "@fortawesome/free-solid-svg-icons/faSquare";
import { faSquareCheck } from "@fortawesome/free-solid-svg-icons/faSquareCheck";
import { faStar } from "@fortawesome/free-solid-svg-icons/faStar";
import { faStethoscope } from "@fortawesome/free-solid-svg-icons/faStethoscope";
import { faSun } from "@fortawesome/free-solid-svg-icons/faSun";
import { faSyringe } from "@fortawesome/free-solid-svg-icons/faSyringe";
import { faTriangleExclamation } from "@fortawesome/free-solid-svg-icons/faTriangleExclamation";
import { faUser } from "@fortawesome/free-solid-svg-icons/faUser";
import { faUserGroup } from "@fortawesome/free-solid-svg-icons/faUserGroup";
import { faUserPlus } from "@fortawesome/free-solid-svg-icons/faUserPlus";
import { faUserSecret } from "@fortawesome/free-solid-svg-icons/faUserSecret";
import { faVial } from "@fortawesome/free-solid-svg-icons/faVial";
import { faWifi } from "@fortawesome/free-solid-svg-icons/faWifi";
import { faXmark } from "@fortawesome/free-solid-svg-icons/faXmark";
import { faXRay } from "@fortawesome/free-solid-svg-icons/faXRay";
import type { ComponentProps } from "react";
import Svg, { Path } from "react-native-svg";

import { logger } from "@/services/logger";

type Props = Omit<ComponentProps<typeof FontAwesome6>, "name"> & {
    name: string;
    /**
     * v-icon's box: the glyph `size` tall at its own width, centred in a box 1.25 × size wide (Font Awesome's
     * fixed width), as the web client draws a Font Awesome icon in a v-icon. Without it the glyph fits a square,
     * so a wide one (cloud-arrow-down, pills) draws shorter.
     */
    fixedWidth?: boolean;
};

/**
 * Web: FontAwesome as inline SVG from the same packages the web client uses (`@fortawesome/free-*-svg-icons`).
 * Each icon is imported from its own file (Metro does not tree-shake the package index), so only these reach the bundle, instead of the 424 KB FontAwesome 6 icon font that
 * `@expo/vector-icons` loads on web. Native keeps the icon font (see Icon.tsx). Add new icons to these maps;
 * `Icon.test.ts` checks that every name used in `src` is covered. Unmapped names fall back to the font.
 */
const solid: Record<string, IconDefinition> = {
    bars: faBars,
    "box-archive": faBoxArchive,
    "arrow-left": faArrowLeft,
    "arrow-up": faArrowUp,
    calendar: faCalendar,
    "caret-down": faCaretDown,
    "user-plus": faUserPlus,
    "calendar-check": faCalendarCheck,
    "angles-right": faAnglesRight,
    bell: faBell,
    "chevron-down": faChevronDown,
    "chevron-left": faChevronLeft,
    "chevron-right": faChevronRight,
    "chevron-up": faChevronUp,
    "circle-check": faCircleCheck,
    "circle-half-stroke": faCircleHalfStroke,
    "circle-info": faCircleInfo,
    "circle-question": faCircleQuestion,
    "file-lines": faFileLines,
    "address-card": faAddressCard,
    "arrow-up-right-from-square": faArrowUpRightFromSquare,
    "circle-xmark": faCircleXmark,
    "cloud-arrow-down": faCloudArrowDown,
    comments: faComments,
    copy: faCopy,
    download: faDownload,
    "ellipsis-vertical": faEllipsisVertical,
    eye: faEye,
    "file-medical": faFileMedical,
    "file-waveform": faFileWaveform,
    "hand-holding-medical": faHandHoldingMedical,
    filter: faFilter,
    house: faHouse,
    "house-medical": faHouseMedical,
    lock: faLock,
    "magnifying-glass": faMagnifyingGlass,
    microscope: faMicroscope,
    gear: faGear,
    check: faCheck,
    moon: faMoon,
    wifi: faWifi,
    shield: faShield,
    "arrow-rotate-left": faArrowRotateLeft,
    paperclip: faPaperclip,
    "pen-to-square": faPenToSquare,
    "person-digging": faPersonDigging,
    pills: faPills,
    plus: faPlus,
    ribbon: faRibbon,
    "right-from-bracket": faRightFromBracket,
    "right-to-bracket": faRightToBracket,
    square: faSquare,
    "square-check": faSquareCheck,
    star: faStar,
    stethoscope: faStethoscope,
    sun: faSun,
    syringe: faSyringe,
    "triangle-exclamation": faTriangleExclamation,
    user: faUser,
    "user-secret": faUserSecret,
    "user-group": faUserGroup,
    vial: faVial,
    "x-ray": faXRay,
    xmark: faXmark,
};

const regular: Record<string, IconDefinition> = {
    "circle-question": faCircleQuestionRegular,
    comment: faCommentRegular,
    square: faSquareRegular,
    "square-check": faSquareCheckRegular,
    star: faStarRegular,
};

export const webIconNames = {
    solid: Object.keys(solid),
    regular: Object.keys(regular),
};

export function Icon({
    name,
    solid: isSolid = true,
    size = 24,
    color = "black",
    style,
    fixedWidth,
    accessibilityLabel,
    ...rest
}: Props) {
    const def = (isSolid ? solid : regular)[name];
    // decorative unless named: the text or labelled button beside it says what it means (WCAG 1.1.1)
    const semantics = accessibilityLabel
        ? { role: "img", "aria-label": accessibilityLabel }
        : { "aria-hidden": true };
    if (!def) {
        logger.warn(
            `Icon "${name}" has no SVG mapping on web; using the icon font`
        );
        return (
            <FontAwesome6
                name={name}
                solid={isSolid}
                size={size}
                color={color}
                style={style}
                {...rest}
                {...(semantics as object)}
            />
        );
    }
    const [width, height, , , path] = def.icon;
    const d = Array.isArray(path) ? path.join(" ") : path;
    // fixed width: a viewBox 1.25 × the glyph's height wide, the glyph centred in it
    const boxWidth = fixedWidth ? height * 1.25 : width;
    return (
        <Svg
            width={fixedWidth ? size * 1.25 : size}
            height={size}
            viewBox={`${(width - boxWidth) / 2} 0 ${boxWidth} ${height}`}
            style={style}
            // like Font Awesome's own SVGs: Cypress selects icons by `[data-icon=chevron-right]`
            {...({ "data-icon": name } as object)}
            {...rest}
            {...(semantics as object)}
        >
            <Path d={d} fill={color as string} />
        </Svg>
    );
}
