import sharp from 'sharp';
import { readFileSync } from 'fs';
import { join, dirname } from 'path';
import { fileURLToPath } from 'url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const assetsDir = join(__dirname, 'divoom.app', 'Assets');

const icons = ['AppIcon-dark', 'AppIcon-light'];

for (const name of icons) {
  const svg = readFileSync(join(assetsDir, `${name}.svg`));
  await sharp(svg).resize(48, 48).png().toFile(join(assetsDir, `${name}.png`));
  console.log(`${name}.png written at 48x48`);
}
