/**
 * Regenerates the PNG app assets from the master SVGs in src/assets/.
 * Run after editing any of the SVG sources: npm run generate-assets
 */
const path = require('path');
const sharp = require('sharp');

const ASSETS = path.join(__dirname, '..', 'src', 'assets');

const TARGETS = [
  { source: 'logo.svg', output: 'icon.png', size: 1024 },
  { source: 'logo.svg', output: 'favicon.png', size: 48 },
  { source: 'adaptive-icon.svg', output: 'adaptive-icon.png', size: 1024 },
  { source: 'splash-icon.svg', output: 'splash-icon.png', size: 1024 },
];

async function main() {
  for (const { source, output, size } of TARGETS) {
    const from = path.join(ASSETS, source);
    const to = path.join(ASSETS, output);
    await sharp(from, { density: Math.max(72, (size / 1024) * 72) })
      .resize(size, size)
      .png()
      .toFile(to);
    console.log(`${source} -> ${output} (${size}x${size})`);
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
