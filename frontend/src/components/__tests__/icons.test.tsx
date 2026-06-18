import type { ReactElement } from 'react';
import Svg, { Path } from 'react-native-svg';

import type { IconProps } from '../icons';
import { ChartBar, Lightning, Plus, User } from '../icons';

/**
 * The tab bar / quest icons are local Phosphor SVG components (no icon font).
 * These tests assert each one emits a sized <Svg> wrapping a single <Path>
 * with real geometry — the failure mode that previously left the glyphs blank
 * when they were icon-font based.
 *
 * The icons are pure, hook-free function components, so we render them to their
 * element tree directly. That keeps the assertions deterministic and avoids
 * react-native-svg's native layout, which is unavailable under jest.
 */
type IconComponent = (props: IconProps) => ReactElement;
type PathEl = ReactElement<{ d: string; fill: string }>;
type SvgEl = ReactElement<{
  width: number;
  height: number;
  viewBox: string;
  children: PathEl;
}>;

function render(Icon: IconComponent, props: IconProps = {}) {
  const svg = Icon(props) as SvgEl;
  return { svg, path: svg.props.children };
}

describe('icons', () => {
  it('renders a sized Svg wrapping a single Path by default', () => {
    const { svg, path } = render(ChartBar);

    expect(svg.type).toBe(Svg);
    expect(svg.props.width).toBe(24);
    expect(svg.props.height).toBe(24);
    expect(svg.props.viewBox).toBe('0 0 256 256');

    expect(path.type).toBe(Path);
    expect(typeof path.props.d).toBe('string');
    expect(path.props.d).toMatch(/^M/); // SVG path data starts with a moveto
  });

  it('forwards size and color to the SVG', () => {
    const { svg, path } = render(User, { size: 32, color: '#ff0000' });

    expect(svg.props.width).toBe(32);
    expect(svg.props.height).toBe(32);
    expect(path.props.fill).toBe('#ff0000');
  });

  it('switches to the fill geometry when weight="fill"', () => {
    const regular = render(ChartBar, { weight: 'regular' }).path.props.d;
    const fill = render(ChartBar, { weight: 'fill' }).path.props.d;

    expect(fill).not.toBe(regular);
  });

  it('falls back to the regular geometry for icons with no fill variant', () => {
    // Plus only defines a regular weight; asking for fill must not crash.
    const regular = render(Plus, { weight: 'regular' }).path.props.d;
    const fill = render(Plus, { weight: 'fill' }).path.props.d;

    expect(fill).toBe(regular);
  });

  it('renders every tab icon with valid path data', () => {
    for (const Icon of [ChartBar, Lightning, User] as IconComponent[]) {
      expect(render(Icon).path.props.d).toMatch(/^M/);
    }
  });
});
